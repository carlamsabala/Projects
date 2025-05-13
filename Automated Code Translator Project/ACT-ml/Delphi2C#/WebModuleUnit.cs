using System;
using System.IO;
using MVCFramework;
using MVCFramework.Commons;
using TestServerControllerU;
using TestServerControllerExceptionU;
using SpeedMiddlewareU;
using MVCFramework.Middleware.Authentication;
using MVCFramework.ActiveRecordController;
using TestServerControllerPrivateU;
using AuthHandlersU;
using TestServerControllerJSONRPCU;
using MVCFramework.Middleware.Compression;
using MVCFramework.Middleware.Session;
using MVCFramework.Middleware.StaticFiles;
using FDConnectionConfigU;

namespace WebModuleUnit
{
    public class MainWebModule : WebModule
    {
        public TMVCEngine MVCEngine { get; private set; }
        public MainWebModule()
        {
            WebModuleCreate();
        }
        public void WebModuleCreate()
        {
            MVCEngine = new TMVCEngine(this, config =>
            {
                config[TMVCConfigKey.PathPrefix] = "";
                config[TMVCConfigKey.ViewPath] = Path.Combine(AppPath, @"..\templates");
                config[TMVCConfigKey.DefaultViewFileExtension] = "html";
            });
            MVCEngine
                .AddController(typeof(TestServerController))
                .AddController(typeof(TestServerControllerException))
                .AddController(typeof(TestServerControllerExceptionAfterCreate))
                .AddController(typeof(TestServerControllerExceptionBeforeDestroy))
                .AddController(typeof(TestServerControllerActionFilters))
                .AddController(typeof(TestPrivateServerControllerCustomAuth))
                .AddController(typeof(TestMultiPathController))
                .AddController(typeof(TestActionResultController))
                .AddController(typeof(TestJSONRPCController), "/jsonrpc")
                .AddController(typeof(TestJSONRPCControllerWithGet), "/jsonrpcwithget")
                .AddController(typeof(TMVCActiveRecordController), "/api/entities")
                .PublishObject(() => new TestJSONRPCClass(), "/jsonrpcclass")
                .PublishObject(() => new TestJSONRPCClassWithGET(), "/jsonrpcclasswithget")
                .PublishObject(() => new TestJSONRPCHookClass(), "/jsonrpcclass1")
                .PublishObject(() => new TestJSONRPCHookClassWithGet(), "/jsonrpcclass1withget")
                .PublishObject(() => new TestJSONRPCHookClassWithGet(), "/jsonrpcclass1withget")
                .AddController(typeof(TestFaultController))
                .AddController(() => new TestFault2Controller(), "/testfault2")
                .AddMiddleware(UseMemorySessionMiddleware())
                .AddMiddleware(new TMVCSpeedMiddleware())
                .AddMiddleware(new TMVCCustomAuthenticationMiddleware(new TCustomAuthHandler(), "/system/users/logged"))
                .AddMiddleware(new TMVCStaticFilesMiddleware("/static", "www", "index.html", false))
                .AddMiddleware(new TMVCStaticFilesMiddleware("/spa", "www", "index.html", true))
                .AddMiddleware(new TMVCBasicAuthenticationMiddleware(new TBasicAuthHandler()))
                .AddMiddleware(new TMVCCompressionMiddleware());
#if MSWINDOWS
            MVCEngine.SetViewEngine(new TMVCMustacheViewEngine());
            RegisterOptionalCustomTypesSerializers(MVCEngine.Serializer(MVCMediaType.APPLICATION_JSON));
#endif
        }
    }
}
