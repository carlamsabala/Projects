
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices; 

namespace LoggerPro.Appenders
{
    
    public delegate void OnCreateJSONData(object sender, TLogItem logItem, LoggerProExtendedInfo extendedInfo, JObject data);

    
    public delegate void OnNetSendError(object sender, TLogItem logItem, Exception netError, ref int retryCount);

    
    public delegate T GetStoredProc<T>(IDbConnection connection) where T : class;

    
    public delegate void SetParams<T>(T dataObject, TLogItem logItem) where T : class;

    
    public enum DMSQueueAggregationType
    {
        ByTag,
        ByType,
        ByTagThenType,
        ByTypeThenTag
    }

    
    public abstract class LoggerProDMSContainerAppender<T> : LoggerProAppenderBase, ILogAppender where T : class
    {
        protected const int MAX_RETRY_COUNT = 5;

        protected OnCreateJSONData _onCreateJSONData;
        protected OnNetSendError _onNetSendError;
        protected LoggerProExtendedInfo _extendedInfo;
        
        protected Dictionary<LogExtendedInfo, string> _extendedInfoData = new Dictionary<LogExtendedInfo, string>();
        protected TEventStreamsRPCProxy _eventStreamsProxy;
        protected string _dmsContainerAPIKey;
        protected string _queueNameBase;
        protected DMSQueueAggregationType _logItemAggregationType;

        
        public OnCreateJSONData OnCreateJSONData
        {
            get => _onCreateJSONData;
            set => _onCreateJSONData = value;
        }

        public OnNetSendError OnNetSendError
        {
            get => _onNetSendError;
            set => _onNetSendError = value;
        }

        
        public LoggerProDMSContainerAppender(
            TEventStreamsRPCProxy eventStreamsProxy,
            string dmsContainerAPIKey,
            string eventStreamsQueueNameBase = "queues.logs.",
            DMSQueueAggregationType logItemAggregationType = DMSQueueAggregationType.ByTag,
            LoggerProExtendedInfo logExtendedInfo = LoggerProExtendedInfo.Default)
        {
            _eventStreamsProxy = eventStreamsProxy ?? throw new ArgumentNullException(nameof(eventStreamsProxy));
            _dmsContainerAPIKey = dmsContainerAPIKey;
            _queueNameBase = eventStreamsQueueNameBase.EndsWith(".") ? eventStreamsQueueNameBase : eventStreamsQueueNameBase + ".";
            _logItemAggregationType = logItemAggregationType;
            _extendedInfo = logExtendedInfo;
            LoadExtendedInfo();
        }

        
        protected abstract void RefreshParams(T dataObject);
        protected abstract void ExecuteDataObject(T dataObject);

        
        public override void Setup()
        {
            base.Setup();
            
        }

        public override void TearDown()
        {
            base.TearDown();
            if (_eventStreamsProxy != null)
            {
                _eventStreamsProxy.Dispose();
                _eventStreamsProxy = null;
            }
        }

        public override void TryToRestart(out bool restarted)
        {
            restarted = false;
            try
            {
                
                if (DbObject != null)
                {
                    if (DbObject is IDisposable disp)
                    {
                        disp.Dispose();
                    }
                    DbObject = null;
                }
            }
            catch
            {
                // Ignore any errors during cleanup.
            }
            
            restarted = true;
        }

        
        protected T DbObject;

        
        protected virtual JObject GetDefaultLog(TLogItem logItem)
        {
            var jo = new JObject();
            jo["timestamp"] = logItem.TimeStamp.ToString("o"); 
            jo["tid"] = logItem.ThreadID;
            jo["type"] = logItem.LogTypeAsString;
            jo["text"] = logItem.LogMessage;
            jo["info"] = GetExtendedInfo();
            
            return jo;
        }

        
        protected virtual JObject GetExtendedInfo()
        {
            var jo = new JObject();
#if WINDOWS
            if (_extendedInfo.HasFlag(LogExtendedInfo.UserName) && _extendedInfoData.ContainsKey(LogExtendedInfo.UserName))
                jo["username"] = _extendedInfoData[LogExtendedInfo.UserName];
            if (_extendedInfo.HasFlag(LogExtendedInfo.ComputerName) && _extendedInfoData.ContainsKey(LogExtendedInfo.ComputerName))
                jo["computername"] = _extendedInfoData[LogExtendedInfo.ComputerName];
            if (_extendedInfo.HasFlag(LogExtendedInfo.ProcessName) && _extendedInfoData.ContainsKey(LogExtendedInfo.ProcessName))
                jo["processname"] = _extendedInfoData[LogExtendedInfo.ProcessName];
            if (_extendedInfo.HasFlag(LogExtendedInfo.ProcessID) && _extendedInfoData.ContainsKey(LogExtendedInfo.ProcessID))
                jo["pid"] = _extendedInfoData[LogExtendedInfo.ProcessID];
#elif ANDROID
            if (_extendedInfo.HasFlag(LogExtendedInfo.ProcessName) && _extendedInfoData.ContainsKey(LogExtendedInfo.ProcessName))
                jo["processname"] = _extendedInfoData[LogExtendedInfo.ProcessName];
#endif
            return jo;
        }

        
        public virtual JObject CreateData(TLogItem logItem)
        {
            JObject data;
            try
            {
                if (OnCreateJSONDataCallback != null)
                {
                    data = new JObject();
                    OnCreateJSONDataCallback(this, logItem, _extendedInfo, data);
                }
                else
                {
                    data = GetDefaultLog(logItem);
                }
            }
            catch
            {
                throw;
            }
            return data;
        }

        
        public override void WriteLog(TLogItem logItem)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    string queueName;
                    switch (_logItemAggregationType)
                    {
                        case DMSQueueAggregationType.ByTag:
                            queueName = _queueNameBase + logItem.LogTag;
                            break;
                        case DMSQueueAggregationType.ByType:
                            queueName = _queueNameBase + logItem.LogTypeAsString;
                            break;
                        case DMSQueueAggregationType.ByTagThenType:
                            queueName = _queueNameBase + logItem.LogTag + "." + logItem.LogTypeAsString;
                            break;
                        case DMSQueueAggregationType.ByTypeThenTag:
                            queueName = _queueNameBase + logItem.LogTypeAsString + "." + logItem.LogTag;
                            break;
                        default:
                            throw new Exception("Invalid Aggregation type");
                    }
                    
                    var data = CreateData(logItem);
                    
                    using (var response = _eventStreamsProxy.EnqueueMessage(_dmsContainerAPIKey, queueName, data))
                    {
                        // Optionally, process the response.
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (OnNetSendErrorCallback != null)
                    {
                        OnNetSendErrorCallback(this, logItem, ex, ref retryCount);
                    }
                    retryCount++;
                    if (retryCount >= MAX_RETRY_COUNT)
                    {
                        break;
                    }
                }
            }
        }

        
        public static string GetModuleBaseName()
        {
#if WINDOWS
            return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
#elif ANDROID
            
            return "AndroidApp";
#else
            throw new NotSupportedException("Current platform not supported by LoggerProDMSContainerAppender");
#endif
        }

        
        protected virtual void LoadExtendedInfo()
        {
#if WINDOWS
            if (_extendedInfo.HasFlag(LogExtendedInfo.ProcessID))
                _extendedInfoData[LogExtendedInfo.ProcessID] = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            if (_extendedInfo.HasFlag(LogExtendedInfo.UserName))
                _extendedInfoData[LogExtendedInfo.UserName] = Environment.UserName;
            if (_extendedInfo.HasFlag(LogExtendedInfo.ComputerName))
                _extendedInfoData[LogExtendedInfo.ComputerName] = Environment.MachineName;
            if (_extendedInfo.HasFlag(LogExtendedInfo.ProcessName))
                _extendedInfoData[LogExtendedInfo.ProcessName] = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
#elif ANDROID
            if (_extendedInfo.HasFlag(LogExtendedInfo.ProcessName))
                _extendedInfoData[LogExtendedInfo.ProcessName] = "AndroidApp"; 
#endif
        }
    }
}
