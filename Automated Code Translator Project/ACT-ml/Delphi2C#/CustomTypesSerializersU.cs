using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CustomTypesSerializersU
{
    public class MVCSerializationException : Exception
    {
        public MVCSerializationException(string message) : base(message) { }
    }

    public interface IMVCTypeSerializer
    {
        void SerializeAttribute(object elementValue, string propertyName, object serializerObject, Attribute[] attributes);
        void SerializeRoot(object obj, out object serializerObject, Attribute[] attributes, object serializationAction = null);
        void DeserializeAttribute(ref object elementValue, string propertyName, object serializerObject, Attribute[] attributes);
        void DeserializeRoot(object serializerObject, object obj, Attribute[] attributes);
    }

    public class TUserRolesSerializer : IMVCTypeSerializer
    {
        public void SerializeAttribute(object elementValue, string propertyName, object serializerObject, Attribute[] attributes)
        {
            if (!(serializerObject is JObject jObj))
                throw new Exception("serializerObject is not a JObject.");

            JArray jArray = jObj[propertyName] as JArray;
            if (jArray == null)
            {
                jArray = new JArray();
                jObj[propertyName] = jArray;
            }

            jArray.Add("--begin--");

            if (elementValue is IEnumerable<string> roles)
            {
                foreach (var role in roles)
                    jArray.Add(role);
            }
            else if (elementValue is string[] rolesArray)
            {
                foreach (var role in rolesArray)
                    jArray.Add(role);
            }

            jArray.Add("--end--");
        }

        public void SerializeRoot(object obj, out object serializerObject, Attribute[] attributes, object serializationAction = null)
        {
            throw new MVCSerializationException($"{GetType().Name} cannot be used as root object");
        }

        public void DeserializeAttribute(ref object elementValue, string propertyName, object serializerObject, Attribute[] attributes)
        {
            throw new Exception("To implement");
        }

        public void DeserializeRoot(object serializerObject, object obj, Attribute[] attributes)
        {
            // No implementation provided.
        }
    }

    public class TSysUserSerializer : IMVCTypeSerializer
    {
        public void SerializeAttribute(object elementValue, string propertyName, object serializerObject, Attribute[] attributes)
        {
            if (!(serializerObject is JObject jObj))
                throw new Exception("serializerObject is not a JObject.");

            jObj["prop"] = "hello there attribute";
        }

        public void SerializeRoot(object obj, out object serializerObject, Attribute[] attributes, object serializationAction = null)
        {
            JObject jObj = new JObject();
            serializerObject = jObj;

            if (!(obj is TSysUser user))
                throw new Exception("obj is not of type TSysUser.");

            jObj["username"] = user.UserName;
            jObj["roles"] = string.Join(",", user.Roles);
        }

        public void DeserializeAttribute(ref object elementValue, string propertyName, object serializerObject, Attribute[] attributes)
        {
            // No implementation provided.
        }

        public void DeserializeRoot(object serializerObject, object obj, Attribute[] attributes)
        {
            // No implementation provided.
        }
    }

    public class TNullableAliasSerializer : IMVCTypeSerializer
    {
        public void Serialize(object elementValue, ref object serializerObject, Attribute[] attributes)
        {
            throw new Exception("TODO");
        }

        public void Deserialize(object serializedObject, ref object elementValue, Attribute[] attributes)
        {
            // No implementation.
        }

        public void SerializeAttribute(object elementValue, string propertyName, object serializerObject, Attribute[] attributes)
        {
            // Empty implementation.
        }

        public void SerializeRoot(object obj, out object serializerObject, Attribute[] attributes, object serializationAction = null)
        {
            throw new MVCSerializationException($"{GetType().Name} cannot be used as root object");
        }

        public void DeserializeAttribute(ref object elementValue, string propertyName, object serializerObject, Attribute[] attributes)
        {
            // Empty implementation.
        }

        public void DeserializeRoot(object serializerObject, object obj, Attribute[] attributes)
        {
            // Empty implementation.
        }
    }

    public class TSysUser
    {
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
    }
}
