using System;
using System.Linq;
using MVCFramework;
using MVCFramework.Commons;
using MVCFramework.Logger;

namespace MVCFramework.Middleware
{
    
    public class MVCRedirectMiddleware : IMVCMiddleware
    {
        private readonly string[] _requestedPathInfos;
        private readonly string _redirectToURL;

        public MVCRedirectMiddleware(string[] requestedPathInfos, string redirectToURL)
        {
            _requestedPathInfos = requestedPathInfos;
            _redirectToURL = redirectToURL;
        }

        
        public void OnBeforeRouting(WebContext context, ref bool handled)
        {
            if (!handled)
            {
                string pathInfo = context.Request.PathInfo;
                foreach (var requestedPath in _requestedPathInfos)
                {
                    if (string.Equals(pathInfo, requestedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.RawWebResponse.SendRedirect(_redirectToURL);
                        Logger.LogInfo($"Redirected from [{pathInfo}] to [{_redirectToURL}]");
                        handled = true;
                        break;
                    }
                }
            }
        }

        public void OnBeforeControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, ref bool handled)
        {
            // No additional logic needed.
        }

        public void OnAfterRouting(WebContext context, bool handled)
        {
            // No additional logic needed.
        }

        public void OnAfterControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, bool handled)
        {
            // No additional logic needed.
        }
    }
}
