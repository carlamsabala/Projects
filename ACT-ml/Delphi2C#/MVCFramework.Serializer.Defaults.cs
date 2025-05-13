using MVCFramework.Serializer.Intf;
using MVCFramework.Serializer.JsonDataObjects;

namespace MVCFramework.Serializer.Defaults
{
    public static class SerializerDefaults
    {
        public static IMVCSerializer GetDefaultSerializer()
        {
            return new MVCJsonDataObjectsSerializer();
        }
    }
}
