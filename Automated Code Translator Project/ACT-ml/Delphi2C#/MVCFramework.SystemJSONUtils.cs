using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MVCFramework
{
    
    public static class SystemJSONUtils
    {
        
        public static JToken StringToJSONValue(string value)
        {
            try
            {
                JToken token = JToken.Parse(value);
                if (token == null)
                    throw new Exception("Invalid JSON");
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid JSON: " + ex.Message);
            }
        }

        
        public static JToken StringToJSONValueNoException(string value)
        {
            try
            {
                return JToken.Parse(value);
            }
            catch
            {
                return null;
            }
        }

       
        public static JObject StringAsJSONObject(string value)
        {
            var token = StringToJSONValue(value) as JObject;
            if (token == null)
                throw new Exception("Invalid JSON Object");
            return token;
        }

        
        public static JObject StringAsJSONObjectNoException(string value)
        {
            return StringToJSONValueNoException(value) as JObject;
        }

        
        public static JArray StringAsJSONArray(string value)
        {
            var token = StringToJSONValue(value) as JArray;
            return token;
        }

       
        public static string JSONValueToString(JToken token, bool owns = true)
        {
            string result = token.ToString(Formatting.None);
            return result;
        }

        
        public static JProperty GetPair(JObject obj, string propertyName)
        {
            if (obj == null)
                throw new Exception("JSONObject is null");
            return obj.Property(propertyName);
        }

        
        public static string GetStringDef(JObject obj, string propertyName, string defaultValue = "")
        {
            var pair = GetPair(obj, propertyName);
            if (pair == null)
                return defaultValue;
            if (pair.Value.Type == JTokenType.String)
                return pair.Value.ToString();
            throw new Exception($"Property {propertyName} is not a String Property");
        }

        
        public static double GetNumberDef(JObject obj, string propertyName, double defaultValue = 0)
        {
            var pair = GetPair(obj, propertyName);
            if (pair == null)
                return defaultValue;
            if (pair.Value.Type == JTokenType.Float || pair.Value.Type == JTokenType.Integer)
                return pair.Value.Value<double>();
            throw new Exception("Property is not a Number Property");
        }

        
        public static JObject GetJSONObj(JObject obj, string propertyName)
        {
            var pair = GetPair(obj, propertyName);
            if (pair == null)
                return null;
            if (pair.Value.Type == JTokenType.Object)
                return (JObject)pair.Value;
            throw new Exception("Property is not a JSONObject");
        }

        
        public static JArray GetJSONArray(JObject obj, string propertyName)
        {
            var pair = GetPair(obj, propertyName);
            if (pair == null)
                return null;
            if (pair.Value.Type == JTokenType.Array)
                return (JArray)pair.Value;
            throw new Exception("Property is not a JSONArray");
        }

        
        public static int GetIntegerDef(JObject obj, string propertyName, int defaultValue = 0)
        {
            var pair = GetPair(obj, propertyName);
            if (pair == null)
                return defaultValue;
            if (pair.Value.Type == JTokenType.Integer)
                return pair.Value.Value<int>();
            throw new Exception($"Property {propertyName} is not an Integer Property");
        }

        
        public static long GetInt64Def(JObject obj, string propertyName, long defaultValue = 0)
        {
            var pair = GetPair(obj, propertyName);
            if (pair == null)
                return defaultValue;
            if (pair.Value.Type == JTokenType.Integer)
                return pair.Value.Value<long>();
            throw new Exception($"Property {propertyName} is not an Int64 Property");
        }

        
        public static bool GetBooleanDef(JObject obj, string propertyName, bool defaultValue = false)
        {
            var pair = GetPair(obj, propertyName);
            if (pair == null)
                return defaultValue;
            if (pair.Value.Type == JTokenType.Boolean)
                return pair.Value.Value<bool>();
            throw new Exception($"Property {propertyName} is not a Boolean Property");
        }

        
        public static object GetProperty(object obj, string propertyName)
        {
            if (obj == null)
                throw new Exception("Object is null");
            var type = obj.GetType();
            PropertyInfo prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                throw new Exception($"Cannot get RTTI for property [{type.Name}.{propertyName}]");
            if (prop.CanRead)
                return prop.GetValue(obj);
            throw new Exception($"Property is not readable [{type.Name}.{propertyName}]");
        }

        
        public static bool PropertyExists(JObject obj, string propertyName)
        {
            return obj.Property(propertyName) != null;
        }
    }
}
