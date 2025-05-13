using System;

namespace Horse.Core.Param.Config
{
    public class HorseCoreParamConfig
    {
        private static HorseCoreParamConfig _instance;
        private string _requiredMessage;
        private string _invalidFormatMessage;
        private string _dateFormat;
        private string _timeFormat;
        private bool _returnUTC;
        private string _trueValue;
        private bool _checkLhsBrackets;

        private HorseCoreParamConfig()
        {
            _returnUTC = true;
            _dateFormat = "yyyy-MM-dd";
            _timeFormat = "hh:mm:ss";
            _trueValue = "true";
            _requiredMessage = "The %s param is required.";
            _invalidFormatMessage = "The %0:s param '%1:s' is not valid a %2:s type.";
            _checkLhsBrackets = false;
        }

        public HorseCoreParamConfig WithRequiredMessage(string value)
        {
            _requiredMessage = value;
            return this;
        }

        
        public string RequiredMessage => _requiredMessage;

        
        public HorseCoreParamConfig WithInvalidFormatMessage(string value)
        {
            _invalidFormatMessage = value;
            return this;
        }

        
        public string InvalidFormatMessage => _invalidFormatMessage;

        
        public HorseCoreParamConfig WithDateFormat(string value)
        {
            _dateFormat = value;
            return this;
        }

        
        public string DateFormat => _dateFormat;

        
        public HorseCoreParamConfig WithTimeFormat(string value)
        {
            _timeFormat = value;
            return this;
        }

        
        public string TimeFormat => _timeFormat;

        
        public HorseCoreParamConfig WithReturnUTC(bool value)
        {
            _returnUTC = value;
            return this;
        }

        
        public bool ReturnUTC => _returnUTC;

        
        public HorseCoreParamConfig WithTrueValue(string value)
        {
            _trueValue = value;
            return this;
        }

        
        public string TrueValue => _trueValue;

        
        public HorseCoreParamConfig WithCheckLhsBrackets(bool value)
        {
            _checkLhsBrackets = value;
            return this;
        }

        
        public bool CheckLhsBrackets => _checkLhsBrackets;

        
        public static HorseCoreParamConfig GetInstance()
        {
            if (_instance == null)
            {
                _instance = new HorseCoreParamConfig();
            }
            return _instance;
        }

        
        public static void UnInitialize()
        {
            _instance = null;
        }
    }
}
