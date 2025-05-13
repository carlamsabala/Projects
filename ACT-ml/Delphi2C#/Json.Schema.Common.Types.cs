using System;

namespace Json.Schema.Common.Types
{
    public enum SchemaKind
    {
        Unknown,
        Integer,
        Int64,
        Number,
        DateTime,
        Date,
        Time,
        Enumeration,
        Boolean,
        Object,
        Array,
        String,
        Char,
        Guid
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SchemaTypeAttribute : Attribute
    {
        private readonly SchemaKind _kind;

        private const string SchemaTypeBoolean = "boolean";
        private const string SchemaTypeInteger = "integer";
        private const string SchemaTypeNumber = "number";
        private const string SchemaTypeString = "string";
        private const string SchemaTypeArray = "array";
        private const string SchemaTypeObject = "object";

        public SchemaTypeAttribute(SchemaKind kind)
        {
            _kind = kind;
        }

        public string Name
        {
            get
            {
                switch (_kind)
                {
                    case SchemaKind.Integer:
                    case SchemaKind.Int64:
                    case SchemaKind.Enumeration:
                        return SchemaTypeInteger;
                    case SchemaKind.Number:
                        return SchemaTypeNumber;
                    case SchemaKind.String:
                    case SchemaKind.Char:
                    case SchemaKind.Guid:
                    case SchemaKind.DateTime:
                    case SchemaKind.Date:
                    case SchemaKind.Time:
                        return SchemaTypeString;
                    case SchemaKind.Boolean:
                        return SchemaTypeBoolean;
                    case SchemaKind.Object:
                        return SchemaTypeObject;
                    case SchemaKind.Array:
                        return SchemaTypeArray;
                    default:
                        return string.Empty;
                }
            }
        }

        public SchemaKind Kind => _kind;
    }
}
