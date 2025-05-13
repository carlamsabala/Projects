using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Linq;

namespace Horse.Provider.Console
{
    
    public class IdHTTPWebBrokerBridge
    {
        public int MaxConnections { get; set; }
        public int ListenQueue { get; set; }
        public bool KeepAlive { get; set; }
        public int DefaultPort { get; set; }
        public bool Active { get; set; }
        public List<Binding> Bindings { get; } = new List<Binding>();

        
        public Action<string, ref string, ref string, ref bool> OnParseAuthentication { get; set; }
        public Action<ushort, ref bool> OnQuerySSLPort { get; set; }

        public void StartListening()
        {
            
            Console.WriteLine("Server started listening on port " + DefaultPort);
        }

        public void StopListening()
        {
            
            Console.WriteLine("Server stopped listening");
        }
    }

    
    public class Binding
    {
        public string IP { get; set; }
        public int Port { get; set; }
    }

    
    public interface IHorseProviderIOHandleSSL
    {
        bool Active { get; }
        string CertFile { get; }
        string RootCertFile { get; }
        string KeyFile { get; }
        string Method { get; }
        string SSLVersions { get; }
        string CipherList { get; }
        string DHParamsFile { get; }
        Func<string> OnGetPassword { get; }
    }

    
    public class HorseProviderIOHandleSSL : IHorseProviderIOHandleSSL
    {
        public bool Active => true;
        public string CertFile => "cert.pem";
        public string RootCertFile => "root.pem";
        public string KeyFile => "key.pem";
        public string Method => "TLSv1.2";
        public string SSLVersions => "TLSv1.2";
        public string CipherList => "HIGH:!aNULL:!MD5";
        public string DHParamsFile => "dh.pem";
        public Func<string> OnGetPassword => () => "password";

        public static IHorseProviderIOHandleSSL New => new HorseProviderIOHandleSSL();
    }

    
    public static class WebRequestHandler
    {
        public static Type WebModuleClass { get; set; }
        public static int MaxConnections { get; set; }
    }

        public class HorseCore
    {
        private static HorseCore _instance;
        public static HorseCore GetInstance() => _instance ?? (_instance = new HorseCore());
    }

        public static class HorseConstants
    {
        public const string DEFAULT_HOST = "0.0.0.0";
        public const int DEFAULT_PORT = 8080;
        public const int IdListenQueueDefault = 15;
    }

    

    public class HorseProvider : HorseProviderAbstract
    {
        
        private static int _port;
        private static string _host;
        private static bool _running;
        private static EventWaitHandle _event;
        private static int _maxConnections;
        private static int _listenQueue;
        private static bool _keepConnectionAlive;
        private static IdHTTPWebBrokerBridge _idHTTPWebBrokerBridge;
        private static IHorseProviderIOHandleSSL _horseProviderIOHandleSSL;

        
        public static string Host { get => GetHost(); set => SetHost(value); }
        public static int Port { get => GetPort(); set => SetPort(value); }
        public static int MaxConnections { get => GetMaxConnections(); set => SetMaxConnections(value); }
        public static int ListenQueue { get => GetListenQueue(); set => SetListenQueue(value); }
        public static bool KeepConnectionAlive { get => GetKeepConnectionAlive(); set => SetKeepConnectionAlive(value); }
        public static IHorseProviderIOHandleSSL IOHandleSSL { get => GetIOHandleSSL(); set => SetIOHandleSSL(value); }

        
        public override void Listen() => InternalListen();
        public override void StopListen() => InternalStopListen();

        
        public static void Listen(int aPort, string aHost, Action aCallbackListen, Action aCallbackStopListen)
        {
            SetPort(aPort);
            SetHost(aHost);
            SetOnListen(aCallbackListen);
            SetOnStopListen(aCallbackStopListen);
            InternalListen();
        }

        public static void Listen(string aHost, Action aCallbackListen, Action aCallbackStopListen)
        {
            Listen(_port, aHost, aCallbackListen, aCallbackStopListen);
        }

        public static void Listen(int aPort, Action aCallbackListen, Action aCallbackStopListen)
        {
            Listen(aPort, _host, aCallbackListen, aCallbackStopListen);
        }

        public static void Listen(Action aCallbackListen, Action aCallbackStopListen)
        {
            Listen(_port, _host, aCallbackListen, aCallbackStopListen);
        }

        public static bool IsRunning() => _running;

        
        private static IdHTTPWebBrokerBridge GetDefaultHTTPWebBroker()
        {
            if (_idHTTPWebBrokerBridge == null)
            {
                _idHTTPWebBrokerBridge = new IdHTTPWebBrokerBridge();
                
            }
            return _idHTTPWebBrokerBridge;
        }

        private static bool HTTPWebBrokerIsNil() => _idHTTPWebBrokerBridge == null;

        private static EventWaitHandle GetDefaultEvent()
        {
            if (_event == null)
                _event = new EventWaitHandle(false, EventResetMode.AutoReset);
            return _event;
        }

        private static IHorseProviderIOHandleSSL GetDefaultHorseProviderIOHandleSSL()
        {
            if (_horseProviderIOHandleSSL == null)
                _horseProviderIOHandleSSL = HorseProviderIOHandleSSL.New;
            return _horseProviderIOHandleSSL;
        }

        private static string GetDefaultHost() => HorseConstants.DEFAULT_HOST;
        private static int GetDefaultPort() => HorseConstants.DEFAULT_PORT;
        private static string GetHost() => _host;
        private static bool GetKeepConnectionAlive() => _keepConnectionAlive;
        private static IHorseProviderIOHandleSSL GetIOHandleSSL() => GetDefaultHorseProviderIOHandleSSL();
        private static int GetListenQueue() => _listenQueue;
        private static int GetMaxConnections() => _maxConnections;
        private static int GetPort() => _port;

        private static void SetListenQueue(int value) => _listenQueue = value;
        private static void SetMaxConnections(int value) => _maxConnections = value;
        private static void SetPort(int value) => _port = value;
        private static void SetIOHandleSSL(IHorseProviderIOHandleSSL value) => _horseProviderIOHandleSSL = value;
        private static void SetHost(string value) => _host = value?.Trim();
        private static void SetKeepConnectionAlive(bool value) => _keepConnectionAlive = value;

        
        private static Action _onListen;
        private static Action _onStopListen;
        public static void SetOnListen(Action callback) => _onListen = callback;
        public static void SetOnStopListen(Action callback) => _onStopListen = callback;

        private static void DoOnListen()
        {
            _onListen?.Invoke();
        }

        private static void DoOnStopListen()
        {
            _onStopListen?.Invoke();
        }

        
        private static void OnQuerySSLPort(ushort aPort, ref bool vUseSSL)
        {
            vUseSSL = (GetDefaultHorseProviderIOHandleSSL() != null) && GetDefaultHorseProviderIOHandleSSL().Active;
        }

        
        private static void OnAuthentication(object aContext, string aAuthType, string aAuthData, ref string vUsername, ref string vPassword, ref bool vHandled)
        {
            vHandled = true;
        }

        
        private static void InitServerIOHandlerSSLOpenSSL(IdHTTPWebBrokerBridge bridge, IHorseProviderIOHandleSSL ioHandlerSSL)
        {
            
            Console.WriteLine("Initializing SSL IOHandler with certificate: " + ioHandlerSSL.CertFile);
            
        }

        private static void InternalListen()
        {
            if (_port <= 0)
                _port = GetDefaultPort();

            if (string.IsNullOrEmpty(_host))
                _host = GetDefaultHost();

            var bridge = GetDefaultHTTPWebBroker();

            

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
                    InitServerIOHandlerSSLOpenSSL(bridge, GetDefaultHorseProviderIOHandleSSL());

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
                DoOnListen();

                if (IsConsole())
                {
                    while (_running)
                    {
                        GetDefaultEvent().WaitOne();
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsConsole())
                {
                    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                    Console.ReadLine();
                }
                else
                {
                    throw;
                }
            }
        }

        private static void InternalStopListen()
        {
            if (!HTTPWebBrokerIsNil())
            {
                GetDefaultHTTPWebBroker().StopListening();
                GetDefaultHTTPWebBroker().Active = false;
                DoOnStopListen();
                _running = false;
                if (_event != null)
                    GetDefaultEvent().Set();
            }
            else
            {
                throw new Exception("Horse not listen");
            }
        }

        public static new void StopListen() => InternalStopListen();
        public static new void Listen() => InternalListen();

        public static void Listen(int aPort, string aHost, Action aCallbackListen, Action aCallbackStopListen)
        {
            SetPort(aPort);
            SetHost(aHost);
            SetOnListen(aCallbackListen);
            SetOnStopListen(aCallbackStopListen);
            InternalListen();
        }

        public static void Listen(string aHost, Action aCallbackListen, Action aCallbackStopListen)
        {
            Listen(_port, aHost, aCallbackListen, aCallbackStopListen);
        }

        public static void Listen(int aPort, Action aCallbackListen, Action aCallbackStopListen)
        {
            Listen(aPort, _host, aCallbackListen, aCallbackStopListen);
        }

        public static void Listen(Action aCallbackListen, Action aCallbackStopListen)
        {
            Listen(_port, _host, aCallbackListen, aCallbackStopListen);
        }

        public static bool IsRunning() => _running;

        public static void UnInitialize()
        {
            _idHTTPWebBrokerBridge = null;
            if (_event != null)
            {
                _event.Dispose();
                _event = null;
            }
        }

        
        private static bool IsConsole()
        {
            
            return Environment.UserInteractive && Console.WindowHeight > 0;
        }
    }

    
    public abstract class HorseProviderAbstract
    {
        public abstract void Listen();
        public abstract void StopListen();
    }
}
