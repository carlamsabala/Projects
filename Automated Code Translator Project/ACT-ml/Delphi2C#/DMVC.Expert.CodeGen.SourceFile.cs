using System;
using Newtonsoft.Json.Linq;
using DMVC.Expert.CodeGen.Executor;  

namespace DMVCExpertCodeGenSourceFile
{
    public interface IOTAFile
    {
        string GetSource();
        DateTime GetAge();
    }

    public class TSourceFile : IOTAFile, IDisposable
    {
        private readonly Action<TMVCCodeGenerator> _generatorCallback;
        private JObject _json;
        private bool _disposed;

        public TSourceFile(Action<TMVCCodeGenerator> generatorCallback, JObject args)
        {
            _generatorCallback = generatorCallback;
            _json = (JObject)args.DeepClone();
        }

        public string GetSource()
        {
            return TMVCCodeGenerator.GenerateSource(_json, gen => _generatorCallback(gen));
        }

        public DateTime GetAge()
        {
            return DateTime.Now;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    
                    _json = null;
                }
                _disposed = true;
            }
        }

        ~TSourceFile()
        {
            Dispose(false);
        }
    }
}
