
using System;
using System.Collections.Generic;

namespace MVCFramework.Server
{
    public class MVCServerException : Exception
    {
        public MVCServerException() { }
        public MVCServerException(string message) : base(message) { }
        public MVCServerException(string message, Exception innerException) : base(message, innerException) { }
    }

    public interface IMVCListenerProperties
    {
        string Name { get; set; }
        IMVCListenerProperties SetName(string value);

        int Port { get; set; }
        IMVCListenerProperties SetPort(int value);

        int MaxConnections { get; set; }
        IMVCListenerProperties SetMaxConnections(int value);

        Type WebModuleClass { get; set; }
        IMVCListenerProperties SetWebModuleClass(Type value);

        
        bool TryGetSSLOptions(out string sslCertFile, out string sslRootCertFile, out string sslKeyFile, out string sslPassword);
        
        IMVCListenerProperties SetSSLOptions(string sslCertFile, string sslRootCertFile, string sslKeyFile, string sslPassword);
    }

    
    public interface IMVCListener
    {
        bool Active { get; }
        void Start();
        void Stop();
    }

    
    public interface IMVCListenersContext
    {
        
        IMVCListenersContext Add(string name, IMVCListener listener);
        
        IMVCListenersContext Add(IMVCListenerProperties properties);
        
        IMVCListenersContext Remove(string listenerName);

        
        void StartAll();
        
        void StopAll();

        
        IMVCListener FindByName(string listenerName);

        
        void ForEach(Action<string, IMVCListener> action);
        
        int Count { get; }
    }

    
    public delegate void MVCRequestDelegate(string controllerQualifiedClassName, string actionName, ref bool authenticationRequired);

    
    public delegate void MVCAuthenticationDelegate(string userName, string password, List<string> userRoles, ref bool isValid, IDictionary<string, string> sessionData);

    
    public delegate void MVCAuthorizationDelegate(List<string> userRoles, string controllerQualifiedClassName, string actionName, ref bool isAuthorized);

    
    public interface IMVCDefaultAuthenticationHandler : IMVCAuthenticationHandler
    {
        IMVCDefaultAuthenticationHandler SetOnRequest(MVCRequestDelegate method);
        IMVCDefaultAuthenticationHandler SetOnAuthentication(MVCAuthenticationDelegate method);
        IMVCDefaultAuthenticationHandler SetOnAuthorization(MVCAuthorizationDelegate method);
    }

    
}
