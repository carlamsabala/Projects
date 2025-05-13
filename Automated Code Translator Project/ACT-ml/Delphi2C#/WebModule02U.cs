using MVCFramework;

namespace YourNamespace
{
    public class WebModule02 : CustomWebModule
    {
        protected override void DoConfigureEngine(TMVCEngine engine)
        {
            base.DoConfigureEngine(engine);
            engine.AddController(typeof(App1MainController));
        }
    }
}
