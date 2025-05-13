using System;
using MVCFramework.RQL.Parser;
using MVCFramework.RQL.AST2FirebirdSQL; 
using MVCFramework.ActiveRecord;

namespace MVCFramework.RQL.AST2InterbaseSQL
{
    
    public class RQLInterbaseCompiler : RQLFirebirdCompiler
    {
        
        protected override string GetLiteralBoolean(bool value)
        {
            return value ? "1" : "0";
        }
    }

    
    public static class RQLInterbaseCompilerRegistration
    {
        static RQLInterbaseCompilerRegistration()
        {
            
            RQLCompilerRegistry.Instance.RegisterCompiler("interbase", typeof(RQLInterbaseCompiler));
        }

        
        public static void Unregister()
        {
            RQLCompilerRegistry.Instance.UnRegisterCompiler("interbase");
        }
    }
}
