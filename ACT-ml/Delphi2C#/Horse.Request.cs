using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Horse.Request
{
    #region Stub Types and Helpers

    
    public class WebRequest
    {
        public string Content { get; set; } = "";
        public byte[] RawContent { get; set; }
        public string Host { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string PathInfo { get; set; } = "";
        
        public List<WebFile> Files { get; set; } = new List<WebFile>();
        public List<KeyValuePair<string, string>> ContentFields { get; set; } = new List<KeyValuePair<string, string>>();
        public List<string> CookieFields { get; set; } = new List<string>();
        public List<string> QueryFields { get; set; } = new List<string>();
#if !FPC
        public TWebRequestMethod MethodType { get; set; } = TWebRequestMethod.GET;
#endif
        public string Method { get; set; } = "GET";
    }

    
    public class WebFile
    {
        public string FieldName { get; set; }
        public Stream Stream { get; set; }
    }

    
    public enum TWebRequestMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        HEAD,
        PATCH,
        OPTIONS
    }

    
    public class HorseCoreParam
    {
        public Dictionary<string, string> Dictionary { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public HorseCoreParam(List<string> list)
        {
            // In a real implementation, 'list' may be used.
        }

        
        public HorseCoreParam Required(bool req)
        {
            // You can store the required flag if needed.
            return this;
        }

        
        public void AddStream(string fieldName, Stream stream)
        {
            // Implementation depends on your requirements.
           
        }
    }

    
    public static class HorseCoreParamHeader
    {
        public static HorseCoreParam GetHeaders(WebRequest request)
        {
            
            return new HorseCoreParam(new List<string>());
        }
    }

    
    public class HorseSessions
    {
        // Implement session handling as needed.
    }

    #endregion

    public class HorseRequest : IDisposable
    {
        private WebRequest _webRequest;
        private HorseCoreParam _headers;
        private HorseCoreParam _query;
        private HorseCoreParam _params;
        private HorseCoreParam _contentFields;
        private HorseCoreParam _cookie;
        private object _body;
        private object _session;
        private HorseSessions _sessions;

        
        public HorseRequest(WebRequest webRequest)
        {
            _webRequest = webRequest;
            _sessions = new HorseSessions();
        }

        
        public void Dispose()
        {
            
            _headers = null;
            _query = null;
            _params = null;
            _contentFields = null;
            _cookie = null;
            if (_body is IDisposable disposableBody)
                disposableBody.Dispose();
            _sessions = null;
        }

        #region Body Methods

        
        public virtual string Body()
        {
            return _webRequest.Content;
        }

        
        public T Body<T>() where T : class
        {
            return _body as T;
        }

        
        public virtual HorseRequest Body(object body)
        {
            if (_body is IDisposable disposableBody)
                disposableBody.Dispose();
            _body = body;
            return this;
        }

        
        public virtual string Body(Encoding encoding)
        {
            if (_webRequest.RawContent != null && _webRequest.RawContent.Length > 0)
            {
                return encoding.GetString(_webRequest.RawContent);
            }
            else
            {
                return _webRequest.Content;
            }
        }

        #endregion

        #region Session Methods

        
        public virtual HorseRequest Session(object session)
        {
            _session = session;
            return this;
        }

       
        public T Session<T>() where T : class
        {
            return _session as T;
        }

        #endregion

        #region Parameter Accessors

        
        public virtual HorseCoreParam Headers()
        {
            if (_headers == null)
            {
                _headers = new HorseCoreParam(HorseCoreParamHeader.GetHeaders(_webRequest).Dictionary).Required(false);
            }
            return _headers;
        }

                public virtual HorseCoreParam Query()
        {
            if (_query == null)
                InitializeQuery();
            return _query;
        }

        
        public virtual HorseCoreParam Params()
        {
            if (_params == null)
                InitializeParams();
            return _params;
        }

        
        public virtual HorseCoreParam Cookie()
        {
            if (_cookie == null)
                InitializeCookie();
            return _cookie;
        }

        
        public virtual HorseCoreParam ContentFields()
        {
            if (_contentFields == null)
                InitializeContentFields();
            return _contentFields;
        }

        
        public virtual HorseSessions Sessions()
        {
            return _sessions;
        }

        #endregion

        #region Request Information

        
        public virtual TWebRequestMethod MethodType()
        {
#if FPC
            return StringCommandToMethodType(_webRequest.Method);
#else
            return _webRequest.MethodType;
#endif
        }

        
        public virtual string ContentType()
        {
            return _webRequest.ContentType;
        }

        
        public virtual string Host()
        {
            return _webRequest.Host;
        }

        
        public virtual string PathInfo()
        {
            string prefix = string.IsNullOrEmpty(_webRequest.PathInfo) ? "/" : "/";
            return prefix + _webRequest.PathInfo;
        }

        
        public virtual WebRequest RawWebRequest()
        {
            return _webRequest;
        }

        #endregion

        #region Initialization Methods

        
        private void InitializeQuery()
        {
            _query = new HorseCoreParam(new List<string>()).Required(false);
            foreach (var item in _webRequest.QueryFields)
            {
                int equalPos = item.IndexOf('=');
                if (equalPos > 0)
                {
                    string key = item.Substring(0, equalPos);
                    string value = item.Substring(equalPos + 1);
                    if (!_query.Dictionary.ContainsKey(key))
                        _query.Dictionary[key] = value;
                    else
                        _query.Dictionary[key] = _query.Dictionary[key] + "," + value;
                }
            }
        }

        
        private void InitializeParams()
        {
            _params = new HorseCoreParam(new List<string>()).Required(true);
        }

        
        private void InitializeContentFields()
        {
            _contentFields = new HorseCoreParam(new List<string>()).Required(false);
            if (!CanLoadContentFields())
                return;

            
            foreach (var file in _webRequest.Files)
            {
                _contentFields.AddStream(file.FieldName, file.Stream);
            }

            
            foreach (var kvp in _webRequest.ContentFields)
            {
                string lName = kvp.Key;
                string lValue = kvp.Value;
                if (!string.IsNullOrEmpty(lName))
                    _contentFields.Dictionary[lName] = lValue;
            }
        }

        
        private void InitializeCookie()
        {
            _cookie = new HorseCoreParam(new List<string>()).Required(false);
            foreach (var cookieField in _webRequest.CookieFields)
            {
                var parts = cookieField.Split('=');
                if (parts.Length >= 2)
                    _cookie.Dictionary[parts[0]] = parts[1];
            }
        }

        
        private bool CanLoadContentFields()
        {
            return IsMultipartForm() || IsFormURLEncoded();
        }

        
        private bool IsFormURLEncoded()
        {
            string contentType = _webRequest.ContentType;
            string formUrlEncoded = "application/x-www-form-urlencoded"; 
            return !string.IsNullOrEmpty(contentType) &&
                   contentType.StartsWith(formUrlEncoded, StringComparison.OrdinalIgnoreCase);
        }

        
        private bool IsMultipartForm()
        {
            string contentType = _webRequest.ContentType;
            string multipart = "multipart/form-data"; 
            return !string.IsNullOrEmpty(contentType) &&
                   contentType.StartsWith(multipart, StringComparison.OrdinalIgnoreCase);
        }

        
        private TWebRequestMethod StringCommandToMethodType(string method)
        {
            if (Enum.TryParse<TWebRequestMethod>(method, true, out var result))
                return result;
            return TWebRequestMethod.GET;
        }

        #endregion

        #region Overloads for Session and Body

        public virtual HorseRequest Session<T>() where T : class
        {
            return Session(_session);
        }

        public virtual HorseRequest Session(object session)
        {
            _session = session;
            return this;
        }

        public virtual HorseRequest Body(string body)
        {
            _body = body;
            return this;
        }

        #endregion

        
        public override string ToString()
        {
            return Body();
        }

        #region Additional Overloads


        #endregion
    }
}
