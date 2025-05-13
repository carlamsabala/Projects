using System;
using System.IO;
using MVCFramework;
using MVCFramework.Commons;
using MVCFramework.Middleware.StaticFiles;
using MVCFramework.HTMX;
using JsonDataObjects;
using MVCFramework.View.Renderers.Mustache;
using uBase.Controller;
using uMovie.Controller;

namespace uConfigModule
{
    public class ConfigModule : WebModule
    {
        private MVCEngine _mvcEngine;
        public override void OnCreate()
        {
            _mvcEngine = new MVCEngine(this, config =>
            {
                config[MVCCOnfigKey.DefaultContentType] = MVCConstants.DEFAULT_CONTENT_TYPE;
                config[MVCCOnfigKey.DefaultContentCharset] = MVCConstants.DEFAULT_CONTENT_CHARSET;
                config[MVCCOnfigKey.AllowUnhandledAction] = "false";
                config[MVCCOnfigKey.LoadSystemControllers] = "true";
                config[MVCCOnfigKey.DefaultViewFileExtension] = "htmx";
                config[MVCCOnfigKey.ViewPath] = "htmx_templates";
                config[MVCCOnfigKey.MaxEntitiesRecordCount] = "20";
                config[MVCCOnfigKey.ExposeServerSignature] = "true";
                config[MVCCOnfigKey.ExposeXPoweredBy] = "true";
                config[MVCCOnfigKey.MaxRequestSize] = MVCConstants.DEFAULT_MAX_REQUEST_SIZE.ToString();
            });
            _mvcEngine.SetViewEngine(new MVCMustacheViewEngine());
            string modulePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string imgPath = Path.Combine(modulePath, @"www\img");
            _mvcEngine.AddMiddleware(new MVCStaticFilesMiddleware("/img", imgPath));
            _mvcEngine.AddController<BaseController>();
            _mvcEngine.AddController<MovieController>();
            _mvcEngine.SetExceptionHandler((ex, selectedController, webContext, out bool exceptionHandled) =>
            {
                exceptionHandled = false;
                if (webContext.Request.IsHTMX)
                {
                    if (selectedController != null)
                    {
                        selectedController.Render(ex.Message);
                        exceptionHandled = true;
                        webContext.Response.StatusCode = 400;
                    }
                }
            });
        }
        public override void OnDestroy()
        {
            _mvcEngine.Dispose();
        }
    }
}
