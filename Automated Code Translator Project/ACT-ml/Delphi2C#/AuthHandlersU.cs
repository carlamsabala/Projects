using System;
using System.Collections.Generic;

namespace AuthHandlerU
{
    public class WebContext
    {
        // Add any context properties as needed.
    }
    
    public interface IMVCAuthenticationHandler
    {
        void OnRequest(WebContext context, string controllerQualifiedClassName, string actionName, ref bool authenticationRequired);
        void OnAuthentication(WebContext context, string userName, string password, List<string> userRoles, ref bool isValid, Dictionary<string, string> sessionData);
        void OnAuthorization(WebContext context, List<string> userRoles, string controllerQualifiedClassName, string actionName, ref bool isAuthorized);
    }
    
    public abstract class AuthHandlerBase : IMVCAuthenticationHandler
    {
        public abstract void OnRequest(WebContext context, string controllerQualifiedClassName, string actionName, ref bool authenticationRequired);
        
        public virtual void OnAuthentication(WebContext context, string userName, string password, List<string> userRoles, ref bool isValid, Dictionary<string, string> sessionData)
        {
            userRoles.Clear();
            isValid = userName == password;
            if (!isValid)
                return;
            if (userName == "user1")
            {
                isValid = true;
                userRoles.Add("role1");
            }
            if (userName == "user2")
            {
                isValid = true;
                userRoles.Add("role2");
            }
        }
        
        public virtual void OnAuthorization(WebContext context, List<string> userRoles, string controllerQualifiedClassName, string actionName, ref bool isAuthorized)
        {
            isAuthorized = false;
            if (actionName == "OnlyRole1" || actionName == "OnlyRole1Session")
                isAuthorized = userRoles.Contains("role1");
            if (actionName == "OnlyRole2")
                isAuthorized = userRoles.Contains("role2");
        }
    }
    
    public class BasicAuthHandler : AuthHandlerBase
    {
        public override void OnRequest(WebContext context, string controllerQualifiedClassName, string actionName, ref bool authenticationRequired)
        {
            authenticationRequired = controllerQualifiedClassName.EndsWith("TTestPrivateServerController");
        }
    }
    
    public class CustomAuthHandler : AuthHandlerBase
    {
        public override void OnRequest(WebContext context, string controllerQualifiedClassName, string actionName, ref bool authenticationRequired)
        {
            authenticationRequired = controllerQualifiedClassName.EndsWith("TTestPrivateServerControllerCustomAuth");
        }
    }
}
