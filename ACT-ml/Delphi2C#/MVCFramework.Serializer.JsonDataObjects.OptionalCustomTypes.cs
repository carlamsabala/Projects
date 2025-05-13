
using System;
using System.Drawing;
using System.IO;
using Newtonsoft.Json.Linq;
using MVCFramework.Serializer.Intf;
using MVCFramework.Serializer.Commons;

namespace MVCFramework.Serializer.JsonDataObjects.OptionalCustomTypes
{
#if WINDOWS
    public class MVCBitmapSerializerJsonDataObject : IMVCTypeSerializer
    {
        
        public void SerializeAttribute(object elementValue, string propertyName, object serializerObject, object[] attributes)
        {
            var image = elementValue as Image;
            if (image != null)
            {
                using (var ms = new MemoryStream())
                {
                    
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    string base64 = Convert.ToBase64String(ms.ToArray());
                    
                    if (serializerObject is JObject jObj)
                    {
                        jObj[propertyName] = base64;
                    }
                }
            }
            else
            {
                if (serializerObject is JObject jObj)
                {
                    jObj[propertyName] = null;
                }
            }
        }

        
        public void SerializeRoot(object obj, out object serializerObject, object[] attributes, Action serializationAction = null)
        {
            var jObj = new JObject();
            serializerObject = jObj;
            try
            {
                SerializeAttribute(obj, "data", jObj, attributes);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        
        public void DeserializeAttribute(ref object elementValue, string propertyName, object serializerObject, object[] attributes)
        {
            if (serializerObject is JObject jObj)
            {
                string base64 = jObj.Value<string>(propertyName);
                if (!string.IsNullOrEmpty(base64))
                {
                    byte[] bytes = Convert.FromBase64String(base64);
                    using (var ms = new MemoryStream(bytes))
                    {
                        var image = Image.FromStream(ms);
                        elementValue = image;
                    }
                }
            }
        }

        
        public void DeserializeRoot(object serializerObject, object obj, object[] attributes)
        {
            throw new NotSupportedException("Direct image deserialization not supported");
        }
    }
#endif

    public static class OptionalCustomTypesSerializers
    {
#if WINDOWS
        
        public static void RegisterOptionalCustomTypesSerializers(IMVCSerializer serializer)
        {
            
            serializer.RegisterTypeSerializer(typeof(Bitmap), new MVCBitmapSerializerJsonDataObject());
            serializer.RegisterTypeSerializer(typeof(Image), new MVCBitmapSerializerJsonDataObject());
        }
#endif

        
        public static void RegisterOptionalCustomTypesSerializersForJSON(System.Collections.Generic.Dictionary<string, IMVCSerializer> serializers)
        {
#if WINDOWS
            if (serializers.TryGetValue(MVCMediaType.APPLICATION_JSON, out IMVCSerializer serializer))
            {
                RegisterOptionalCustomTypesSerializers(serializer);
            }
#endif
        }
    }
}
