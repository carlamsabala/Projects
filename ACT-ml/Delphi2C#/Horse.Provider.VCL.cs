using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Horse.Provider.VCL
{
    #region Stub Types

    public class IdHTTPWebBrokerBridge
    {
        public int MaxConnections { get; set; }
        public int ListenQueue { get; set; }
        public bool KeepAlive { get; set; }
        public int DefaultPort { get; set; }
        public bool Active { get; set; }
        public List<Binding> Bindings { get; } = new List<Binding>();
        public object IOHandler { get; set; }
        public Action OnParseAuthentication { get; set; }
        public Action<ushort, ref bool> OnQuerySSLPort { get; set; }

        public IdHTTPWebBrokerBridge()
        {
            // Initialize default values if needed.
        }

        public void StartListening()
        {
            
            Console.WriteLine($"HTTP Web Broker Bridge listening on port {DefaultPort}.");
        }

        public void StopListening()
        {
            
            Console.WriteLine("HTTP Web Broker Bridge stopped listening.");
        }
    }

    public class Binding
    {
        public string IP { get; set; }
        public int Port { get; set; }
    }

    public class HorseWebModule
    {
        // Your web module implementation goes here.
    }

    public static class WebRequestHandler
    {
        public static Type WebModuleClass { get; set; }
        public static int MaxConnections { get; set; }
    }

    public interface IHorseProviderIOHandleSSL
    {
        bool Active { get; }
        string CertFile { get; }
        string RootCertFile { get; }
        string KeyFile { get; }
        TIdSSLVersion Method { get; }
        TIdSSLVersions SSLVersions { get; }
        string CipherList { get; }
        string DHParamsFile { get; }
        Func<string> OnGetPassword { get; }
    }

    public enum TIdSSLVersion { TLSv1, TLSv1_1, TLSv1_2 }
    [Flags]
    public enum TIdSSLVersions { TLSv1 = 1, TLSv1_1 = 2, TLSv1_2 = 4 }

    public class HorseProviderIOHandleSSL : IHorseProviderIOHandleSSL
    {
        public bool Active => true;
        public string CertFile => "cert.pem";
        public string RootCertFile => "root.pem";
        public string KeyFile => "key.pem";
        public TIdSSLVersion Method => TIdSSLVersion.TLSv1_2;
        public TIdSSLVersions SSLVersions => TIdSSLVersions.TLSv1_2;
        public string CipherList => "HIGH:!aNULL:!MD5";
        public string DHParamsFile => "dh.pem";
        public Func<string> OnGetPassword => () => "password";

        public static IHorseProviderIOHandleSSL New => new HorseProviderIOHandleSSL();
    }

    public static class HorseConstants
    {
        public const string DEFAULT_HOST = "0.0.0.0";
        public const int DEFAULT_PORT = 8080;
        public const int IdListenQueueDefault = 15;
    }

    #endregion

    #region Abstract Base Provider

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

    #endregion

    #region HorseProvider (VCL)

    public static class HorseProvider : HorseProviderAbstract
    {
        private static int _port;
        private static string _host;
        private static bool _running;
        private static int _maxConnections;
        private static int _listenQueue;
        private static bool _keepConnectionAlive;
        private static IdHTTPWebBrokerBridge _httpWebBrokerBridge;
        private static IHorseProviderIOHandleSSL _horseProviderIOHandleSSL;

        public static string Host { get => GetHost(); set => SetHost(value); }
        public static int Port { get => GetPort(); set => SetPort(value); }
        public static int MaxConnections { get => GetMaxConnections(); set => SetMaxConnections(value); }
        public static int ListenQueue { get => GetListenQueue(); set => SetListenQueue(value); }
        public static bool KeepConnectionAlive { get => GetKeepConnectionAlive(); set => SetKeepConnectionAlive(value); }
        public static IHorseProviderIOHandleSSL IOHandleSSL { get => GetIOHandleSSL(); set => SetIOHandleSSL(value); }

        public override void Listen() => InternalListen();

        public override void StopListen() => InternalStopListen();

        public static void Listen(int aPort, string aHost = "0.0.0.0", Action callbackListen = null, Action callbackStopListen = null)
        {
            SetPort(aPort);
            SetHost(aHost);
            SetOnListen(callbackListen);
            SetOnStopListen(callbackStopListen);
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
        private static void SetHost(string value) => _host = value?.Trim();
        private static int GetPort() => _port;
        private static void SetPort(int value) => _port = value;
        private static int GetListenQueue() => _listenQueue;
        private static void SetListenQueue(int value) => _listenQueue = value;
        private static int GetMaxConnections() => _maxConnections;
        private static void SetMaxConnections(int value) => _maxConnections = value;
        private static bool GetKeepConnectionAlive() => _keepConnectionAlive;
        private static void SetKeepConnectionAlive(bool value) => _keepConnectionAlive = value;
        private static int GetDefaultPort() => HorseConstants.DEFAULT_PORT;
        private static string GetDefaultHost() => HorseConstants.DEFAULT_HOST;
        private static IHorseProviderIOHandleSSL GetIOHandleSSL() => _horseProviderIOHandleSSL ?? ( _horseProviderIOHandleSSL = HorseProviderIOHandleSSL.New);

        private static IdHTTPWebBrokerBridge GetDefaultHTTPWebBroker()
        {
            if (HTTPWebBrokerIsNil())
            {
                _httpWebBrokerBridge = new IdHTTPWebBrokerBridge();
                _httpWebBrokerBridge.OnParseAuthentication = () => OnAuthentication(null, null, null, ref dummyUsername, ref dummyPassword, ref dummyHandled);
                _httpWebBrokerBridge.OnQuerySSLPort = (port, ref boolUseSSL) => OnQuerySSLPort(port, ref boolUseSSL);
            }
            return _httpWebBrokerBridge;
        }

        private static string dummyUsername = "";
        private static string dummyPassword = "";
        private static bool dummyHandled = false;

        private static bool HTTPWebBrokerIsNil() => _httpWebBrokerBridge == null;

        private static void SetOnListen(Action callback) => SetOnListenCallback(callback);
        private static void SetOnStopListen(Action callback) => SetOnStopListenCallback(callback);
        private static void SetOnListenCallback(Action callback) => HorseProviderAbstract.SetOnListen(callback);
        private static void SetOnStopListenCallback(Action callback) => HorseProviderAbstract.SetOnStopListen(callback);

        private static void OnQuerySSLPort(ushort aPort, ref bool vUseSSL)
        {
            vUseSSL = (_horseProviderIOHandleSSL != null) && GetIOHandleSSL().Active;
        }

        private static void OnAuthentication(object aContext, string aAuthType, string aAuthData, ref string vUsername, ref string vPassword, ref bool vHandled)
        {
            vHandled = true;
        }

        private static void InitServerIOHandlerSSLOpenSSL(IdHTTPWebBrokerBridge bridge, IHorseProviderIOHandleSSL ioHandlerSSL)
        {
            
            Console.WriteLine("Initializing SSL IOHandler with CertFile: " + ioHandlerSSL.CertFile);
            
        }

        private static void InternalListen()
        {
            if (_port <= 0)
                _port = GetDefaultPort();
            if (string.IsNullOrEmpty(_host))
                _host = GetDefaultHost();

            var bridge = GetDefaultHTTPWebBroker();
            
            WebRequestHandler.WebModuleClass = typeof(Horse.WebModule.HorseWebModule);
            try
            {
                if (_maxConnections > 0)
                {
                    WebRequestHandler.MaxConnections = _maxConnections;
                    bridge.MaxConnections = _maxConnections;
                }

                if (_listenQueue == 0)
                    _listenQueue = HorseConstants.IdListenQueueDefault;

                if (_horseProviderIOHandleSSL != null)
                    InitServerIOHandlerSSLOpenSSL(bridge, GetIOHandleSSL());

                bridge.ListenQueue = _listenQueue;
                bridge.Bindings.Clear();
                if (_host != GetDefaultHost())
                {
                    
                    bridge.Bindings.Add(new Binding { IP = _host, Port = _port });
                }

                bridge.KeepAlive = _keepConnectionAlive;
                bridge.DefaultPort = _port;
                bridge.Active = true;
                bridge.StartListening();
                _running = true;
                HorseProviderAbstract.DoOnListen();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void InternalStopListen()
        {
            if (!HTTPWebBrokerIsNil())
            {
                var bridge = GetDefaultHTTPWebBroker();
                bridge.Active = false;
                _running = false;
                HorseProviderAbstract.DoOnStopListen();
                bridge.StopListening();
            }
            else
            {
                throw new Exception("Horse not listen");
            }
        }

        public static void StopListen() => InternalStopListen();

        public static void Listen() => InternalListen();

        public static void Listen(int aPort, string aHost, Action callbackListen, Action callbackStopListen)
        {
            SetPort(aPort);
            SetHost(aHost);
            SetOnListen(callbackListen);
            SetOnStopListen(callbackStopListen);
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

        class destructor UnInitialize()
        {
            _httpWebBrokerBridge = null;
            return;
        }
    }
}
