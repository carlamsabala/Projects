
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MVCFramework.DotEnv
{
    #region Exceptions and Enums

    
    public class DotEnvException : Exception
    {
        public DotEnvException(string message) : base(message) { }
    }

    public enum DotEnvPriority
    {
        FileThenEnv,
        EnvThenFile,
        OnlyFile,
        OnlyEnv
    }

    #endregion

    #region Interfaces

    public interface IMVCDotEnv
    {
        string Env(string name);
        string Env(string name, string defaultValue);
        int Env(string name, int defaultValue);
        bool Env(string name, bool defaultValue);
        void RequireKeys(string[] keys);
        IMVCDotEnv SaveToFile(string fileName);
        string[] ToArray();
    }

    public interface IMVCDotEnvBuilder : IMVCDotEnv
    {
        IMVCDotEnvBuilder UseStrategy(DotEnvPriority strategy = DotEnvPriority.EnvThenFile);
        IMVCDotEnvBuilder SkipDefaultEnv();
        IMVCDotEnvBuilder UseLogger(Action<string> logger);
        IMVCDotEnvBuilder UseProfile(string profileName);
        IMVCDotEnvBuilder UseProfile(Func<string> profileDelegate);
        IMVCDotEnvBuilder ClearProfiles();
        IMVCDotEnv Build(string dotEnvDirectory = "");
    }

    #endregion

    #region Public Static API

    public static class DotEnv
    {
        private static IMVCDotEnvBuilder _instance;

        public static IMVCDotEnvBuilder NewDotEnv()
        {
            if (_instance == null)
            {
                _instance = new MVCDotEnv();
            }
            return _instance;
        }
    }

    #endregion

    #region Implementation

    internal class MVCDotEnv : IMVCDotEnvBuilder
    {
        #region Private Types and Fields

        private enum DotEnvEngineState { Created, Building, Built }

        private DotEnvEngineState _state;
        private DotEnvPriority _priority;
        private string _envPath;
        private readonly Dictionary<string, string> _envDict;
        private Action<string> _loggerProc;
        private readonly List<string> _profiles;
        private bool _skipDefaultEnv;

        #endregion

        #region Constructor / Destructor

        public MVCDotEnv()
        {
            _state = DotEnvEngineState.Created;
            _profiles = new List<string>();
            _envDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _envPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
            _envPath = Path.EndsInDirectorySeparator(_envPath) ? _envPath : _envPath + Path.DirectorySeparatorChar;
            _priority = DotEnvPriority.EnvThenFile;
            _skipDefaultEnv = false;
        }

        #endregion

        #region Private Helper Methods

        private void DoLog(string value)
        {
            _loggerProc?.Invoke(value);
        }

        private void CheckAlreadyBuilt()
        {
            if (_state == DotEnvEngineState.Built)
            {
                throw new Exception("DotEnv Engine Already Built");
            }
        }

        private void PopulateDictionary(Dictionary<string, string> envDict, string envFilePath)
        {
            if (!File.Exists(envFilePath))
            {
                DoLog($"Missed file {envFilePath}");
                return;
            }

            string dotenvCode = File.ReadAllText(envFilePath);
            DotEnvParser.Parse(envDict, dotenvCode);
            DoLog($"Applied file {envFilePath}");
        }

        private void ReadEnvFile()
        {
            if (!_skipDefaultEnv)
            {
                PopulateDictionary(_envDict, Path.Combine(_envPath, ".env"));
            }
            foreach (var profile in _profiles)
            {
                string profileEnvPath = Path.Combine(_envPath, $".env.{profile}");
                PopulateDictionary(_envDict, profileEnvPath);
            }
        }

        private string ExplodePlaceholders(string value)
        {
            string result = value;
            while (result.Contains("${"))
            {
                int startPos = result.IndexOf("${", StringComparison.Ordinal);
                int endPos = result.IndexOf("}", startPos, StringComparison.Ordinal);
                if (endPos == -1 || endPos < startPos)
                {
                    throw new DotEnvException("Unclosed expansion (${...}) at: " + value);
                }
                string key = result.Substring(startPos + 2, endPos - (startPos + 2));

                string replacement = "";
                if (key.StartsWith("__") && key.EndsWith("__"))
                {
                    if (!GetBuiltInVariable(key, out replacement))
                    {
                        replacement = Env(key);
                    }
                }
                else
                {
                    replacement = Env(key);
                }
                result = result.Replace($"${{{key}}}", replacement);
            }
            return result;
        }

        private void ExplodeReferences()
        {
            foreach (var key in _envDict.Keys.ToList())
            {
                _envDict[key] = ExplodePlaceholders(_envDict[key]);
            }
        }

        private bool GetBuiltInVariable(string varName, out string value)
        {
            string lVarName = varName.ToLower(); 
            if (lVarName == "__os__")
            {
                value = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
                return true;
            }
            if (lVarName == "__home__")
            {
                value = AppPath();
                return true;
            }
            if (lVarName == "__dmvc.version__")
            {
                value = DMVCFRAMEWORK_VERSION();
                return true;
            }
            DoLog("Unknown built-in env var named " + lVarName + ".");
            value = "";
            return false;
        }

        private string AppPath()
        {
            return _envPath;
        }

        private string DMVCFRAMEWORK_VERSION()
        {
            return "6.0.0";
        }

        private string GetDotEnvVar(string key)
        {
            _envDict.TryGetValue(key, out string val);
            return val ?? "";
        }

        #endregion

        #region IMVCDotEnv Implementation

        public string Env(string name)
        {
            if (_state == DotEnvEngineState.Created)
            {
                throw new DotEnvException("dotEnv Engine not built");
            }

            string result = "";
            if (_priority == DotEnvPriority.FileThenEnv || _priority == DotEnvPriority.OnlyFile)
            {
                result = GetDotEnvVar(name);
                if (result.Contains($"${{{name}}}"))
                {
                    throw new DotEnvException($"Configuration loop detected with key \"{name}\"");
                }
                if (_priority == DotEnvPriority.OnlyFile)
                {
                    return result;
                }
                if (string.IsNullOrEmpty(result))
                {
                    result = ExplodePlaceholders(Environment.GetEnvironmentVariable(name) ?? "");
                    return result;
                }
                return result;
            }
            else if (_priority == DotEnvPriority.EnvThenFile || _priority == DotEnvPriority.OnlyEnv)
            {
                result = ExplodePlaceholders(Environment.GetEnvironmentVariable(name) ?? "");
                if (_priority == DotEnvPriority.OnlyEnv)
                {
                    return result;
                }
                if (string.IsNullOrEmpty(result))
                {
                    string tmp = GetDotEnvVar(name);
                    if (tmp.Contains($"${{{name}}}"))
                    {
                        throw new DotEnvException($"Configuration loop detected with key \"{name}\"");
                    }
                    return tmp;
                }
                return result;
            }
            else
            {
                throw new DotEnvException($"Unknown dotEnv Priority: {_priority}");
            }
        }

        public string Env(string name, string defaultValue)
        {
            string val = Env(name);
            return string.IsNullOrEmpty(val) ? defaultValue : val;
        }

        public int Env(string name, int defaultValue)
        {
            string val = Env(name);
            if (string.IsNullOrEmpty(val))
                return defaultValue;
            if (!int.TryParse(val.Trim(), out int result))
            {
                throw new DotEnvException($"Env \"{name}\" is not a valid integer [Current Value: \"{val}\"]");
            }
            return result;
        }

        public bool Env(string name, bool defaultValue)
        {
            string val = Env(name);
            if (string.IsNullOrEmpty(val))
                return defaultValue;
            val = val.Trim().ToLower();
            if (val == "yes" || val == "1" || val == "true")
                return true;
            if (val == "no" || val == "0" || val == "false")
                return false;
            if (!bool.TryParse(val, out bool result))
            {
                throw new DotEnvException($"Env \"{name}\" is not a valid boolean [Current Value: \"{val}\"]");
            }
            return result;
        }

        public void RequireKeys(string[] keys)
        {
            if (keys == null || keys.Length == 0) return;
            DoLog("Checking required keys: " + string.Join(", ", keys));
            List<string> notFoundKeys = new List<string>();
            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty(Env(key)))
                    notFoundKeys.Add(key);
            }
            if (notFoundKeys.Any())
            {
                string msg = "Required keys not found: " + string.Join(", ", notFoundKeys);
                DoLog(msg);
                throw new DotEnvException(msg);
            }
        }

        public IMVCDotEnv SaveToFile(string fileName)
        {
            var keys = _envDict.Keys.ToList();
            keys.Sort(StringComparer.OrdinalIgnoreCase);
            StringBuilder sb = new StringBuilder();
            foreach (var key in keys)
            {
                sb.AppendLine($"{key}={GetDotEnvVar(key)}");
            }
            File.WriteAllText(fileName, sb.ToString());
            return this;
        }

        public string[] ToArray()
        {
            var keys = _envDict.Keys.ToList();
            keys.Sort(StringComparer.OrdinalIgnoreCase);
            return keys.Select(key => $"{key}={GetDotEnvVar(key)}").ToArray();
        }

        #endregion

        #region IMVCDotEnvBuilder Implementation

        public IMVCDotEnvBuilder UseStrategy(DotEnvPriority strategy = DotEnvPriority.EnvThenFile)
        {
            CheckAlreadyBuilt();
            _priority = strategy;
            return this;
        }

        public IMVCDotEnvBuilder SkipDefaultEnv()
        {
            _skipDefaultEnv = true;
            return this;
        }

        public IMVCDotEnvBuilder UseLogger(Action<string> logger)
        {
            if (_loggerProc != null)
                throw new DotEnvException("Logger already set");
            _loggerProc = logger;
            return this;
        }

        public IMVCDotEnvBuilder UseProfile(string profileName)
        {
            CheckAlreadyBuilt();
            _profiles.Add(profileName);
            return this;
        }

        public IMVCDotEnvBuilder UseProfile(Func<string> profileDelegate)
        {
            CheckAlreadyBuilt();
            _profiles.Add(profileDelegate());
            return this;
        }

        public IMVCDotEnvBuilder ClearProfiles()
        {
            CheckAlreadyBuilt();
            _profiles.Clear();
            return this;
        }

        public IMVCDotEnv Build(string dotEnvDirectory = "")
        {
            if (_state != DotEnvEngineState.Created)
            {
                throw new DotEnvException("dotEnv engine already built");
            }
            _state = DotEnvEngineState.Building;
            _envPath = string.IsNullOrEmpty(dotEnvDirectory)
                ? _envPath
                : Path.Combine(_envPath, dotEnvDirectory);
            DoLog("Path = " + _envPath);
            _envDict.Clear();

            List<string> allProfiles = new List<string> { "default" };
            allProfiles.AddRange(_profiles);
            if (_skipDefaultEnv && allProfiles.Count > 0)
            {
                allProfiles.RemoveAt(0);
            }
            DoLog("Active profile/s priority = [" + string.Join(",", allProfiles) +
                $"] (Priority: {_priority})");
            ReadEnvFile();
            ExplodeReferences();
            _state = DotEnvEngineState.Built;
            return this;
        }

        #endregion

    }

    #endregion

    #region Simple DotEnv Parser

    public static class DotEnvParser
    {
        public static void Parse(Dictionary<string, string> envDict, string dotenvCode)
        {
            if (string.IsNullOrWhiteSpace(dotenvCode))
                return;
            var lines = dotenvCode.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed))
                    continue;
                int idx = trimmed.IndexOf('=');
                if (idx > 0)
                {
                    string key = trimmed.Substring(0, idx).Trim();
                    string value = trimmed.Substring(idx + 1).Trim();
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    envDict[key] = value;
                }
            }
        }
    }

    #endregion
}
