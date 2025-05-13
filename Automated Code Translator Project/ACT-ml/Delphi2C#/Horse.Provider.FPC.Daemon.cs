using System;
using System.Collections.Generic;
using System.Threading;

namespace Horse.Provider.FPC.Daemon
{
    #region Stub Classes and Interfaces

    
    public class FPHTTPServer
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public bool Threaded { get; set; }
        public int QueueSize { get; set; }
        public bool Active { get; set; }

        
        public event Action<object, FPHTTPConnectionRequest, FPHTTPConnectionResponse> OnRequest;

        
        public List<Binding> Bindings { get; } = new List<Binding>();

        public FPHTTPServer()
        {
            // Initialize as needed.
        }

        public void StartListening()
        {
            
            Console.WriteLine($"FPHTTPServer listening on {HostName}:{Port} with queue size {QueueSize}.");
            Active = true;
        }

        public void StopListening()
        {
           
            Console.WriteLine("FPHTTPServer stopped listening.");
            Active = false;
        }
    }

    
    public class Binding
    {
        public string IP { get; set; }
        public int Port { get; set; }
    }

    
    public class FPHTTPConnectionRequest
    {
        public string Content { get; set; }
    }

    
    public class FPHTTPConnectionResponse
    {
        public string Content { get; set; }
        public int Code { get; set; }
    }

    
    public class THorseRequest : IDisposable
    {
        public THorseRequest(FPHTTPConnectionRequest req)
        {
            // Wrap the request.
        }
        public void Dispose()
        {
            // Clean up.
        }
    }

    public class THorseResponse : IDisposable
    {
        public THorseResponse(FPHTTPConnectionResponse resp)
        {
            // Wrap the response.
        }
        public void Send(string content)
        {
            Console.WriteLine("Response sent: " + content);
        }
        public void Status(int code)
        {
            Console.WriteLine("Response status: " + code);
        }
        public void Dispose()
        {
            // Clean up.
        }
    }

    
    public class HorseCore
    {
        private static HorseCore _instance;
        public static HorseCore GetInstance() => _instance ?? (_instance = new HorseCore());
        public RoutesCollection Routes { get; } = new RoutesCollection();
    }

    
    public class RoutesCollection
    {
        
        public bool Execute(THorseRequest request, THorseResponse response)
        {
            
            return false;
        }
    }

    
    public static class HTTPStatus
    {
        public static int NotFound => 404;
    }

    #endregion

    #region HTTPServerThread

    
    public class HTTPServerThread
    {
        private Thread _thread;
        private volatile bool _startServer;
        private readonly FPHTTPServer _server;
        private readonly HorseCore _horse;
        public string Host { get; set; }
        public int Port { get; set; }
        public ushort ListenQueue { get; set; }

        public HTTPServerThread(bool createSuspended)
        {
            _server = new FPHTTPServer();
            
            _server.OnRequest += OnRequest;
            _horse = HorseCore.GetInstance();
            _startServer = false;

            
            _thread = new Thread(Execute) { IsBackground = true };
            if (!createSuspended)
            {
                _thread.Start();
            }
        }

        
        public void StartServer()
        {
            _startServer = true;
            if (!_thread.IsAlive)
                _thread.Start();
        }

        
        public void StopServer()
        {
            _startServer = false;
            _server.Active = _startServer;
        }

        
        private void Execute()
        {
            while (true)
            {
                if (_startServer)
                {
                    _server.HostName = Host;
                    _server.Port = Port;
                    _server.Threaded = true;
                    _server.QueueSize = ListenQueue;
                    _server.Active = true;
                    _server.StartListening();
                }
                
                Thread.Sleep(100);
            }
        }

        
        private void OnRequest(object sender, FPHTTPConnectionRequest req, FPHTTPConnectionResponse resp)
        {
            using (var horseReq = new THorseRequest(req))
            using (var horseResp = new THorseResponse(resp))
            {
                try
                {
                    if (!_horse.Routes.Execute(horseReq, horseResp))
                    {
                        resp.Content = "Not Found";
                        resp.Code = HTTPStatus.NotFound;
                    }
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine("Exception in OnRequest: " + ex.Message);
                }
            }
        }
    }

    #endregion

    #region HorseProvider

    
    public abstract class HorseProviderAbstract
    {
        public abstract void Listen();
        public abstract void StopListen();
    }

    
    public static class HorseProvider : HorseProviderAbstract
    {
        private static int _port;
        private static string _host;
        private static bool _running;
        private static int _listenQueue;
        private static HTTPServerThread _httpServerThread;

        
        public static string Host { get => GetHost(); set => SetHost(value); }
        public static int Port { get => GetPort(); set => SetPort(value); }
        public static int ListenQueue { get => _listenQueue; set => _listenQueue = value; }

        public override void Listen() => InternalListen();

        public override void StopListen() => InternalStopListen();

        public static void Listen(int aPort, string aHost, Action callbackListen, Action callbackStopListen)
        {
            SetPort(aPort);
            SetHost(aHost);
            
            InternalListen();
        }

        public static void Listen(string aHost, Action callbackListen, Action callbackStopListen)
        {
            Listen(_port, aHost, callbackListen, callbackStopListen);
        }

        public static void Listen(int aPort, Action callbackListen, Action callbackStopListen)
        {
            Listen(aPort, _host, callbackListen, callbackStopListen);
        }

        public static void Listen(Action callbackListen, Action callbackStopListen)
        {
            Listen(_port, _host, callbackListen, callbackStopListen);
        }

        public static bool IsRunning() => _running;

        private static string GetHost() => _host;
        private static void SetHost(string value) => _host = value;
        private static int GetPort() => _port;
        private static void SetPort(int value) => _port = value;
        private static int GetListenQueue() => _listenQueue;

        private static void InternalListen()
        {
            if (_port <= 0)
                _port = DEFAULT_PORT;
            if (string.IsNullOrEmpty(_host))
                _host = DEFAULT_HOST;
            if (_listenQueue == 0)
                _listenQueue = 15;

            if (_httpServerThread == null)
            {
                _httpServerThread = new HTTPServerThread(createSuspended: true);
            }
            _httpServerThread.Port = _port;
            _httpServerThread.Host = _host;
            _httpServerThread.ListenQueue = (ushort)_listenQueue;
            _httpServerThread.StartServer();
            _running = true;
            
        }

        private static void InternalStopListen()
        {
            if (_httpServerThread != null)
            {
                _httpServerThread.StopServer();
                _running = false;
            }
            else
            {
                throw new Exception("Horse not listen");
            }
        }

        public static void UnInitialize()
        {
            _httpServerThread = null;
        }

        
        private const string DEFAULT_HOST = "0.0.0.0";
        private const int DEFAULT_PORT = 8080;
    }

    #endregion
}
