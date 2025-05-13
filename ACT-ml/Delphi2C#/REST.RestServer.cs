using System;
using System.Linq;
using System.Net;
using MVCFramework;
using MVCFramework.Logger;
using MVCFramework.Commons;
using MVCFramework.REPLCommandsHandlerU;
using REST.WebModule;

namespace REST
{
    public class DMVCRestServer : IDisposable
    {
        private HttpListener _server;
        public Type WebModuleClass { get; set; }
        public DMVCRestServer(int port)
        {
            _server = new HttpListener();
            _server.Prefixes.Add($"http://*:{port}/");
            if (WebRequestHandler.Instance != null)
            {
                WebRequestHandler.Instance.WebModuleClass = WebModuleClass;
                WebRequestHandlerProc.MaxConnections = 1024;
            }
        }
        public void Activate()
        {
            _server.Start();
            if (_server.IsListening)
            {
                var prefix = _server.Prefixes.First();
                LogD("Server active on port=" + prefix.Split(':')[2].TrimEnd('/'));
            }
        }
        public void Dispose()
        {
            EnterInShutdownState();
            try
            {
                if (_server.IsListening)
                    _server.Stop();
                _server.Close();
                LogD("Server not active");
            }
            catch (Exception ex)
            {
                LogE("[DMVCRestServer.Dispose] " + ex.Message);
            }
            ReleaseGlobalLogger();
        }
        ~DMVCRestServer()
        {
            Dispose();
        }
        private void EnterInShutdownState() { }
        private void ReleaseGlobalLogger() { }
    }
}
