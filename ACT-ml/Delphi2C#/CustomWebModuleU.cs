using System;

namespace CustomWebModuleU
{
    
    public class TMVCConfig
    {
        // Configuration properties go here.
    }

    public class TMVCEngine : IDisposable
    {
        public TMVCEngine(Action<TMVCConfig> configAction)
        {
            
            TMVCConfig config = new TMVCConfig();
            configAction(config);
        }

        public void Dispose()
        {
            // Dispose any resources held by the engine.
        }
    }

    
    public abstract class CustomWebModule : IDisposable
    {
        protected TMVCEngine MVCEngine { get; private set; }

        
        protected CustomWebModule()
        {
            
            MVCEngine = new TMVCEngine(config =>
            {
                // Nothing to configure here.
            });
            
            DoConfigureEngine(MVCEngine);
        }

        
        
        protected abstract void DoConfigureEngine(TMVCEngine engine);

        
        public void Dispose()
        {
            if (MVCEngine != null)
            {
                MVCEngine.Dispose();
                MVCEngine = null;
            }
        }
    }
}
