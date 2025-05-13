
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json.Linq; 

namespace LiveServerTests
{
    public interface IRestClient
    {
        
        IRestClient BaseUrl(string host, int port);
        IRestClient Accept(string mediaType);
        IRestClient AddHeader(string name, string value);
        IRestClient AddPathParam(string name, object value);
        IRestClient AddQueryStringParam(string name, object value);
        IRestClient AddCookie(string name, string value);
        Task<IRestResponse> GetAsync(string url);
        Task<IRestResponse> PostAsync(string url, string body, string contentType = null);
        Task<IRestResponse> PutAsync(string url, string body, string contentType = null);
        Task<IRestResponse> DeleteAsync(string url);
        IRestClient ClearHeaders();
        
    }

    public interface IRestResponse
    {
        int StatusCode { get; }
        string StatusText { get; }
        string Content { get; }
        string ContentType { get; }
        long ContentLength { get; }
        string GetHeaderValue(string name);
        
        JObject ToJObject();
        JArray ToJArray();
    }

    public abstract class BaseServerTest
    {
        protected IRestClient RestClient;
        protected const string TEST_SERVER_ADDRESS = "127.0.0.1";

        [SetUp]
        public virtual void Setup()
        {
            
            RestClient = RestClientFactory.Create()
                          .BaseUrl(TEST_SERVER_ADDRESS, 8888);
            
        }

        [TearDown]
        public virtual void TearDown()
        {
            RestClient = null;
        }

        protected async Task DoLoginWithAsync(string userName)
        {
            
            var response = await RestClient.AddPathParam("username", userName)
                                           .GetAsync("/login/{username}");
            Assert.That(response.StatusCode, Is.EqualTo(200), "Login Failed: " + response.Content);
        }

        protected async Task DoLogoutAsync()
        {
            var response = await RestClient.GetAsync("/logout");
            Assert.That(response.StatusCode, Is.EqualTo(200), "Logout Failed: " + response.Content);
        }
    }

    [TestFixture]
    public class ServerTest : BaseServerTest
    {
        
        [Test]
        [TestCase("/exception/fault")]
        [TestCase("/exception/fault2")]
        public async Task TestControllerWithExceptionInCreate(string urlSegment)
        {
            
            var response = await RestClient.Accept("application/json")
                                           .GetAsync(urlSegment);
            Assert.That(response.StatusCode, Is.EqualTo(500));
            Assert.That(response.ContentType, Does.Contain("application/json"));
            Assert.That(response.Content, Does.Contain("Cannot create controller"));
        }

        [Test]
        [TestCase("/")]
        [TestCase("/action1")]
        [TestCase("/action2")]
        [TestCase("/api/v1")]
        [TestCase("/api/v1/action1")]
        [TestCase("/api/v1/action2")]
        [TestCase("/api/v2")]
        [TestCase("/api/v2/action1")]
        [TestCase("/api/v2/action2")]
        public async Task TestMultiMVCPathOnControllerAndAction(string urlSegment)
        {
            var response = await RestClient.GetAsync(urlSegment);
            Assert.That(response.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task TestReqWithParams()
        {
            
            var response = await RestClient.GetAsync("/unknownurl/bla/bla");
            Assert.That(response.StatusCode, Is.EqualTo(404));

            response = await RestClient.GetAsync("/req/with/params/");
            Assert.That(response.StatusCode, Is.EqualTo(404));

            response = await RestClient.GetAsync("/req/with/params");
            Assert.That(response.StatusCode, Is.EqualTo(404));

            response = await RestClient.AddPathParam("par1", "1")
                                       .AddPathParam("par2", "2")
                                       .AddPathParam("par3", "3")
                                       .GetAsync("/req/with/params/($par1)/($par2)/($par3)");
            Assert.That(response.StatusCode, Is.EqualTo(200));

            
            JObject json = response.ToJObject();
            Assert.That(json.Value<string>("par1"), Is.EqualTo("1"));
            Assert.That(json.Value<string>("par2"), Is.EqualTo("2"));
            Assert.That(json.Value<string>("par3"), Is.EqualTo("3"));
            Assert.That(json.Value<string>("method"), Is.EqualTo("GET"));

            
            response = await RestClient.AddPathParam("par1", "1")
                                       .AddPathParam("par2", "2")
                                       .AddPathParam("par3", "3")
                                       .DeleteAsync("/req/with/params/($par1)/($par2)/($par3)");
            Assert.That(response.StatusCode, Is.EqualTo(200));
            json = response.ToJObject();
            Assert.That(json.Value<string>("par1"), Is.EqualTo("1"));
            Assert.That(json.Value<string>("par2"), Is.EqualTo("2"));
            Assert.That(json.Value<string>("par3"), Is.EqualTo("3"));
            Assert.That(json.Value<string>("method"), Is.EqualTo("DELETE"));
        }

        
        [Test]
        public async Task TestPOSTWithParamsAndJSONBody()
        {
            JObject jsonBody = new JObject
            {
                ["client"] = "clientdata"
            };
            var response = await RestClient.AddPathParam("par1", 1)
                                           .AddPathParam("par2", 2)
                                           .AddPathParam("par3", 3)
                                           .PostAsync("/echo/($par1)/($par2)/($par3)",
                                                      jsonBody.ToString());
            JObject jsonResponse = JObject.Parse(response.Content);
            Assert.That(jsonResponse.Value<string>("client"), Is.EqualTo("clientdata"));
            Assert.That(jsonResponse.Value<string>("echo"), Is.EqualTo("from server"));
        }

        
    }

    [TestFixture]
    [Category("jsonrpc")]
    public class JSONRPCServerTest
    {
        
        private IJSONRPCExecutor _executor;
        private IJSONRPCExecutor _executor2;
        private IJSONRPCExecutor _executor3;

        [SetUp]
        public void Setup()
        {
            
            _executor = JSONRPCExecutorFactory.Create("http://" + BaseServerTest.TEST_SERVER_ADDRESS + ":8888/jsonrpc", false);
            _executor2 = JSONRPCExecutorFactory.Create("http://" + BaseServerTest.TEST_SERVER_ADDRESS + ":8888/jsonrpcclass", false);
            _executor3 = JSONRPCExecutorFactory.Create("http://" + BaseServerTest.TEST_SERVER_ADDRESS + ":8888/jsonrpcclass1", false);

            
            _executor.SetOnSendCommand((jsonRequest) => Console.WriteLine("[JSONRPC REQUEST] : " + jsonRequest));
            _executor.SetOnReceiveHTTPResponse((httpResponse) => Console.WriteLine("[JSONRPC RESPONSE]: " + httpResponse.Content));
        }

        [Test]
        public void TestRequestWithoutParams()
        {
            var request = new JSONRPCRequest(1234, "MyRequest");
            var response = _executor.ExecuteRequest(request);
            Assert.IsFalse(response.IsError);
            Assert.IsTrue(response.Result.AsBoolean);
        }

        
    }

    [TestFixture]
    [Category("jsonrpc")]
    public class JSONRPCServerWithGETTest : JSONRPCServerTest
    {
        [SetUp]
        public new void Setup()
        {
            
            _executor = JSONRPCExecutorFactory.Create("http://" + BaseServerTest.TEST_SERVER_ADDRESS + ":8888/jsonrpcwithget", false, JSONRPCMethodType.Get);
            _executor2 = JSONRPCExecutorFactory.Create("http://" + BaseServerTest.TEST_SERVER_ADDRESS + ":8888/jsonrpcclasswithget", false, JSONRPCMethodType.Get);
            _executor3 = JSONRPCExecutorFactory.Create("http://" + BaseServerTest.TEST_SERVER_ADDRESS + ":8888/jsonrpcclass1withget", false, JSONRPCMethodType.Get);
        }
    }
}
