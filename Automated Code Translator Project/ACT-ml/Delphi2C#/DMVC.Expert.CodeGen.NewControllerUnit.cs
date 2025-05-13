using System;
using Newtonsoft.Json.Linq;

namespace DMVCExpertCodeGenNewControllerUnit
{
    
    public interface IOTAFile
    {
        // Define members as needed.
    }

    
    public abstract class TNewUnit
    {
        
        protected JObject fConfigModelRef;
        
        public string Personality { get; set; }

        protected TNewUnit(JObject configModelRef)
        {
            fConfigModelRef = configModelRef;
        }

        public abstract IOTAFile NewImplSource(string moduleIdent, string formIdent, string ancestorIdent);
    }

    public delegate void TTemplateLoadProcedure(TMVCCodeGenerator gen);

    public class TMVCCodeGenerator
    {
        // Implementation details omitted.
    }

    public class TSourceFile : IOTAFile
    {
        public string Source { get; private set; }

        public TSourceFile(Action<TMVCCodeGenerator> fillerProc, JObject configModel)
        {
            TMVCCodeGenerator generator = new TMVCCodeGenerator();
            fillerProc(generator);
            Source = string.Empty;
        }

        public static IOTAFile Create(Action<TMVCCodeGenerator> fillerProc, JObject configModel)
        {
            return new TSourceFile(fillerProc, configModel);
        }
    }

    public static class OTAModuleServices
    {
        public static void GetNewModuleAndClassName(string prefix, out string unitIdent, out string formName, out string fileName)
        {
            unitIdent = "NewUnitName";
            formName = "NewFormName";
            fileName = "NewFileName.cs";
        }
    }

     public class TNewControllerUnitEx : TNewUnit
    {
        public TNewControllerUnitEx(JObject configModelRef, string aPersonality = "")
            : base(configModelRef)
        {
            Personality = aPersonality;
        }

        public override IOTAFile NewImplSource(string moduleIdent, string formIdent, string ancestorIdent)
        {
            OTAModuleServices.GetNewModuleAndClassName(string.Empty, out string lUnitIdent, out string lFormName, out string lFileName);

            fConfigModelRef["controller_unit_name"] = lUnitIdent;

            return TSourceFile.Create(
                gen =>
                {
                    
                    Console.WriteLine("Filling controller templates...");
                },
                fConfigModelRef);
        }
    }

    public class TNewGenericUnitFromTemplate : TNewUnit
    {
        private readonly TTemplateLoadProcedure fTemplateLoadProcedure;
        private readonly string fUnitIdentKeyName;

        public TNewGenericUnitFromTemplate(
            JObject configModelRef,
            TTemplateLoadProcedure templateLoadProcedure,
            string unitIdentKeyName,
            string aPersonality = "")
            : base(configModelRef)
        {
            fTemplateLoadProcedure = templateLoadProcedure;
            fUnitIdentKeyName = unitIdentKeyName;
            Personality = aPersonality;
        }

        public override IOTAFile NewImplSource(string moduleIdent, string formIdent, string ancestorIdent)
        {
            OTAModuleServices.GetNewModuleAndClassName(string.Empty, out string lUnitIdent, out string dummy, out string lFileName);
            fConfigModelRef[fUnitIdentKeyName] = lUnitIdent;

            return TSourceFile.Create(
                gen =>
                {
                    fTemplateLoadProcedure(gen);
                },
                fConfigModelRef);
        }
    }
}
