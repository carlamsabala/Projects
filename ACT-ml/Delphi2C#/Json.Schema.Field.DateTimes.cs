using System;
using Newtonsoft.Json.Linq;
using Json.Schema.Common.Types;

namespace Json.Schema.Field.DateTimes
{
    
    public abstract class JsonField
    {
        
        public virtual JObject ToJsonSchema()
        {
            return new JObject
            {
                // Other common schema properties can be added here.
            };
        }
    }

    [SchemaType(SchemaKind.DateTime)]
    public class JsonFieldDateTime : JsonField
    {
        protected const string DateFormat = "yyyy-MM-dd";
        protected const string TimeFormat = "HH:mm:ss";
        protected const string DateTimeFormat = DateFormat + "T" + TimeFormat;

        
        protected virtual string GetFormat()
        {
            return DateTimeFormat;
        }

        public override JObject ToJsonSchema()
        {
            var schema = base.ToJsonSchema();
            schema["format"] = GetFormat();
            return schema;
        }

        
        public string Format => GetFormat();
    }

    [SchemaType(SchemaKind.Date)]
    public class JsonFieldDate : JsonFieldDateTime
    {
        
        protected override string GetFormat()
        {
            return DateFormat;
        }
    }

    [SchemaType(SchemaKind.Time)]
    public class JsonFieldTime : JsonFieldDateTime
    {
        
        protected override string GetFormat()
        {
            return TimeFormat;
        }
    }

    
    public static class JsonFieldRegistration
    {
        static JsonFieldRegistration()
        {
           
            // SchemaRegistry.Register(typeof(JsonFieldDateTime));
            // SchemaRegistry.Register(typeof(JsonFieldDate));
            // SchemaRegistry.Register(typeof(JsonFieldTime));
        }
    }
}
