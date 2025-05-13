using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Horse.Core.Files;      
using Horse.Mime;           

namespace Horse.Response
{
    
    public class WebResponse
    {
        public string Content { get; set; }
        public int StatusCode { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public Stream ContentStream { get; set; }
        public bool FreeContentStream { get; set; }
        public Dictionary<string, string> CustomHeaders { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public void SetCustomHeader(string name, string value)
        {
            CustomHeaders[name] = value;
        }

        public void SendResponse()
        {
            
            Console.WriteLine("Response sent.");
        }
    }

    
    public enum THTTPStatus
    {
        Ok = 200,
        SeeOther = 303,
        
    }

    public static class THTTPStatusExtensions
    {
        public static int ToInteger(this THTTPStatus status) => (int)status;
    }

    public class HorseResponse : IDisposable
    {
        private WebResponse _webResponse;
        private object _content; 

        public HorseResponse(WebResponse webResponse)
        {
            _webResponse = webResponse;
            _webResponse.StatusCode = THTTPStatus.Ok.ToInteger();
            
            _webResponse.FreeContentStream = true;
        }

        
        public HorseResponse AddHeader(string name, string value)
        {
            _webResponse.SetCustomHeader(name, value);
            return this;
        }

        
        public HorseResponse Content(object content)
        {
            _content = content;
            return this;
        }

        public object Content() => _content;

       
        public HorseResponse ContentType(string contentType)
        {
            _webResponse.ContentType = contentType;
            return this;
        }

        
        public WebResponse RawWebResponse() => _webResponse;

        
        public HorseResponse Send(string content)
        {
            _webResponse.Content = content;
            return this;
        }

        
        public HorseResponse Send<T>(T content) where T : class
        {
            _content = content;
            return this;
        }

        
        public HorseResponse RedirectTo(string location)
        {
            _webResponse.SetCustomHeader("Location", location);
            return Status(THTTPStatus.SeeOther);
        }

        
        public HorseResponse RedirectTo(string location, THTTPStatus status)
        {
            _webResponse.SetCustomHeader("Location", location);
            return Status(status);
        }

        
        public HorseResponse RemoveHeader(string name)
        {
            if (_webResponse.CustomHeaders.ContainsKey(name))
                _webResponse.CustomHeaders.Remove(name);
            return this;
        }

        
        public HorseResponse Status(int status)
        {
            _webResponse.StatusCode = status;
            return this;
        }

        
        public HorseResponse Status(THTTPStatus status)
        {
            _webResponse.StatusCode = status.ToInteger();
            return this;
        }

        
        public HorseResponse SendFile(Stream fileStream, string fileName, string contentType)
        {
            fileStream.Position = 0;
            string lFileName = System.IO.Path.GetFileName(fileName);
            _webResponse.FreeContentStream = false;
            _webResponse.ContentLength = fileStream.Length;
            _webResponse.ContentStream = fileStream;
            _webResponse.SetCustomHeader("Content-Disposition", string.Format("inline; filename=\"{0}\"", lFileName));
            _webResponse.ContentType = contentType;
            if (string.IsNullOrEmpty(contentType))
                _webResponse.ContentType = Horse.Mime.HorseMimeTypes.GetFileType(lFileName);
            _webResponse.SendResponse();
            return this;
        }

        
        public HorseResponse SendFile(string fileName, string contentType)
        {
            using (var file = new HorseCoreFile(fileName))
            {
                file.FreeContentStream = true;
                string lContentType = contentType;
                if (string.IsNullOrEmpty(contentType))
                    lContentType = Horse.Mime.HorseMimeTypes.GetFileType(file.Name);
                return SendFile(file.ContentStream, file.Name, lContentType);
            }
        }

        
        public HorseResponse Download(Stream fileStream, string fileName, string contentType)
        {
            fileStream.Position = 0;
            string lFileName = System.IO.Path.GetFileName(fileName);
            _webResponse.FreeContentStream = false;
            _webResponse.ContentLength = fileStream.Length;
            _webResponse.ContentStream = fileStream;
            _webResponse.SetCustomHeader("Content-Disposition", string.Format("attachment; filename=\"{0}\"", lFileName));
            _webResponse.ContentType = contentType;
            if (string.IsNullOrEmpty(contentType))
                _webResponse.ContentType = Horse.Mime.HorseMimeTypes.GetFileType(lFileName);
            _webResponse.SendResponse();
            return this;
        }

        
        public HorseResponse Download(string fileName, string contentType)
        {
            using (var file = new HorseCoreFile(fileName))
            {
                file.FreeContentStream = true;
                string lContentType = contentType;
                if (string.IsNullOrEmpty(contentType))
                    lContentType = Horse.Mime.HorseMimeTypes.GetFileType(file.Name);
                return Download(file.ContentStream, file.Name, lContentType);
            }
        }

        
        public HorseResponse Render(Stream fileStream, string fileName)
        {
            return SendFile(fileStream, fileName, Horse.Commons.TMimeTypes.TextHTML.ToString());
        }

        
        public HorseResponse Render(string fileName)
        {
            return SendFile(fileName, Horse.Commons.TMimeTypes.TextHTML.ToString());
        }

        
        public int Status() => _webResponse.StatusCode;

        public void Dispose()
        {
            if (_content is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
