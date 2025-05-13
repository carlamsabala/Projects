using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MVCFramework; 
using MVCFramework.Commons; 
using MVCFramework.Logger;

namespace MVCFramework.Middleware.Compression
{
    
    public class MVCCompressionMiddleware : IMVCMiddleware
    {
        private readonly int _compressionThreshold;

        
        public MVCCompressionMiddleware(int compressionThreshold = 1024)
        {
            _compressionThreshold = compressionThreshold;
        }

        public void OnBeforeRouting(WebContext context, ref bool handled)
        {
            // No routing-time actions required.
        }

        public void OnBeforeControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, ref bool handled)
        {
            // Nothing to do before controller action.
        }

        public void OnAfterControllerAction(WebContext context, string controllerQualifiedClassName, string actionName, bool handled)
        {
            // Nothing to do after controller action.
        }

       
        public void OnAfterRouting(WebContext context, bool handled)
        {
            
            if (WebContext.IsLibrary)
            {
                return;
            }

            
            Stream contentStream = context.Response.RawWebResponse.ContentStream;
            if (contentStream == null || contentStream.Length <= _compressionThreshold)
            {
                return;
            }

            
            string acceptEncoding = context.Request.Headers["Accept-Encoding"];
            if (string.IsNullOrWhiteSpace(acceptEncoding))
            {
                return;
            }
            acceptEncoding = acceptEncoding.Trim().ToLowerInvariant();

            
            CompressionType respCompressionType = CompressionType.None;
            
            var encodings = acceptEncoding.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(e => e.Trim());
            foreach (var encoding in encodings)
            {
                if (encoding == "gzip")
                {
                    respCompressionType = CompressionType.GZip;
                    break;
                }
                else if (encoding == "deflate")
                {
                    respCompressionType = CompressionType.Deflate;
                    break;
                }
            }
            if (respCompressionType == CompressionType.None)
            {
                return;
            }

            
            MemoryStream originalContent;
            if (contentStream is MemoryStream memStream)
            {
                originalContent = memStream;
            }
            else
            {
                originalContent = new MemoryStream();
                contentStream.CopyTo(originalContent);
            }
            originalContent.Position = 0;

            
            var compressedStream = new MemoryStream();
            try
            {
                
                if (respCompressionType == CompressionType.GZip)
                {
                    using (var gzip = new GZipStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
                    {
                        originalContent.CopyTo(gzip);
                    }
                }
                else if (respCompressionType == CompressionType.Deflate)
                {
                    using (var deflate = new DeflateStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
                    {
                        originalContent.CopyTo(deflate);
                    }
                }
                
                compressedStream.Position = 0;
                
                context.Response.RawWebResponse.ContentStream = compressedStream;
                
                context.Response.RawWebResponse.ContentEncoding = 
                    respCompressionType == CompressionType.GZip ? "gzip" : "deflate";
            }
            catch (Exception ex)
            {
                
                Logger.LogError("Compression failed: " + ex.Message);
                compressedStream.Dispose();
                throw;
            }
        }

       
        private enum CompressionType
        {
            None,
            GZip,
            Deflate
        }
    }
}
