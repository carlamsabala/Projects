using System;
using MVCFramework;

namespace MVCFramework.Middleware
{
    
    public class MVCSecurityHeadersMiddleware : IMVCMiddleware
    {
        
        public void OnBeforeRouting(WebContext context, ref bool handled)
        {
            context.Response.SetCustomHeader("X-XSS-Protection", "1; mode=block");
            context.Response.SetCustomHeader("X-Content-Type-Options", "nosniff");
        }

        
        public void OnBeforeControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, ref bool handled)
        {
            // Do nothing.
        }

        
        public void OnAfterControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, bool handled)
        {
            // Do nothing.
        }

        
        public void OnAfterRouting(WebContext context, bool handled)
        {
            // Do nothing.
        }
    }
}
