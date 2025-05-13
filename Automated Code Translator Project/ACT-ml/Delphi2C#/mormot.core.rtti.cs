using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Synopse.Rtti
{
    
    public enum RttiKind
    {
        Unknown,
        Integer,
        Float,
        String,
        Record,
        Array,
        Class,
        Variant,
        
    }

    
    public class RttiInfo
    {
        public RttiKind Kind { get; private set; }
        public string RawName { get; private set; }
        public Type Type { get; private set; }

        public RttiInfo(Type type)
        {
            Type = type;
            RawName = type.Name;
            Kind = DetermineKind(type);
        }

        static RttiKind DetermineKind(Type t)
        {
            
            if (t.IsEnum)
                return RttiKind.Integer; 
            else if (t == typeof(int) || t == typeof(uint) || t == typeof(short) || t == typeof(ushort)
                  || t == typeof(byte) || t == typeof(sbyte))
                return RttiKind.Integer;
            else if (t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                return RttiKind.Float;
            else if (t == typeof(string))
                return RttiKind.String;
            else if (t.IsArray)
                return RttiKind.Array;
            else if (t.IsClass)
                return RttiKind.Class;
            else if (t.IsValueType)
                return RttiKind.Record;
            else if (t == typeof(object))
                return RttiKind.Variant;
            else
                return RttiKind.Unknown;
        }

        public int RttiSize
        {
            get
            {
                try
                {
                    return Marshal.SizeOf(Type);
                }
                catch
                {
                    return 0;
                }
            }
        }

        
        public void Clear(object instance)
        {
            
            // If you need to “clear” fields, you could set them to default.
        }

        
        public void Copy(object destination, object source)
        {
            if (destination == null || source == null)
                throw new ArgumentNullException();
            foreach (var prop in Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    object value = prop.GetValue(source);
                    prop.SetValue(destination, value);
                }
            }
        }
    }

   
    public class RttiCustom
    {
        public RttiInfo Info { get; private set; }
        public Type ValueClass { get; private set; }

        
        public Dictionary<string, PropertyInfo> Properties { get; private set; }

        
        public RttiCustom(Type type)
        {
            ValueClass = type;
            Info = new RttiInfo(type);
            Properties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Properties[pi.Name] = pi;
            }
        }

        
        public object GetValue(object instance, string propertyName)
        {
            if (Properties.TryGetValue(propertyName, out PropertyInfo pi))
            {
                return pi.GetValue(instance);
            }
            throw new Exception($"Property '{propertyName}' not found in {ValueClass.Name}");
        }

        
        public void SetValue(object instance, string propertyName, object value)
        {
            if (Properties.TryGetValue(propertyName, out PropertyInfo pi))
            {
                pi.SetValue(instance, value);
            }
            else
            {
                throw new Exception($"Property '{propertyName}' not found in {ValueClass.Name}");
            }
        }

       
        public void CopyProperties(object destination, object source)
        {
            foreach (var pi in Properties.Values)
            {
                if (pi.CanRead && pi.CanWrite)
                {
                    object value = pi.GetValue(source);
                    pi.SetValue(destination, value);
                }
            }
        }
    }

    
    public class RttiCustomList
    {
       
        private readonly ConcurrentDictionary<Type, RttiCustom> _registeredTypes =
            new ConcurrentDictionary<Type, RttiCustom>();

        
        public RttiCustom RegisterType(Type type)
        {
            return _registeredTypes.GetOrAdd(type, t => new RttiCustom(t));
        }

        
        public RttiCustom FindType(Type type)
        {
            _registeredTypes.TryGetValue(type, out RttiCustom result);
            return result;
        }
    }

    
    public static class Rtti
    {
        
        public static readonly RttiCustomList Global = new RttiCustomList();

        
        public static string ToText(RttiKind kind)
        {
            return kind.ToString();
        }
    }

    
    public class ObjectWithCustomCreate
    {
        
        public ObjectWithCustomCreate()
        {
            
            Rtti.Global.RegisterType(GetType());
        }

       
        public static T CreateInstance<T>() where T : ObjectWithCustomCreate
        {
            return (T)Activator.CreateInstance(typeof(T));
        }
    }

    
    public class ObjectWithID : ObjectWithCustomCreate
    {
        public long ID { get; set; }

        public ObjectWithID() : base() { }

        public ObjectWithID(long id) : this()
        {
            ID = id;
        }
    }
    public static class ObjectCopyHelper
    {
        
        public static void CopyObject(object source, object destination)
        {
            if (source == null || destination == null)
                throw new ArgumentNullException();
            if (source.GetType() != destination.GetType())
                throw new ArgumentException("Source and destination must be of the same type.");

            var custom = Rtti.Global.RegisterType(source.GetType());
            custom.CopyProperties(destination, source);
        }

        
        public static T CopyObject<T>(T source) where T : ObjectWithCustomCreate
        {
            if (source == null) return null;
            T newObj = ObjectWithCustomCreate.CreateInstance<T>();
            CopyObject(source, newObj);
            return newObj;
        }
    }

}
