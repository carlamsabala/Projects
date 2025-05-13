using System;
using System.Reflection;

namespace Horse.Rtti
{
    public class HorseRtti
    {
        private static HorseRtti _instance;

        
        private HorseRtti()
        {
            if (_instance != null)
                throw new Exception("The Horse Rtti instance has already been created");
            _instance = this;
        }
        
        
        public Type GetType(Type aClass)
        {
            
            return aClass;
        }
        
        
        public static HorseRtti GetInstance()
        {
            if (_instance == null)
                _instance = new HorseRtti();
            return _instance;
        }
        
        
        public static void UnInitialize()
        {
            _instance = null;
        }
    }
}
