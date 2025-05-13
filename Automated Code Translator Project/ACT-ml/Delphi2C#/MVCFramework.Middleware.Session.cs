using System;
using MVCFramework.Session;
using MVCFramework.Middleware.Session.Internal;
using MVCFramework.Session.Database; 

namespace MVCFramework.Middleware.Session
{
    public static class SessionMiddlewareExtensions
    {
        
        public static SessionMiddleware UseMemorySessionMiddleware(int timeoutInMinutes = 0)
        {
            return new SessionMiddleware(MVCWebSessionMemoryFactory.Create(timeoutInMinutes));
        }

        
        public static SessionMiddleware UseFileSessionMiddleware(int timeoutInMinutes = 0, string sessionFolder = "dmvc_sessions")
        {
            
            return new SessionMiddleware(MVCWebSessionFileFactory.Create(timeoutInMinutes, sessionFolder));
        }

        
        public static SessionMiddleware UseDatabaseSessionMiddleware(int timeoutInMinutes = 0)
        {
            
            return new SessionMiddleware(MVCWebSessionDatabaseFactory.Create(timeoutInMinutes, "notused"));
        }
    }
}
