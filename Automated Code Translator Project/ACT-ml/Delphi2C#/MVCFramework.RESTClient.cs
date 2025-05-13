using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MVCFramework.RESTClient
{
    
    public interface IMVCRESTClient : IDisposable
    {
        string BaseURL { get; set; }
        string Resource { get; set; }
        IDictionary<string, string> Headers { get; }
        IMVCRESTClient AddHeader(string name, string value);
        IMVCRESTClient SetBasicAuthorization(string username, string password);
        IMVCRESTClient SetBearerAuthorization(string token);
        IMVCRESTClient ClearAuthorization();
        

        Task<IMVCRESTResponse> GetAsync(string resource = null);
        Task<IMVCRESTResponse> PostAsync(string resource, object body);
        Task<IMVCRESTResponse> PutAsync(string resource, object body);
        Task<IMVCRESTResponse> DeleteAsync(string resource);
    }

    
    public interface IMVCRESTResponse
    {
        bool Success { get; }
        int StatusCode { get; }
        string StatusText { get; }
        string Content { get; }
        T ToObject<T>();
        
    }


    public class MVCRESTClient : IMVCRESTClient
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        public string BaseURL { get; set; }
        public string Resource { get; set; }

        public IDictionary<string, string> Headers => _headers;

        public MVCRESTClient()
        {
            _httpClient = new HttpClient();
        }

        public IMVCRESTClient AddHeader(string name, string value)
        {
            if (_headers.ContainsKey(name))
                _headers[name] = value;
            else
                _headers.Add(name, value);
            return this;
        }

        public IMVCRESTClient SetBasicAuthorization(string username, string password)
        {
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            return AddHeader("Authorization", "Basic " + credentials);
        }

        public IMVCRESTClient SetBearerAuthorization(string token)
        {
            return AddHeader("Authorization", "Bearer " + token);
        }

        public IMVCRESTClient ClearAuthorization()
        {
            if (_headers.ContainsKey("Authorization"))
                _headers.Remove("Authorization");
            return this;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string resource)
        {
            var url = new Uri(new Uri(BaseURL), resource ?? Resource);
            var request = new HttpRequestMessage(method, url);
            foreach (var header in _headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return request;
        }

        public async Task<IMVCRESTResponse> GetAsync(string resource = null)
        {
            var request = CreateRequest(HttpMethod.Get, resource);
            var response = await _httpClient.SendAsync(request);
            return new MVCRESTResponse(response);
        }

        public async Task<IMVCRESTResponse> PostAsync(string resource, object body)
        {
            var request = CreateRequest(HttpMethod.Post, resource);
            string json = JsonConvert.SerializeObject(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            return new MVCRESTResponse(response);
        }

        public async Task<IMVCRESTResponse> PutAsync(string resource, object body)
        {
            var request = CreateRequest(HttpMethod.Put, resource);
            string json = JsonConvert.SerializeObject(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            return new MVCRESTResponse(response);
        }

        public async Task<IMVCRESTResponse> DeleteAsync(string resource)
        {
            var request = CreateRequest(HttpMethod.Delete, resource);
            var response = await _httpClient.SendAsync(request);
            return new MVCRESTResponse(response);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    
    public class MVCRESTResponse : IMVCRESTResponse
    {
        private readonly HttpResponseMessage _response;
        private string _content;

        public bool Success => _response.IsSuccessStatusCode;
        public int StatusCode => (int)_response.StatusCode;
        public string StatusText => _response.ReasonPhrase;

        public string Content
        {
            get
            {
                if (_content == null)
                {
                    _content = _response.Content.ReadAsStringAsync().Result;
                }
                return _content;
            }
        }

        public MVCRESTResponse(HttpResponseMessage response)
        {
            _response = response;
        }

        public T ToObject<T>()
        {
            return JsonConvert.DeserializeObject<T>(Content);
        }
    }
}
