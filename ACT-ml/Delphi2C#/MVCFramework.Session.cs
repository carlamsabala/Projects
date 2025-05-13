using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MVCFramework.Session
{
    public class MVCSessionException : Exception
    {
        public MVCSessionException() { }
        public MVCSessionException(string message) : base(message) { }
        public MVCSessionException(string message, Exception innerException) : base(message, innerException) { }
    }

    
    public abstract class MVCWebSession
    {
        protected bool _changed;
        protected DateTime? _expirationTimeStamp;
        protected MVCWebSessionFactory _sessionFactory;

        protected MVCWebSession(MVCWebSessionFactory ownerFactory)
        {
            _sessionFactory = ownerFactory;
            _changed = false;
        }

        
        public abstract string GetItem(string key);
        public virtual void SetItem(string key, string value)
        {
            _changed = true;
        }

       
        protected virtual void InternalApplyChanges() { }

       
        public virtual DateTime? ExpirationTimeStamp
        {
            get { return _expirationTimeStamp; }
            set { _expirationTimeStamp = value; }
        }

        
        public string SessionId { get; protected set; }

        
        public int Timeout { get; set; }

        public MVCWebSessionFactory SessionFactory => _sessionFactory;

        
        public void MarkAsUsed()
        {
            _changed = true;
            RefreshSessionExpiration();
        }

        
        public void ApplyChanges()
        {
            if (_changed)
            {
                InternalApplyChanges();
                _changed = false;
            }
        }

        public override string ToString()
        {
            return string.Join(",", Keys());
        }

        
        public abstract string[] Keys();

        
        public virtual void StopSession() { }

        
        public virtual void RefreshSessionExpiration()
        {
            if (Timeout > 0)
                _expirationTimeStamp = DateTime.Now.AddMinutes(Timeout);
            else
                _expirationTimeStamp = null;
        }

        
        public abstract MVCWebSession Clone();

        
        public virtual string this[string key]
        {
            get { return GetItem(key); }
            set { SetItem(key, value); }
        }

        
        public bool IsExpired()
        {
            if (_expirationTimeStamp.HasValue)
            {
                return _expirationTimeStamp.Value < DateTime.Now;
            }
            return false;
        }
    }

    
    public abstract class MVCWebSessionFactory
    {
        protected int _timeoutInMinutes;

        public const int DEFAULT_SESSION_INACTIVITY = 60;

        public MVCWebSessionFactory(int timeoutInMinutes = DEFAULT_SESSION_INACTIVITY)
        {
            _timeoutInMinutes = timeoutInMinutes;
        }

        public int TimeoutInMinutes => _timeoutInMinutes;

        public abstract MVCWebSession CreateNewSession(string sessionId);
        public abstract MVCWebSession CreateFromSessionID(string sessionId);
        public abstract bool TryFindSessionID(string sessionId);
        public abstract void TryDeleteSessionID(string sessionId);
    }

    #region In‐Memory Sessions

   
    public class MVCWebSessionMemory : MVCWebSession
    {
        private Dictionary<string, string> _data;

        public MVCWebSessionMemory(MVCWebSessionFactory ownerFactory) : base(ownerFactory)
        {
            _data = new Dictionary<string, string>();
        }

        public override string GetItem(string key)
        {
            return _data.TryGetValue(key, out string value) ? value : string.Empty;
        }

        public override void SetItem(string key, string value)
        {
            base.SetItem(key, value);
            _data[key] = value;
        }

        public override string[] Keys()
        {
            return _data.Keys.ToArray();
        }

        public override MVCWebSession Clone()
        {
            var clone = new MVCWebSessionMemory(_sessionFactory);
            clone.SessionId = this.SessionId;
            clone.Timeout = this.Timeout;
            foreach (var kvp in _data)
            {
                clone._data[kvp.Key] = kvp.Value;
            }
            return clone;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _data.Select(kvp => $"{kvp.Key}='{kvp.Value}'"));
        }

        protected override void InternalApplyChanges()
        {
            
            lock (MVCWebSessionMemoryFactory.GlobalSessionListLock)
            {
                MVCWebSessionMemoryFactory.GlobalSessionList[SessionId] = (MVCWebSessionMemory)this.Clone();
            }
        }
    }

    
    public class MVCWebSessionMemoryFactory : MVCWebSessionFactory
    {
        
        internal static Dictionary<string, MVCWebSessionMemory> GlobalSessionList = new Dictionary<string, MVCWebSessionMemory>();
        internal static readonly object GlobalSessionListLock = new object();
        private static DateTime _lastSessionListClear = DateTime.Now;

        public MVCWebSessionMemoryFactory(int timeoutInMinutes = DEFAULT_SESSION_INACTIVITY)
            : base(timeoutInMinutes)
        {
        }

        private void ClearExpiredSessions()
        {
            lock (GlobalSessionListLock)
            {
                if ((DateTime.Now - _lastSessionListClear).TotalMinutes >= 1)
                {
                    var expiredKeys = GlobalSessionList
                        .Where(pair => pair.Value.IsExpired())
                        .Select(pair => pair.Key)
                        .ToList();
                    foreach (var key in expiredKeys)
                    {
                        GlobalSessionList.Remove(key);
                    }
                    _lastSessionListClear = DateTime.Now;
                }
            }
        }

        public override MVCWebSession CreateNewSession(string sessionId)
        {
            lock (GlobalSessionListLock)
            {
                ClearExpiredSessions();
                var newSession = new MVCWebSessionMemory(this)
                {
                    SessionId = sessionId,
                    Timeout = _timeoutInMinutes
                };
                newSession.MarkAsUsed();
                GlobalSessionList[sessionId] = newSession;
                return newSession.Clone();
            }
        }

        public override MVCWebSession CreateFromSessionID(string sessionId)
        {
            lock (GlobalSessionListLock)
            {
                ClearExpiredSessions();
                if (GlobalSessionList.TryGetValue(sessionId, out MVCWebSessionMemory session))
                {
                    session.Timeout = _timeoutInMinutes;
                    return session.Clone();
                }
                return null;
            }
        }

        public override bool TryFindSessionID(string sessionId)
        {
            lock (GlobalSessionListLock)
            {
                ClearExpiredSessions();
                return GlobalSessionList.ContainsKey(sessionId);
            }
        }

        public override void TryDeleteSessionID(string sessionId)
        {
            lock (GlobalSessionListLock)
            {
                GlobalSessionList.Remove(sessionId);
            }
        }
    }

    #endregion

    #region File-based Sessions

    
    public class MVCWebSessionFile : MVCWebSession
    {
        private Dictionary<string, string> _data;
        private string _sessionFolder;

        public MVCWebSessionFile(MVCWebSessionFactory ownerFactory, string sessionFolder)
            : base(ownerFactory)
        {
            _data = new Dictionary<string, string>();
            _sessionFolder = sessionFolder;
        }

        public override string GetItem(string key)
        {
            return _data.TryGetValue(key, out string value) ? value : string.Empty;
        }

        public override void SetItem(string key, string value)
        {
            base.SetItem(key, value);
            _data[key] = value;
        }

        public override string[] Keys()
        {
            return _data.Keys.ToArray();
        }

        public override MVCWebSession Clone()
        {
            var clone = new MVCWebSessionFile(_sessionFactory, _sessionFolder);
            clone.SessionId = this.SessionId;
            clone.Timeout = this.Timeout;
            foreach (var kvp in _data)
            {
                clone._data[kvp.Key] = kvp.Value;
            }
            return clone;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _data.Select(kvp => $"{kvp.Key}='{kvp.Value}'"));
        }

        protected override void InternalApplyChanges()
        {
           
            lock (MVCWebSessionFileFactory.GlobalFileLock)
            {
                SaveToFile();
            }
        }

        
        protected void LoadFromFile()
        {
            string fileName = MVCWebSessionFileFactory.GetSessionFileNameStatic(_sessionFactory, SessionId, _sessionFolder);
            if (!File.Exists(fileName))
                return;

            using (var reader = new StreamReader(fileName))
            {
                
                string expLine = reader.ReadLine();
                _expirationTimeStamp = ISOTimeStampToDateTime(expLine);
                string timeoutLine = reader.ReadLine();
                Timeout = int.Parse(timeoutLine);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    var parts = line.Split('=');
                    if (parts.Length >= 2)
                        SetItem(parts[0], parts[1]);
                }
            }
        }

        
        protected void SaveToFile()
        {
            MarkAsUsed();
            string fileName = MVCWebSessionFileFactory.GetSessionFileNameStatic(_sessionFactory, SessionId, _sessionFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            using (var writer = new StreamWriter(fileName, false))
            {
                writer.WriteLine(DateTimeToISOTimeStamp(ExpirationTimeStamp));
                writer.WriteLine(Timeout);
                foreach (var kvp in _data)
                {
                    writer.WriteLine($"{kvp.Key}={kvp.Value}");
                }
            }
        }
    }

    
    public class MVCWebSessionFileFactory : MVCWebSessionFactory
    {
        private string _sessionFolder;

        public MVCWebSessionFileFactory(int timeoutInMinutes = DEFAULT_SESSION_INACTIVITY, string sessionFolder = "dmvc_sessions")
            : base(timeoutInMinutes)
        {
            _sessionFolder = GetSessionFolder(sessionFolder);
        }

        private string GetSessionFolder(string path)
        {
            if (!Path.IsPathRooted(path))
                return Path.Combine(AppPath.Value, path);
            else
                return path;
        }

        
        public static string GetSessionFileNameStatic(MVCWebSessionFactory factory, string sessionId, string folder)
        {
            string fullFolder = folder;
            if (!Path.IsPathRooted(folder))
                fullFolder = Path.Combine(AppPath.Value, folder);
            return Path.Combine(fullFolder, sessionId);
        }

        public override MVCWebSession CreateNewSession(string sessionId)
        {
            var session = new MVCWebSessionFile(this, _sessionFolder)
            {
                SessionId = sessionId,
                Timeout = _timeoutInMinutes
            };
            session.MarkAsUsed();
            session.RefreshSessionExpiration();
            (session as MVCWebSessionFile).SaveToFile();
            return session;
        }

        public override MVCWebSession CreateFromSessionID(string sessionId)
        {
            var session = new MVCWebSessionFile(this, _sessionFolder)
            {
                SessionId = sessionId,
                Timeout = _timeoutInMinutes
            };
            (session as MVCWebSessionFile).LoadFromFile();
            return session;
        }

        public override bool TryFindSessionID(string sessionId)
        {
            string fileName = GetSessionFileNameStatic(this, sessionId, _sessionFolder);
            return File.Exists(fileName);
        }

        public override void TryDeleteSessionID(string sessionId)
        {
            string fileName = GetSessionFileNameStatic(this, sessionId, _sessionFolder);
            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Cannot delete session file {fileName}: {ex.Message}");
                }
            }
        }
    }

    #endregion

    #region Application Path Helper

    public static class AppPath
    {
        public static string Value => AppDomain.CurrentDomain.BaseDirectory;
    }

    #endregion
}
