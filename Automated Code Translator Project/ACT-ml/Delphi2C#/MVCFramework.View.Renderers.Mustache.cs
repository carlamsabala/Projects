using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using MVCFramework;
using MVCFramework.Commons;
using MVCFramework.Serializer.Intf;
using MVCFramework.IntfObjectPool;
using mORMot.Core.Mustache;
using mORMot.Core.Unicode;

namespace MVCFramework.View.Renderers
{
    
    public class MVCMustacheViewEngine : MVCBaseViewEngine
    {
        private bool modelPrepared;
        private string jsonModelAsString;

        private static TSynMustachePartials partials;
        private static TSynMustacheHelpers helpers;
        private static IIntfObjectPool serializerPool;
        private static readonly object globalLock = new object();
        private static bool gPartialsLoaded = false;
        private static bool gHelpersLoaded = false;

        
        public MVCMustacheViewEngine(MVCEngine engine, TWebContext webContext, MVCController controller,
            MVCViewDataObject viewModel, string contentType)
            : base(engine, webContext, controller, viewModel, contentType)
        {
            modelPrepared = false;
            LoadPartials();
            LoadHelpers();
        }

        static MVCMustacheViewEngine()
        {
            serializerPool = TIntfObjectPool.Create(10000, 10, 1, () =>
            {
                var serializer = new MVCJsonDataObjectsSerializer();
                MVCSerializerOptionalCustomTypes.RegisterOptionalCustomTypesSerializers(serializer);
                return serializer;
            });
        }

        
        protected virtual string RenderJSON(TSynMustache viewEngine, string json, TSynMustachePartials partials,
            TSynMustacheHelpers helpers, TOnStringTranslate onTranslate, bool escapeInvert)
        {
            return viewEngine.RenderJSON(json, partials, helpers, onTranslate, escapeInvert);
        }

        
        public override void Execute(string viewName, StringBuilder builder)
        {
            PrepareModels();

            string viewFileName = GetRealFileName(viewName);
            if (string.IsNullOrEmpty(viewFileName))
                throw new EMVCSSVException($"View [{viewName}] not found");

            string viewTemplate = File.ReadAllText(viewFileName, Encoding.UTF8);
            TSynMustache viewEngine = TSynMustache.Parse(viewTemplate);
            string rendered = RenderJSON(viewEngine, jsonModelAsString, partials, helpers, null, false);
            builder.Append(rendered);
        }

        
        private void PrepareModels()
        {
            if (modelPrepared)
                return;

            var serializer = (IMVCSerializer)serializerPool.GetFromPool(true);
            try
            {
                var jsonModel = new JsonObject();
                if (ViewModel != null)
                {
                    foreach (var dataPair in ViewModel)
                    {
                        ((MVCJsonDataObjectsSerializer)serializer).TValueToJSONObjectProperty(jsonModel, dataPair.Key,
                            dataPair.Value, TMVCSerializationType.stDefault, null, null);
                    }
                }
                jsonModelAsString = jsonModel.ToJSON();
            }
            finally
            {
                serializerPool.ReleaseToPool(serializer);
            }
            modelPrepared = true;
        }

        private void LoadPartials()
        {
            if (gPartialsLoaded)
                return;

            lock (globalLock)
            {
                if (!gPartialsLoaded)
                {
                    string viewsExtension = Config[TMVCConfigKey.DefaultViewFileExtension];
                    string viewPath = Config[TMVCConfigKey.ViewPath];
                    string[] partialFiles = Directory.GetFiles(viewPath, "*." + viewsExtension, SearchOption.AllDirectories);

                    partials?.Dispose();
                    partials = new TSynMustachePartials();

                    foreach (string file in partialFiles)
                    {
                        string partialName = file.Substring(0, file.Length - (viewsExtension.Length + 1))
                            .Replace(Path.DirectorySeparatorChar, '/');
                        partialName = partialName.Substring(viewPath.Length + 1);
                        string content = File.ReadAllText(file);
                        partials.Add(partialName, content);
                    }
                    gPartialsLoaded = string.Equals(Config[TMVCConfigKey.ViewCache], "true", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        private void LoadHelpers()
        {
            if (gHelpersLoaded)
                return;

            lock (globalLock)
            {
                if (!gHelpersLoaded)
                {
                    helpers = TSynMustache.HelpersGetStandardList();
                    MVCMustacheHelpers.RegisterHandlers(ref helpers);
                    gHelpersLoaded = true;
                }
            }
        }
    }

    
    public static class MVCMustacheHelpers
    {
        public delegate void TLoadCustomHelpersProc(ref TSynMustacheHelpers mustacheHelpers);

        private static TLoadCustomHelpersProc onLoadCustomHelpers;
        public static TLoadCustomHelpersProc OnLoadCustomHelpers
        {
            get { return onLoadCustomHelpers; }
            set { onLoadCustomHelpers = value; }
        }

        
        public static void RegisterHandlers(ref TSynMustacheHelpers mustacheHelpers)
        {
            TSynMustache.HelperAdd(mustacheHelpers, "UpperCase", new TSynMustacheHelperDelegate(ToUpperCase));
            TSynMustache.HelperAdd(mustacheHelpers, "LowerCase", new TSynMustacheHelperDelegate(ToLowerCase));
            TSynMustache.HelperAdd(mustacheHelpers, "Capitalize", new TSynMustacheHelperDelegate(Capitalize));
            TSynMustache.HelperAdd(mustacheHelpers, "SnakeCase", new TSynMustacheHelperDelegate(SnakeCase));

            onLoadCustomHelpers?.Invoke(ref mustacheHelpers);
        }

        public static void ToLowerCase(object value, out object result)
        {
            result = value?.ToString().ToLower();
        }

        public static void ToUpperCase(object value, out object result)
        {
            result = value?.ToString().ToUpper();
        }

        public static void Capitalize(object value, out object result)
        {
            string s = value?.ToString();
            if (!string.IsNullOrEmpty(s))
                result = char.ToUpper(s[0]) + s.Substring(1).ToLower();
            else
                result = s;
        }

        public static void SnakeCase(object value, out object result)
        {
            string s = value?.ToString();
            if (string.IsNullOrEmpty(s))
            {
                result = s;
                return;
            }
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsUpper(s[i]))
                {
                    if (i > 0)
                        sb.Append('_');
                    sb.Append(char.ToLower(s[i]));
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            result = sb.ToString();
        }
    }
}
