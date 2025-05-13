using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web; 

namespace MVCFramework.Serializer.URLEncoded
{
    
    public enum TMVCSerializationType
    {
        Default,
        Properties,
        Fields,
        Unknown
    }

    
    public enum TMVCNameCase
    {
        AsIs,
        LowerCase,
        UpperCase
    }

    
    public class EMVCException : Exception
    {
        public int StatusCode { get; }
        public EMVCException(int statusCode, string message) : base(message) => StatusCode = statusCode;
    }

    
    public class EMVCDeserializationException : Exception
    {
        public EMVCDeserializationException(string message) : base(message) { }
    }

    
    public interface IMVCTypeSerializer
    {
        void SerializeAttribute(object elementValue, string propertyName, object serializerObject, Attribute[] customAttributes);
        void SerializeRoot(object obj, out object serializerObject, Attribute[] customAttributes, Action serializationAction = null);
        void DeserializeAttribute(ref object elementValue, string propertyName, object serializerObject, Attribute[] customAttributes);
        void DeserializeRoot(object serializerObject, object target, Attribute[] customAttributes);
    }

    
    public interface IMVCSerializer
    {
        void RegisterTypeSerializer(Type type, IMVCTypeSerializer serializer);
        string SerializeObject(object obj, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null);
        string SerializeObject(IInterface obj, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null);
        string SerializeRecord(object record, Type recordType, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null);
        string SerializeCollection(object list, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null);
        string SerializeCollection(IInterface list, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null);
        string SerializeDataSet(System.Data.DataSet dataSet, IList<string> ignoredFields = null, TMVCNameCase nameCase = TMVCNameCase.AsIs, Action datasetSerializationAction = null);
        string SerializeDataSetRecord(System.Data.DataSet dataSet, IList<string> ignoredFields = null, TMVCNameCase nameCase = TMVCNameCase.AsIs, Action datasetSerializationAction = null);
        string SerializeArrayOfRecord(ref object arrayContainer, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null);

        void DeserializeObject(string serializedObject, object target, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, string rootNode = "");
        void DeserializeObject(string serializedObject, IInterface target, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null);
        void DeserializeCollection(string serializedList, object list, Type clazz, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, string rootNode = "");
        void DeserializeCollection(string serializedList, IInterface list, Type clazz, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null);
        void DeserializeDataSet(string serializedDataSet, System.Data.DataSet dataSet, IList<string> ignoredFields = null, TMVCNameCase nameCase = TMVCNameCase.AsIs);
        void DeserializeDataSetRecord(string serializedDataSetRecord, System.Data.DataSet dataSet, IList<string> ignoredFields = null, TMVCNameCase nameCase = TMVCNameCase.AsIs);
    }

    
    public interface IInterface { }

    
    public class MVCURLEncodedSerializer : IMVCSerializer
    {
        

        protected void RaiseNotImplemented()
        {
            throw new NotImplementedException("Not Implemented");
        }

        protected Dictionary<string, string> ParseUrlEncodedString(string serializedObject)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var pairs = serializedObject.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0];
                    var value = HttpUtility.UrlDecode(keyValue[1]);
                    dict[key] = value;
                }
            }
            return dict;
        }

        protected object ConvertRawValue(string rawData, Type targetType)
        {
            
            if (targetType == typeof(string))
            {
                return HttpUtility.UrlDecode(rawData);
            }
            
            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                if (int.TryParse(rawData, NumberStyles.Any, CultureInfo.InvariantCulture, out int intVal))
                    return intVal;
                return null;
            }
            if (targetType == typeof(long) || targetType == typeof(long?))
            {
                if (long.TryParse(rawData, NumberStyles.Any, CultureInfo.InvariantCulture, out long longVal))
                    return longVal;
                return null;
            }
            if (targetType == typeof(double) || targetType == typeof(double?))
            {
                if (double.TryParse(rawData, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleVal))
                    return doubleVal;
                return null;
            }
            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (bool.TryParse(rawData, out bool boolVal))
                    return boolVal;
                if (rawData == "0") return false;
                if (rawData == "1") return true;
                return null;
            }
            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                if (DateTime.TryParse(rawData, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                    return dt;
                return null;
            }
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type underlyingType = Nullable.GetUnderlyingType(targetType);
                return ConvertRawValue(rawData, underlyingType);
            }
            throw new EMVCDeserializationException($"Unsupported type for conversion: {targetType}");
        }

        public void DataValueToAttribute(object target, MemberInfo member, string rawData, string name, out object value,
            TMVCSerializationType serializationType, IList<string> ignoredAttributes, Attribute[] customAttributes)
        {
            value = null;
            Type memberType = null;
            if (member is PropertyInfo prop)
            {
                memberType = prop.PropertyType;
            }
            else if (member is FieldInfo fld)
            {
                memberType = fld.FieldType;
            }
            else
            {
                throw new Exception("Unsupported member type");
            }

            try
            {
                value = ConvertRawValue(rawData, memberType);
            }
            catch (Exception ex)
            {
                throw new EMVCDeserializationException($"Error converting value '{rawData}' for member '{name}': {ex.Message}");
            }
        }

        public void DataValueToAttribute(object target, MemberInfo member, string[] rawDataArray, string name, out object value,
            TMVCSerializationType serializationType, IList<string> ignoredAttributes, Attribute[] customAttributes)
        {
            value = rawDataArray;
        }


        public void RegisterTypeSerializer(Type type, IMVCTypeSerializer serializer)
        {
            RaiseNotImplemented();
        }

        public string SerializeObject(object obj, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null)
        {
            RaiseNotImplemented();
            return string.Empty;
        }

        public string SerializeObject(IInterface obj, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null)
        {
            RaiseNotImplemented();
            return string.Empty;
        }

        public string SerializeRecord(object record, Type recordType, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null)
        {
            RaiseNotImplemented();
            return string.Empty;
        }

        public string SerializeCollection(object list, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null)
        {
            RaiseNotImplemented();
            return string.Empty;
        }

        public string SerializeCollection(IInterface list, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null)
        {
            RaiseNotImplemented();
            return string.Empty;
        }

        public string SerializeDataSet(System.Data.DataSet dataSet, IList<string> ignoredFields = null, TMVCNameCase nameCase = TMVCNameCase.AsIs, Action datasetSerializationAction = null)
        {
            RaiseNotImplemented();
            return string.Empty;
        }

        public string SerializeDataSetRecord(System.Data.DataSet dataSet, IList<string> ignoredFields = null, TMVCNameCase nameCase = TMVCNameCase.AsIs, Action datasetSerializationAction = null)
        {
            RaiseNotImplemented();
            return string.Empty;
        }

        public string SerializeArrayOfRecord(ref object arrayContainer, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, Action serializationAction = null)
        {
            RaiseNotImplemented();
            return string.Empty;
        }

        public void DeserializeObject(string serializedObject, object target, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, string rootNode = "")
        {
            if (string.IsNullOrEmpty(serializedObject))
                throw new EMVCException(400, "Invalid body");

            if (target == null)
                return;

            var dict = ParseUrlEncodedString(serializedObject);

            URLEncodedStringToObject(dict, target, GetSerializationType(target, serializationType), ignoredAttributes);
        }

        public void DeserializeObject(string serializedObject, IInterface target, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null)
        {
            RaiseNotImplemented();
        }

        public void DeserializeCollection(string serializedList, object list, Type clazz, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null, string rootNode = "")
        {
            RaiseNotImplemented();
        }

        public void DeserializeCollection(string serializedList, IInterface list, Type clazz, TMVCSerializationType serializationType = TMVCSerializationType.Default, IList<string> ignoredAttributes = null)
        {
            RaiseNotImplemented();
        }

        public void DeserializeDataSet(string serializedDataSet, System.Data.DataSet dataSet, IList<string> ignoredFields = null, TMVCNameCase nameCase = TMVCNameCase.AsIs)
        {
            RaiseNotImplemented();
        }

        public void DeserializeDataSetRecord(string serializedDataSetRecord, System.Data.DataSet dataSet, IList<string> ignoredFields = null, TMVCNameCase nameCase = TMVCNameCase.AsIs)
        {
            RaiseNotImplemented();
        }

        public void URLEncodedStringToObject(Dictionary<string, string> data, object target, TMVCSerializationType serializationType, IList<string> ignoredAttributes)
        {
            if (target == null)
                return;

            if (target is Newtonsoft.Json.Linq.JObject jObj)
            {
                foreach (var kvp in data)
                {
                    if (jObj[kvp.Key] == null)
                    {
                        jObj[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        var current = jObj[kvp.Key];
                        jObj[kvp.Key] = new Newtonsoft.Json.Linq.JArray(current, kvp.Value);
                    }
                }
                return;
            }

            Type targetType = target.GetType();
            var props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (!prop.CanWrite) continue;
                if (ignoredAttributes != null && ignoredAttributes.Contains(prop.Name)) continue;

                string key = prop.Name;
                if (data.ContainsKey(key))
                {
                    string rawData = data[key];
                    object convertedValue;
                    try
                    {
                        DataValueToAttribute(target, prop, rawData, key, out convertedValue, serializationType, ignoredAttributes, prop.GetCustomAttributes().ToArray());
                        if (convertedValue != null)
                        {
                            prop.SetValue(target, convertedValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new EMVCException(400, $"Invalid type conversion for property '{key}': {ex.Message}");
                    }
                }
            }

            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (ignoredAttributes != null && ignoredAttributes.Contains(field.Name)) continue;
                string key = field.Name;
                if (data.ContainsKey(key))
                {
                    string rawData = data[key];
                    object convertedValue;
                    try
                    {
                        DataValueToAttribute(target, field, rawData, key, out convertedValue, serializationType, ignoredAttributes, field.GetCustomAttributes().ToArray());
                        if (convertedValue != null)
                        {
                            field.SetValue(target, convertedValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new EMVCException(400, $"Invalid type conversion for field '{key}': {ex.Message}");
                    }
                }
            }
        }

        protected TMVCSerializationType GetSerializationType(object target, TMVCSerializationType type)
        {
            return type;
        }
    }
}
