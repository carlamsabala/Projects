using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DMVCExpertCodeGenExecutor
{
        public interface IGenCommand
    {
                void ExecuteInterface(StringBuilder intf, JObject model);
        void ExecuteImplementation(StringBuilder impl, JObject model);
    }

    public class TMVCCodeGenerator
    {
        private StringBuilder fIntf;
        private StringBuilder fImpl;
        private List<IGenCommand> fCommands;
        private string fSource;

        public TMVCCodeGenerator()
        {
            fCommands = new List<IGenCommand>();
            fSource = string.Empty;
        }

        public List<IGenCommand> Commands => fCommands;

        public string Source => fSource;

        public void Execute(JObject model)
        {
            fSource = string.Empty;
            fIntf = new StringBuilder();
            try
            {
                fImpl = new StringBuilder();
                try
                {
                    foreach (var command in fCommands)
                    {
                        command.ExecuteInterface(fIntf, model);
                        command.ExecuteImplementation(fImpl, model);
                    }
                    fSource = fIntf.ToString() + fImpl.ToString();
                }
                finally
                {
                    // fImpl is a managed object; no explicit disposal is required.
                }
            }
            finally
            {
                // fIntf is a managed object; no explicit disposal is required.
            }
        }

        
        public static string GenerateSource(JObject configModelRef, Action<TMVCCodeGenerator> fillerProc)
        {
            var generator = new TMVCCodeGenerator();
            try
            {
                generator.Commands.Clear();
                fillerProc(generator);
                generator.Execute(configModelRef);
                return generator.Source;
            }
            finally
            {
                // No unmanaged resources to dispose.
            }
        }
    }
}
