using System;
using System.ServiceProcess;
using System.Threading;
using MVCFramework;
using MVCFramework.Commons;
using Web.WebReq;
using WebModuleUnit1;
using IdHTTPWebBrokerBridge;

namespace REST.RestServer
{
    public class ArticlesService : ServiceBase
    {
        private HTTPWebBrokerBridge fServer;
        private Thread serviceThread;
        private volatile bool terminated;
        public ArticlesService()
        {
            this.ServiceName = "ArticlesService";
        }
        protected override void OnStart(string[] args)
        {
            if (WebRequestHandler.Instance != null)
            {
                WebRequestHandler.Instance.WebModuleClass = typeof(WebModule);
            }
            fServer = new HTTPWebBrokerBridge();
            fServer.OnParseAuthentication += MVCParseAuthentication.OnParseAuthentication;
            fServer.DefaultPort = 8080;
            fServer.Active = true;
            terminated = false;
            serviceThread = new Thread(ServiceExecute);
            serviceThread.Start();
        }
        protected override void OnStop()
        {
            terminated = true;
            serviceThread.Join();
            if (fServer != null)
            {
                fServer.Active = false;
                fServer.Dispose();
                fServer = null;
            }
        }
        private void ServiceExecute()
        {
            while (!terminated)
            {
                ServiceThread.ProcessRequests(true);
                Thread.Sleep(1000);
            }
        }
        public static void Main()
        {
            ServiceBase.Run(new ArticlesService());
        }
    }
}
