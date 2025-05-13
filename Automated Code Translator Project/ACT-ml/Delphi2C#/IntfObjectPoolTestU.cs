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
        public byte[] RawContent { get; set; } = new byte[0];
        public string Host { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string PathInfo { get; set; } = "";
        public List<WebFile> Files { get; set; } = new List<WebFile>();
        public Dictionary<string, string> ContentFields { get; set; } = new Dictionary<string, string>();
        public List<string> CookieFields { get; set; } = new List<string>();
        public List<string> QueryFields { get; set; } = new List<string>();

        public TWebRequestMethod MethodType { get; set; } = TWebRequestMethod.GET;
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
            // Optionally use 'list' if needed.
        }

        public HorseCoreParam Required(bool req)
        {
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

        public virtual HorseRequest Session(object session)
        {
            _session = session;
            return this;
        }

        public T Session<T>() where T : class
        {
            return _session as T;
        }

        public virtual HorseCoreParam Headers()
        {
            if (_headers == null)
            {
                _headers = new HorseCoreParam(HorseCoreParamHeader.GetHeaders(_webRequest).Dictionary.ToList()).Required(false);
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

        public virtual TWebRequestMethod MethodType()
        {
            return _webRequest.MethodType;
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
                string name = kvp.Key;
                string value = kvp.Value;
                if (!string.IsNullOrEmpty(name))
                    _contentFields.Dictionary[name] = value;
            }
        }

        private void InitializeCookie()
        {
            _cookie = new HorseCoreParam(new List<string>()).Required(false);
            foreach (var cookieField in _webRequest.CookieFields)
            {
                var parts = cookieField.Split(new[] { '=' }, 2);
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

        #endregion

        public virtual HorseRequest Body(string body)
        {
            _body = body;
            return this;
        }

        public virtual HorseRequest Session(object session)
        {
            _session = session;
            return this;
        }

        public virtual T Session<T>() where T : class
        {
            return _session as T;
        }

        public override string ToString()
        {
            return Body();
        }

        public void Dispose()
        {
            if (_body is IDisposable disposableBody)
                disposableBody.Dispose();
        }
    }
}
