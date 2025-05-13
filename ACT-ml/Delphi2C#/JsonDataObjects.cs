
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonDataObjects
{
    #region Exceptions

    public class JsonException : Exception
    {
        public JsonException(string message) : base(message) { }
    }

    public class JsonCastException : JsonException
    {
        public JsonCastException(string message) : base(message) { }
    }

    public class JsonPathException : JsonException
    {
        public JsonPathException(string message) : base(message) { }
    }

    public class JsonParserException : JsonException
    {
        public int LineNum { get; }
        public int Column { get; }
        public int Position { get; }

        public JsonParserException(string message, int lineNum, int column, int position)
            : base($"{message} ({lineNum}, {column})")
        {
            LineNum = lineNum;
            Column = column;
            Position = position;
        }
    }

    #endregion

    #region Configuration

    public struct JsonSerializationConfig
    {
        public string LineBreak;
        public string IndentChar;
        public bool UseUtcTime;
        public bool EscapeAllNonASCIIChars;
        public bool NullConvertsToValueTypes;

        public void InitDefaults()
        {
            LineBreak = "\n";
            IndentChar = "\t";
            UseUtcTime = true;
            EscapeAllNonASCIIChars = false;
            NullConvertsToValueTypes = false;
        }

        public static JsonSerializationConfig Default
        {
            get
            {
                var cfg = new JsonSerializationConfig();
                cfg.InitDefaults();
                return cfg;
            }
        }
    }

    #endregion

    #region Core JSON Objects

    
    public abstract class JsonBaseObject
    {
        
        public abstract JObject ToJson();

        
        public abstract void FromJson(JObject json);

        
        public JsonBaseObject Clone()
        {
            
            var json = ToJson();
            var clone = CreateInstance();
            clone.FromJson(json);
            return clone;
        }

        protected abstract JsonBaseObject CreateInstance();

        public static string DateTimeToJSON(DateTime value, bool useUtcTime)
        {
            if (useUtcTime)
                return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            else
                return value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        public static DateTime JSONToDateTime(string value, bool convertToLocalTime = true)
        {
            
            var dt = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return convertToLocalTime ? dt.ToLocalTime() : dt;
        }

        public static string UtcDateTimeToJSON(DateTime utcDateTime)
        {
            return utcDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }
    }

    
    public class JsonObject : JsonBaseObject
    {
        private readonly Dictionary<string, object> _dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public void Add(string key, object value)
        {
            _dict[key] = value;
        }

        public bool ContainsKey(string key) => _dict.ContainsKey(key);

        public object this[string key]
        {
            get => _dict.TryGetValue(key, out var value) ? value : null;
            set => _dict[key] = value;
        }

        public override JObject ToJson()
        {
            var jobj = new JObject();
            foreach (var kv in _dict)
            {
                if (kv.Value is JsonBaseObject jbo)
                    jobj[kv.Key] = jbo.ToJson();
                else if (kv.Value is JsonArray jarr)
                    jobj[kv.Key] = jarr.ToJson()["array"]; 
                else
                    jobj[kv.Key] = JToken.FromObject(kv.Value);
            }
            return jobj;
        }

        public override void FromJson(JObject json)
        {
            _dict.Clear();
            foreach (var prop in json.Properties())
            {
                
                _dict[prop.Name] = prop.Value;
            }
        }

        protected override JsonBaseObject CreateInstance() => new JsonObject();

        public JsonObject CloneObject() => (JsonObject)this.Clone();
    }

    
    public class JsonArray : JsonBaseObject
    {
        private readonly List<object> _items = new List<object>();

        public void Add(object value)
        {
            _items.Add(value);
        }

        public object this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public int Count => _items.Count;

        public override JObject ToJson()
        {
            var jarr = new JArray();
            foreach (var item in _items)
            {
                if (item is JsonBaseObject jbo)
                    jarr.Add(jbo.ToJson());
                else
                    jarr.Add(JToken.FromObject(item));
            }
            
            return new JObject { ["array"] = jarr };
        }

        public override void FromJson(JObject json)
        {
            _items.Clear();
            var jarr = json["array"] as JArray;
            if (jarr != null)
            {
                foreach (var token in jarr)
                    _items.Add(token);
            }
        }

        protected override JsonBaseObject CreateInstance() => new JsonArray();

        public JsonArray CloneArray() => (JsonArray)this.Clone();
    }

    #endregion

    #region Data Value Helper

    
    public class JsonDataValueHelper
    {
        public JsonDataType Type { get; set; }
        public object Value { get; set; }

        public static implicit operator JsonDataValueHelper(string value)
        {
            return new JsonDataValueHelper { Type = JsonDataType.String, Value = value };
        }

        public static implicit operator string(JsonDataValueHelper helper)
        {
            return helper.Value?.ToString();
        }

        public static implicit operator JsonDataValueHelper(int value)
        {
            return new JsonDataValueHelper { Type = JsonDataType.Int, Value = value };
        }

        public static implicit operator int(JsonDataValueHelper helper)
        {
            if (helper.Value is int i) return i;
            if (int.TryParse(helper.Value?.ToString(), out i))
                return i;
            return 0;
        }

        
    }

    public enum JsonDataType
    {
        None,
        String,
        Int,
        Long,
        ULong,
        Float,
        DateTime,
        UtcDateTime,
        Bool,
        Array,
        Object
    }

    #endregion

    #region Parsing and Serialization

    
    public static class JsonParser
    {
        
        public static JsonBaseObject Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;
            json = json.Trim();
            if (json.StartsWith("["))
            {
                var arr = new JsonArray();
                arr.FromJson(new JObject { ["array"] = JArray.Parse(json) });
                return arr;
            }
            else
            {
                var obj = new JsonObject();
                obj.FromJson(JObject.Parse(json));
                return obj;
            }
        }

        
        public static string Serialize(JsonBaseObject jsonObj, bool compact = true)
        {
            var formatting = compact ? Formatting.None : Formatting.Indented;
            return jsonObj.ToJson().ToString(formatting);
        }
    }

    #endregion

    #region Example Usage

    

    #endregion
}
