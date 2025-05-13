using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Horse.Core.Param.Field
{
    #region Dependent Stubs

    public class HorseException : Exception
    {
        public int Status { get; set; }
        public HorseException(string message) : base(message) { }
        public static HorseException New(int status, string message)
        {
            return new HorseException(message) { Status = status };
        }
    }

    public static class HTTPStatus
    {
        public const int BadRequest = 400;
    }

    public class HorseCoreParamConfig
    {
        private static HorseCoreParamConfig _instance;
        public bool CheckLhsBrackets { get; set; } = false;
        private HorseCoreParamConfig() { }
        public static HorseCoreParamConfig GetInstance()
        {
            if (_instance == null)
                _instance = new HorseCoreParamConfig();
            return _instance;
        }
    }

    public enum TLhsBracketsType
    {
        Square, 
        Curly   
        
    }

    
    public class HorseCoreParamFieldLhsBrackets
    {
        public List<TLhsBracketsType> Types { get; } = new List<TLhsBracketsType>();
        private readonly Dictionary<TLhsBracketsType, string> _values = new Dictionary<TLhsBracketsType, string>();
        public void SetValue(TLhsBracketsType type, string value)
        {
            _values[type] = value;
        }
    }

    #endregion

    public class HorseCoreParamField : IDisposable
    {
        
        private bool FContains;
        private string FFieldName;
        private bool FRequired;
        private string FRequiredMessage;
        private string FInvalidFormatMessage;
        private string FDateFormat;
        private string FTimeFormat;
        private bool FReturnUTC;
        private string FTrueValue;
        private string FValue;
        private Stream FStream;
        private HorseCoreParamFieldLhsBrackets FLhsBrackets;

        
        public HorseCoreParamField(Dictionary<string, string> AParams, string AFieldName)
        {
            FFieldName = AFieldName;
            FValue = "";
            FRequired = false;
            FContains = false;
            
            foreach (var key in AParams.Keys)
            {
                if (string.Equals(key, FFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    FContains = true;
                    FValue = AParams[key];
                    break;
                }
            }
            InitializeLhsBrackets(AParams, AFieldName);
        }

       
        public HorseCoreParamField(Stream AStream, string AFieldName)
        {
            FFieldName = AFieldName;
            FValue = "";
            FRequired = false;
            FContains = true;
            FStream = AStream;
        }

        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                
                FLhsBrackets = null;
                
            }
        }
        ~HorseCoreParamField() => Dispose(false);

        #region Fluent Setters and Getters

        public HorseCoreParamField DateFormat(string AValue)
        {
            FDateFormat = AValue;
            return this;
        }
        public string DateFormat() => FDateFormat;

        public HorseCoreParamField InvalidFormatMessage(string AValue)
        {
            FInvalidFormatMessage = AValue;
            return this;
        }
        public string InvalidFormatMessage() => FInvalidFormatMessage;

        public HorseCoreParamField Required()
        {
            FRequired = true;
            return this;
        }
        public HorseCoreParamField Required(bool AValue)
        {
            FRequired = AValue;
            return this;
        }

        public HorseCoreParamField RequiredMessage(string AValue)
        {
            FRequiredMessage = AValue;
            return this;
        }

        public HorseCoreParamField ReturnUTC(bool AValue)
        {
            FReturnUTC = AValue;
            return this;
        }

        public HorseCoreParamField TimeFormat(string AValue)
        {
            FTimeFormat = AValue;
            return this;
        }

        public HorseCoreParamField TrueValue(string AValue)
        {
            FTrueValue = AValue;
            return this;
        }

        
        public HorseCoreParamField CheckLhsBrackets(bool AValue)
        {
            
            return this;
        }

        
        public HorseCoreParamFieldLhsBrackets LhsBrackets => FLhsBrackets;

        #endregion

        #region Conversion Methods

        public bool AsBoolean()
        {
            string LStrParam = AsString().Trim();
            if (!string.IsNullOrEmpty(LStrParam))
                return LStrParam.ToLowerInvariant() == FTrueValue.ToLowerInvariant();
            return false;
        }

        public double AsCurrency() => AsFloat();

        public DateTime AsDate()
        {
            string LStrParam = AsString().Trim();
            if (string.IsNullOrEmpty(LStrParam))
                return default;
            try
            {
                var fs = GetFormatSettings();
                
                string datePart = LStrParam.Substring(0, Math.Min(FDateFormat.Length, LStrParam.Length));
                return DateTime.ParseExact(datePart, FDateFormat, fs);
            }
            catch (Exception)
            {
                RaiseHorseException(FInvalidFormatMessage, new object[] { FFieldName, LStrParam, "date" });
                return default;
            }
        }

        public DateTime AsDateTime()
        {
            string LStrParam = AsString().Trim();
            if (string.IsNullOrEmpty(LStrParam))
                return default;
            try
            {
                var fs = GetFormatSettings();
                return DateTime.Parse(LStrParam, fs);
            }
            catch (Exception)
            {
                RaiseHorseException(FInvalidFormatMessage, new object[] { FFieldName, LStrParam, "datetime" });
                return default;
            }
        }

        public double AsExtended() => AsFloat();

        public double AsFloat()
        {
            string LStrParam = AsString().Trim();
            try
            {
                if (string.IsNullOrEmpty(LStrParam))
                    return 0;
                var fs = GetFormatSettings();
                
                LStrParam = LStrParam.Replace(",", fs.NumberFormat.NumberDecimalSeparator)
                                   .Replace(".", fs.NumberFormat.NumberDecimalSeparator);
                return double.Parse(LStrParam, fs);
            }
            catch (Exception)
            {
                RaiseHorseException(FInvalidFormatMessage, new object[] { FFieldName, LStrParam, "numeric" });
                return 0;
            }
        }

        public long AsInt64()
        {
            string LStrParam = AsString().Trim();
            try
            {
                if (!string.IsNullOrEmpty(LStrParam))
                    return long.Parse(LStrParam);
                return 0;
            }
            catch (Exception)
            {
                RaiseHorseException(FInvalidFormatMessage, new object[] { FFieldName, LStrParam, "int64" });
                return 0;
            }
        }

        public int AsInteger()
        {
            string LStrParam = AsString().Trim();
            try
            {
                if (!string.IsNullOrEmpty(LStrParam))
                    return int.Parse(LStrParam);
                return 0;
            }
            catch (Exception)
            {
                RaiseHorseException(FInvalidFormatMessage, new object[] { FFieldName, LStrParam, "integer" });
                return 0;
            }
        }

        public DateTime AsISO8601DateTime()
        {
            string LStrParam = AsString().Trim();
            if (string.IsNullOrEmpty(LStrParam))
                return default;
            DateTime dt;
            if (!TryISO8601ToDate(LStrParam, out dt))
                RaiseHorseException(FInvalidFormatMessage, new object[] { FFieldName, LStrParam, "ISO8601 date" });
            return dt;
        }

        public Stream AsStream()
        {
            if (FContains)
            {
                if (FStream != null)
                    FStream.Position = 0;
                return FStream;
            }
            else if (FRequired)
            {
                RaiseHorseException(FRequiredMessage, new object[] { FFieldName });
            }
            return null;
        }

        public string AsString()
        {
            if (FContains)
                return FValue;
            else if (FRequired)
                RaiseHorseException(FRequiredMessage, new object[] { FFieldName });
            return "";
        }

        public TimeSpan AsTime()
        {
            string LStrParam = AsString().Trim();
            try
            {
                if (string.IsNullOrEmpty(LStrParam))
                    return default;
                var fs = GetFormatSettings();
                string timePart = LStrParam.Substring(0, Math.Min(FTimeFormat.Length, LStrParam.Length));
                DateTime dt = DateTime.ParseExact(timePart, FTimeFormat, fs);
                return dt.TimeOfDay;
            }
            catch (Exception)
            {
                RaiseHorseException(FInvalidFormatMessage, new object[] { FFieldName, LStrParam, "time" });
                return default;
            }
        }

        public List<string> AsList()
        {
            if (FContains)
            {
                var arr = FValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim()).ToList();
                return arr;
            }
            else if (FRequired)
                RaiseHorseException(FRequiredMessage, new object[] { FFieldName });
            return new List<string>();
        }

        public List<T> AsList<T>()
        {
            if (FContains)
            {
                var arr = FValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var list = new List<T>();
                foreach (var s in arr)
                {
                    string trimmed = s.Trim();
                    try
                    {
                        object converted = null;
                        var targetType = typeof(T);
                        if (targetType == typeof(string))
                            converted = trimmed;
                        else if (targetType == typeof(int))
                            converted = int.Parse(trimmed);
                        else if (targetType == typeof(long))
                            converted = long.Parse(trimmed);
                        else if (targetType == typeof(double))
                            converted = double.Parse(trimmed, GetFormatSettings());
                        else if (targetType == typeof(DateTime))
                            converted = DateTime.Parse(trimmed, GetFormatSettings());
                        else
                        {
                            if (targetType == typeof(DateTime)) 
                                converted = DateTime.Parse(trimmed, GetFormatSettings());
                            else
                                throw new Exception("Unsupported type");
                        }
                        list.Add((T)converted);
                    }
                    catch (Exception)
                    {
                        RaiseHorseException(FInvalidFormatMessage, new object[] { FFieldName, trimmed, targetType.Name.ToLowerInvariant() });
                    }
                }
                return list;
            }
            else if (FRequired)
                RaiseHorseException(FRequiredMessage, new object[] { FFieldName });
            return new List<T>();
        }

        #endregion

        #region Helpers

        
        private CultureInfo GetFormatSettings()
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            if (!string.IsNullOrEmpty(FDateFormat) && FDateFormat.Contains("-"))
                ci.DateTimeFormat.DateSeparator = "-";
            ci.DateTimeFormat.ShortDatePattern = FDateFormat;
            ci.DateTimeFormat.ShortTimePattern = FTimeFormat;
            return ci;
        }

        private void RaiseHorseException(string AMessage, object[] Args)
        {
            string msg = string.Format(AMessage, Args);
            RaiseHorseException(msg);
        }

        private void RaiseHorseException(string AMessage)
        {
            throw HorseException.New(HTTPStatus.BadRequest, AMessage);
        }

        
        private bool TryISO8601ToDate(string AValue, out DateTime Value)
        {
            var style = FReturnUTC ? DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal : DateTimeStyles.None;
            return DateTime.TryParse(AValue, null, style, out Value);
        }

        
        private void InitializeLhsBrackets(Dictionary<string, string> AParams, string AFieldName)
        {
            FLhsBrackets = new HorseCoreParamFieldLhsBrackets();
            if (HorseCoreParamConfig.GetInstance().CheckLhsBrackets)
            {
                foreach (TLhsBracketsType type in Enum.GetValues(typeof(TLhsBracketsType)))
                {
                    string key = FFieldName + type.ToString();
                    if (AParams.ContainsKey(key))
                    {
                        FLhsBrackets.SetValue(type, AParams[key]);
                        FLhsBrackets.Types.Add(type);
                    }
                }
            }
        }

        #endregion

        #region Fluent Setters (Additional)

        public HorseCoreParamField RequiredMessage(string AValue)
        {
            FRequiredMessage = AValue;
            return this;
        }

        #endregion
       
        #region Conversion of String Value

        
        private string Trim(string s)
        {
            return s?.Trim() ?? "";
        }

        #endregion
    }
}
