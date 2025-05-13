using System;
using System.Threading;

namespace Horse.Provider.FPC.HTTPApplication
{
    
    public class HTTPApplication
    {
        public bool AllowDefaultModule { get; set; }
        public OnGetModuleDelegate OnGetModule { get; set; }
        public bool Threaded { get; set; }
        public int QueueSize { get; set; }
        public int Port { get; set; }
        public bool LegacyRouting { get; set; }
        public string Address { get; set; }
        public bool Active { get; set; }

        
        public void Initialize()
        {
            
            Console.WriteLine("HTTPApplication initialized.");
        }

        
        public void Run()
        {
            Active = true;
            Console.WriteLine($"HTTPApplication running on {Address}:{Port}.");
            
            while (Active)
            {
                Thread.Sleep(1000);
            }
        }
    }

    
    public delegate void OnGetModuleDelegate(object sender, Request request, out Type moduleClass);

    
    public class Request
    {
        // Define request properties as needed.
    }

    
    public class HorseWebModule
    {
        // Define your web module logic here.
    }

    
    public static class WebRequestHandler
    {
        public static Type WebModuleClass { get; set; }
    }

    
    public static class HorseConstants
    {
        public const string DEFAULT_HOST = "0.0.0.0";
        public const int DEFAULT_PORT = 8080;
        public const int IdListenQueueDefault = 15;
    }

    
    public abstract class HorseProviderAbstract
    {
        public abstract void Listen();
        public abstract void StopListen();

        
        protected static Action OnListenCallback;
        protected static Action OnStopListenCallback;

        protected static void DoOnListen() => OnListenCallback?.Invoke();
        protected static void DoOnStopListen() => OnStopListenCallback?.Invoke();

        public static void SetOnListen(Action callback) => OnListenCallback = callback;
        public static void SetOnStopListen(Action callback) => OnStopListenCallback = callback;
    }

    
    public static class HorseProvider 
    {
        private static int _port;
        private static string _host;
        private static bool _running;
        private static int _listenQueue;
        private static HTTPApplication _httpApplication;

        
        public static string Host { get => GetHost(); set => SetHost(value); }
        public static int Port { get => GetPort(); set => SetPort(value); }
        public static int ListenQueue { get => GetListenQueue(); set => SetListenQueue(value); }

        
        public static void Listen() => InternalListen();

        public static void Listen(int aPort, string aHost = "0.0.0.0", Action callbackListen = null)
        {
            SetPort(aPort);
            SetHost(aHost);
            HorseProviderAbstract.SetOnListen(callbackListen);
            InternalListen();
        }

        public static void Listen(string aHost, Action callbackListen)
        {
            Listen(_port, aHost, callbackListen);
        }

        public static void Listen(int aPort, Action callbackListen)
        {
            Listen(aPort, _host, callbackListen);
        }

        public static void Listen(Action callbackListen)
        {
            Listen(_port, _host, callbackListen);
        }

        public static void StopListen() => InternalStopListen();

        public static bool IsRunning() => _running;

        private static string GetHost() => _host;
        private static void SetHost(string value) => _host = value?.Trim();
        private static int GetPort() => _port;
        private static void SetPort(int value) => _port = value;
        private static int GetListenQueue() => _listenQueue;
        private static void SetListenQueue(int value) => _listenQueue = value;
        private static int GetDefaultPort() => HorseConstants.DEFAULT_PORT;
        private static string GetDefaultHost() => HorseConstants.DEFAULT_HOST;

        private static HTTPApplication GetDefaultHTTPApplication()
        {
            if (HTTPApplicationIsNil())
            {
                _httpApplication = new HTTPApplication();
            }
            return _httpApplication;
        }

        private static bool HTTPApplicationIsNil() => _httpApplication == null;

        private static void DoGetModule(object sender, Request request, out Type moduleClass)
        {
            moduleClass = typeof(HorseWebModule);
        }

        private static void InternalListen()
        {
            if (_port <= 0)
                _port = GetDefaultPort();
            if (string.IsNullOrEmpty(_host))
                _host = GetDefaultHost();
            if (_listenQueue == 0)
                _listenQueue = HorseConstants.IdListenQueueDefault;

            var app = GetDefaultHTTPApplication();
            app.AllowDefaultModule = true;
            app.OnGetModule = DoGetModule;
            app.Threaded = true;
            app.QueueSize = _listenQueue;
            app.Port = _port;
            app.LegacyRouting = true;
            app.Address = _host;
            app.Initialize();
            _running = true;
            HorseProviderAbstract.DoOnListen();
            app.Run();
        }

        private static void InternalStopListen()
        {
            if (!HTTPApplicationIsNil())
            {
                var app = GetDefaultHTTPApplication();
                app.Active = false;
                HorseProviderAbstract.DoOnStopListen();
                _running = false;
            }
            else
            {
                throw new Exception("Horse not listen");
            }
        }
    }
}
