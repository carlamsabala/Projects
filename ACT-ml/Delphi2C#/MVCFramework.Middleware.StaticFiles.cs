using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MVCFramework;
using MVCFramework.Commons;   
using MVCFramework.Logger;    

namespace MVCFramework.Middleware
{
    
    public static class MvcStaticFilesDefaults
    {
        
        public const string StaticFilesPath = "/static";

        
        public const string DocumentRoot = @".\www";

        
        public const string IndexDocument = "index.html";

        
        public const string StaticFilesContentCharset = MVCConstants.DefaultContentCharset;
    }

    
    public delegate void MvcStaticFileRulesProc(IWebContext context, ref string pathInfo, ref bool handled);

    
    public delegate void MvcStaticFileMediaTypesCustomizer(Dictionary<string, string> mediaTypes);

    
    public class MvcStaticFilesMiddleware : IMiddleware
    {
        private bool sanityCheckOK;
        private readonly Dictionary<string, string> mediaTypes;
        private readonly string staticFilesPath;
        private readonly string documentRoot;
        private readonly string indexDocument;
        private readonly string staticFilesCharset;
        private readonly bool spaWebAppSupport;
        private readonly MvcStaticFileRulesProc rules;

        
        public MvcStaticFilesMiddleware(
            string staticFilesPath = MvcStaticFilesDefaults.StaticFilesPath,
            string documentRoot = MvcStaticFilesDefaults.DocumentRoot,
            string indexDocument = MvcStaticFilesDefaults.IndexDocument,
            bool spaWebAppSupport = true,
            string staticFilesCharset = MvcStaticFilesDefaults.StaticFilesContentCharset,
            MvcStaticFileRulesProc rules = null,
            MvcStaticFileMediaTypesCustomizer mediaTypesCustomizer = null)
        {
            sanityCheckOK = false;
            this.staticFilesPath = staticFilesPath.Trim();
            if (!this.staticFilesPath.EndsWith("/"))
            {
                this.staticFilesPath += "/";
            }

            
            if (Directory.Exists(documentRoot))
            {
                this.documentRoot = Path.GetFullPath(documentRoot);
            }
            else
            {
                
                this.documentRoot = Path.Combine(MVCConstants.AppPath, documentRoot);
            }

            this.indexDocument = indexDocument;
            this.staticFilesCharset = staticFilesCharset;
            this.spaWebAppSupport = spaWebAppSupport;
            this.rules = rules;

            mediaTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AddMediaTypes();
            mediaTypesCustomizer?.Invoke(mediaTypes);
        }

        private void AddMediaTypes()
        {
            mediaTypes[".html"] = MVCMediaType.TextHtml;
            mediaTypes[".htm"] = MVCMediaType.TextHtml;
            mediaTypes[".txt"] = MVCMediaType.TextPlain;
            mediaTypes[".text"] = MVCMediaType.TextPlain;
            mediaTypes[".csv"] = MVCMediaType.TextCsv;
            mediaTypes[".css"] = MVCMediaType.TextCss;
            mediaTypes[".js"] = MVCMediaType.TextJavascript;
            mediaTypes[".json"] = MVCMediaType.ApplicationJson;
            mediaTypes[".jpg"] = MVCMediaType.ImageJpeg;
            mediaTypes[".jpeg"] = MVCMediaType.ImageJpeg;
            mediaTypes[".jpe"] = MVCMediaType.ImageJpeg;
            mediaTypes[".png"] = MVCMediaType.ImagePng;
            mediaTypes[".ico"] = MVCMediaType.ImageXIcon;
            mediaTypes[".appcache"] = MVCMediaType.TextCacheManifest;
            mediaTypes[".svg"] = MVCMediaType.ImageSvgXml;
            mediaTypes[".xml"] = MVCMediaType.TextXml;
            mediaTypes[".pdf"] = MVCMediaType.ApplicationPdf;
            mediaTypes[".svgz"] = MVCMediaType.ImageSvgXml;
            mediaTypes[".gif"] = MVCMediaType.ImageGif;
        }

        private void DoSanityCheck()
        {
            if (!staticFilesPath.StartsWith("/"))
            {
                throw new Exception("StaticFilePath must begin with '/' and cannot be empty");
            }
            if (!Directory.Exists(documentRoot))
            {
                throw new Exception($"MvcStaticFilesMiddleware Error: DocumentRoot [{documentRoot}] is not a valid directory");
            }
            sanityCheckOK = true;
        }

        
        private bool SendStaticFileIfPresent(IWebContext context, string fileName)
        {
            if (File.Exists(fileName))
            {
                string contentType;
                string ext = Path.GetExtension(fileName).ToLower();
                if (!mediaTypes.TryGetValue(ext, out contentType))
                {
                    contentType = BuildContentType(MVCMediaType.ApplicationOctetStream, "");
                }
                else
                {
                    contentType = BuildContentType(contentType, staticFilesCharset);
                }
                
                MvcStaticContents.SendFile(fileName, contentType, context);
                Log.Info($"{context.Request.HttpMethod}:{context.Request.PathInfo} [{context.Request.ClientIp}] -> {GetType().Name} - {context.Response.StatusCode} {context.Response.ReasonPhrase}");
                return true;
            }
            return false;
        }

        
        private string BuildContentType(string baseContentType, string charset)
        {
            if (string.IsNullOrEmpty(charset))
            {
                return baseContentType;
            }
            return $"{baseContentType}; charset={charset}";
        }

        public void OnBeforeRouting(IWebContext context, ref bool handled)
        {
            
            string pathInfo = context.Request.PathInfo;

            
            if (!pathInfo.StartsWith(staticFilesPath, StringComparison.OrdinalIgnoreCase))
            {
                if (!pathInfo.EndsWith("/"))
                {
                    
                    pathInfo += "/";
                    if (!pathInfo.StartsWith(staticFilesPath, StringComparison.OrdinalIgnoreCase))
                    {
                        handled = false;
                        return;
                    }
                }
                else
                {
                    handled = false;
                    return;
                }
            }

            
            if (rules != null)
            {
                bool allow = true;
                rules(context, ref pathInfo, ref allow);
                if (!allow)
                {
                    handled = true;
                    return;
                }
            }

            
            if (pathInfo.StartsWith(staticFilesPath, StringComparison.OrdinalIgnoreCase))
            {
                pathInfo = pathInfo.Substring(staticFilesPath.Length);
            }
            
            pathInfo = pathInfo.Replace("/", Path.DirectorySeparatorChar.ToString());
            if (pathInfo.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                pathInfo = pathInfo.Substring(1);
            }
            
            string fullPathInfo = Path.Combine(documentRoot, pathInfo);

            if (!sanityCheckOK)
            {
                DoSanityCheck();
            }

            
            bool isDirTraversal;
            string realFileName = MvcStaticContents.IsStaticFile(documentRoot, pathInfo, out isDirTraversal);
            if (!string.IsNullOrEmpty(realFileName))
            {
                if (isDirTraversal)
                {
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    handled = true;
                    return;
                }
                handled = SendStaticFileIfPresent(context, realFileName);
                if (handled)
                {
                    return;
                }
            }

            
            if (Directory.Exists(fullPathInfo))
            {
                
                if (!context.Request.PathInfo.EndsWith("/"))
                {
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.MovedPermanently;
                    context.Response.CustomHeaders["Location"] = context.Request.PathInfo + "/";
                    handled = true;
                    return;
                }
                
                if (!string.IsNullOrEmpty(indexDocument))
                {
                    handled = SendStaticFileIfPresent(context, Path.Combine(fullPathInfo, indexDocument));
                    return;
                }
            }

            
            if (!handled && spaWebAppSupport && !string.IsNullOrEmpty(indexDocument))
            {
                while (!string.IsNullOrEmpty(fullPathInfo) && !Directory.Exists(fullPathInfo))
                {
                    DirectoryInfo parent = Directory.GetParent(fullPathInfo);
                    fullPathInfo = parent?.FullName ?? "";
                }
                string fileName = Path.GetFullPath(Path.Combine(fullPathInfo, indexDocument));
                handled = SendStaticFileIfPresent(context, fileName);
            }
        }

        public void OnBeforeControllerAction(IWebContext context, string controllerQualifiedClassName, string actionName, ref bool handled)
        {
            // No action needed here.
        }

        public void OnAfterControllerAction(IWebContext context, string controllerQualifiedClassName, string actionName, bool handled)
        {
            // No action needed here.
        }

        public void OnAfterRouting(IWebContext context, bool handled)
        {
            // No action needed here.
        }
    }
}
