
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MessagingExtensionsTests
{
    [TestFixture]
    public class MessagingExtensionsTestCase
    {
        private HttpClient _client;
        private const string BaseUrl = "http://localhost:8080";

        [SetUp]
        public void Setup()
        {
            
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromMinutes(30);
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }

        #region Helper Methods

        private async Task DoLoginAsync(string username)
        {
            
            var response = await _client.PostAsync($"{BaseUrl}/login/{username}", null);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Login failed");
        }

        private async Task DoLogoutAsync()
        {
            var response = await _client.PostAsync($"{BaseUrl}/logout", null);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Logout failed");
        }

        
        private async Task<HttpResponseMessage> PostAsync(string url)
        {
            return await _client.PostAsync(url, null);
        }

        #endregion

        #region Tests

        [Test]
        public async Task TestSubscribeOnATopic()
        {
            await DoLoginAsync("guest");

            HttpResponseMessage response = await PostAsync($"{BaseUrl}/messages/subscriptions/queue/test01");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Subscription failed");

            
            response = await _client.DeleteAsync($"{BaseUrl}/messages/subscriptions/queue/test01");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Unsubscription failed");

            await DoLogoutAsync();
        }

        [Test]
        public async Task TestMultipleSubscribeOnSameTopic()
        {
            
            await DoLoginAsync("guest");

           
            HttpResponseMessage response = await PostAsync($"{BaseUrl}/messages/subscriptions/queue/test01");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "First subscription failed");

            response = await PostAsync($"{BaseUrl}/messages/subscriptions/queue/test01");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Second subscription failed");

            

            await DoLogoutAsync();
        }

        [Test]
        public async Task TestMultipleSubscribeAndUnsubscribe()
        {
            await DoLoginAsync("guest");

            HttpResponseMessage response = await PostAsync($"{BaseUrl}/messages/subscriptions/queue/test01");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Subscription test01 failed");

            response = await PostAsync($"{BaseUrl}/messages/subscriptions/queue/test010");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Subscription test010 failed");

            response = await _client.GetAsync($"{BaseUrl}/messages/subscriptions");
            string subs = (await response.Content.ReadAsStringAsync()).Trim();
            Assert.AreEqual("/queue/test01;/queue/test010", subs, "Subscription list incorrect");


            response = await _client.DeleteAsync($"{BaseUrl}/messages/subscriptions/queue/test01");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Unsubscription test01 failed");

            response = await _client.GetAsync($"{BaseUrl}/messages/subscriptions");
            subs = (await response.Content.ReadAsStringAsync()).Trim();
            Assert.AreEqual("/queue/test010", subs, "Subscription list after unsubscription incorrect");

            await DoLogoutAsync();
        }

        

        #endregion
    }
}
