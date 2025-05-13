
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using MVCFramework;
using MVCFramework.Commons;
using MVCFramework.Serializer.Intf;
using MVCFramework.Serializer.Defaults;
using System.Linq;

namespace MVCFramework.View.Renderers
{
    #region Dummy/Placeholder Types

    public abstract class MVCBaseViewEngine
    {
        protected MVCEngine Engine { get; }
        protected TWebContext WebContext { get; }
        protected MVCController Controller { get; }
        protected MVCViewDataObject ViewModel { get; }
        protected string ContentType { get; }
        protected IDictionary<string, string> Config { get; } 
        protected Action<TWebStencilsProcessor> BeforeRenderCallback { get; set; }

        protected MVCBaseViewEngine(MVCEngine engine, TWebContext webContext, MVCController controller, MVCViewDataObject viewModel, string contentType)
        {
            Engine = engine;
            WebContext = webContext;
            Controller = controller;
            ViewModel = viewModel;
            ContentType = contentType;
            Config = engine.Config;
        }

        public abstract void Execute(string viewName, StringBuilder builder);

        protected virtual string GetRealFileName(string viewName)
        {
            string viewPath = Config.ContainsKey("ViewPath") ? Config["ViewPath"] : "Views";
            string extension = Config.ContainsKey("DefaultViewFileExtension") ? Config["DefaultViewFileExtension"] : "html";
            string fullPath = Path.Combine(AppPath, viewPath, $"{viewName}.{extension}");
            return File.Exists(fullPath) ? fullPath : string.Empty;
        }

        protected string AppPath => AppDomain.CurrentDomain.BaseDirectory;
    }

    public class MVCEngine
    {
        public IDictionary<string, string> Config { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ViewPath", "Views" },
            { "DefaultViewFileExtension", "html" },
            { "ViewCache", "true" }
        };
    }

    public class TWebContext
    {
        public TWebRequest Request { get; set; }
        public TWebResponse Response { get; set; }
        public MVCLoggedUser LoggedUser { get; set; }
    }

    public class TWebRequest
    {
        public object RawWebRequest { get; set; }
    }

    public class TWebResponse { }

    public class MVCController { }

    public class MVCViewDataObject : Dictionary<string, TValue> { }

    public interface IMVCSerializer
    {
        string SerializeObject(object obj);
        string SerializeCollection(object collection);
    }

    public static class MVCSerializerDefaults
    {
        public static IMVCSerializer GetDefaultSerializer() => new MVCJsonDataObjectsSerializer();
    }

    public class MVCJsonDataObjectsSerializer : IMVCSerializer
    {
        public string SerializeObject(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
        public string SerializeCollection(object collection)
        {
            return SerializeObject(collection);
        }
    }

    public class TWebStencilsProcessor : IDisposable
    {
        public string InputFileName { get; set; }
        public string PathTemplate { get; set; }
        public object WebRequest { get; set; }
        public bool UserLoggedIn { get; set; }
        public string UserRoles { get; set; }
        public event Action<object, string, ref string, ref bool> OnValue;
        public event Action<object, string, ref string, ref bool> OnFile;

        public string Content
        {
            get
            {
                return File.ReadAllText(InputFileName);
            }
        }

        public void AddVar(string key, object value, bool flag)
        {
            // Implementation to add variable into the processor's model.
        }

        public void Dispose()
        {
            // Dispose resources if necessary.
        }
    }

    public static class WebStencilsGlobalFunctions
    {
        public static bool Initialized { get; set; } = false;
        public static readonly object LockObject = new object();

        public static void RegisterMethod(string name, Func<IValue[], IValue> invokable, string description)
        {
            // Implementation for binding method registration.
        }
    }

    public interface IValue
    {
        TValue GetValue();
    }

    public struct TValue
    {
        private object _value;
        public TValue(object value) { _value = value; }
        public bool IsEmpty => _value == null;
        public bool IsObject => _value != null;
        public object AsObject => _value;
        public int AsInteger => Convert.ToInt32(_value);
        public long AsInt64 => Convert.ToInt64(_value);
        public double AsDouble => Convert.ToDouble(_value);
        public string AsString => Convert.ToString(_value);
        public bool AsBoolean => Convert.ToBoolean(_value);
        public static implicit operator TValue(int value) => new TValue(value);
        public static implicit operator TValue(string value) => new TValue(value);
    }

    public class TValueWrapper : IValue
    {
        private TValue _value;
        public TValueWrapper(object value) { _value = new TValue(value); }
        public TValue GetValue() => _value;
    }

    #endregion

    #region MVCWebStencilsViewEngine

    public class MVCWebStencilsViewEngine : MVCBaseViewEngine
    {
        private bool modelPrepared = false;
        private string jsonModelAsString = "";
        private static TSynMustachePartials partials;
        private static TSynMustacheHelpers helpers;
        private static readonly object globalLock = new object();
        private static bool gPartialsLoaded = false;
        private static bool gHelpersLoaded = false;

        public MVCWebStencilsViewEngine(MVCEngine engine, TWebContext webContext, MVCController controller,
            MVCViewDataObject viewModel, string contentType)
            : base(engine, webContext, controller, viewModel, contentType)
        {
            LoadPartials();
            LoadHelpers();
        }

        
        public void OnGetValue(object sender, string objectName, string propName, ref string replaceText, ref bool handled)
        {
            if (ViewModel != null && ViewModel.TryGetValue(objectName, out TValue value))
            {
                replaceText = GetTValueVarAsString(value, objectName, sender as TWebStencilsProcessor);
                handled = true;
            }
            else
            {
                replaceText = "";
                handled = true;
            }
        }

        
        public void OnGetFile(object sender, string filename, ref string text, ref bool handled)
        {
            if (!Path.IsPathRooted(filename))
            {
                string fullFileName = Path.Combine(Config["ViewPath"], filename);
                fullFileName = Path.ChangeExtension(fullFileName, Config["DefaultViewFileExtension"]);
                fullFileName = Path.Combine(AppPath, fullFileName);
                text = File.ReadAllText(fullFileName);
                handled = true;
            }
        }

        
        public static string GetTValueVarAsString(TValue value, string varName, TWebStencilsProcessor processor)
        {
            if (value.IsEmpty)
                return "";
            if (value.IsObject)
            {
                object obj = value.AsObject;
                if (obj is System.Data.DataRowView) 
                    return obj.ToString();
                else if (obj is JsonObject)
                    return ((JsonObject)obj).ToJSON();
                else
                    return obj.ToString();
            }
            else
            {
                
                if (value.AsObject is int)
                    return value.AsInteger.ToString();
                if (value.AsObject is long)
                    return value.AsInt64.ToString();
                if (value.AsObject is double)
                    return value.AsDouble.ToString();
                if (value.AsObject is bool)
                    return value.AsBoolean.ToString().ToLower();
                if (value.AsObject is string)
                    return value.AsString;
                throw new Exception($"Unsupported type for variable \"{varName}\"");
            }
        }

        
        private void RegisterWSFunctions(TWebStencilsProcessor processor)
        {
            if (WebStencilsGlobalFunctions.Initialized)
                return;
            lock (globalLock)
            {
                if (!WebStencilsGlobalFunctions.Initialized)
                {
                    WebStencilsGlobalFunctions.RegisterMethod("json",
                        (args) =>
                        {
                            if (args.Length != 1)
                                throw new Exception("Expected 1 parameter in 'json' function");
                            object obj = args[0].GetValue().AsObject;
                            return new TValueWrapper(GetDefaultSerializer().SerializeObject(obj));
                        },
                        "Serialize an object to JSON");

                    WebStencilsGlobalFunctions.RegisterMethod("ValueOf",
                        (args) =>
                        {
                            if (args.Length != 1)
                                throw new Exception("Expected 1 parameter in 'ValueOf' function");
                            return new TValueWrapper(GetTValueVarAsString(args[0].GetValue(), "", null));
                        },
                        "Returns inner value of a nullable as string");

                    WebStencilsGlobalFunctions.RegisterMethod("Defined",
                        (args) =>
                        {
                            if (args.Length != 1)
                                throw new Exception("Expected 1 parameter in 'Defined' function");
                            bool exists = (ViewModel != null && ViewModel.ContainsKey(args[0].GetValue().AsString));
                            return new TValueWrapper(exists);
                        },
                        "Returns true if variable is defined");

                    WebStencilsGlobalFunctions.Initialized = true;
                }
            }
        }

        private void LoadPartials()
        {
            if (gPartialsLoaded)
                return;

            lock (globalLock)
            {
                if (!gPartialsLoaded)
                {
                    string viewsExtension = Config["DefaultViewFileExtension"];
                    string viewPath = Config["ViewPath"];
                    string[] files = Directory.GetFiles(viewPath, "*." + viewsExtension, SearchOption.AllDirectories);
                    partials?.Dispose();
                    partials = new TSynMustachePartials();
                    foreach (string file in files)
                    {
                        string partialName = file.Substring(0, file.Length - (viewsExtension.Length + 1))
                            .Replace(Path.DirectorySeparatorChar, '/');
                        partialName = partialName.Substring(viewPath.Length + 1);
                        string content = File.ReadAllText(file);
                        partials.Add(partialName, content);
                    }
                    gPartialsLoaded = string.Equals(Config["ViewCache"], "true", StringComparison.OrdinalIgnoreCase);
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
                    MVCWebStencilsHelpers.RegisterHandlers(ref helpers);
                    gHelpersLoaded = true;
                }
            }
        }

        private void PrepareModels()
        {
            if (modelPrepared)
                return;

            IMVCSerializer serializer = serializerPool.GetFromPool(true) as IMVCSerializer;
            try
            {
                var jsonModel = new JsonObject();
                if (ViewModel != null)
                {
                    foreach (var pair in ViewModel)
                    {
                        ((MVCJsonDataObjectsSerializer)serializer).TValueToJSONObjectProperty(jsonModel, pair.Key, pair.Value,
                            TMVCSerializationType.stDefault, null, null);
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
    }

    #endregion

    #region MVCWebStencilsHelpers

    public static class MVCWebStencilsHelpers
    {
        public delegate void LoadCustomHelpersProc(ref TSynMustacheHelpers mustacheHelpers);
        private static LoadCustomHelpersProc onLoadCustomHelpers;
        public static LoadCustomHelpersProc OnLoadCustomHelpers
        {
            get => onLoadCustomHelpers;
            set => onLoadCustomHelpers = value;
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

    #endregion
}
