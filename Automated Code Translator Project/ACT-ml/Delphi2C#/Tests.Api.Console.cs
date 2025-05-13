using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RESTRequest4D.Request;
using Controllers.Api;
using Horse;
using Horse.Jhonson;

namespace Tests.Api.Console
{
    [TestFixture]
    public class TApiTest
    {
        private JObject FJSONObject;
        private JArray FJSONArray;

        private void CreateApi()
        {
            Horse.Horse.Create();
        }

        private void StartApiListen()
        {
            if (!Horse.Horse.IsRunning)
            {
                Task.Run(() =>
                {
                    Horse.Horse.Use(Jhonson.Jhonson);
                    Controllers.Api.Registry();
                    Horse.Horse.MaxConnections = 10;
                    Horse.Horse.Listen();
                });
            }
        }

        private void StartApiListenPort()
        {
            if (!Horse.Horse.IsRunning)
            {
                Task.Run(() =>
                {
                    Controllers.Api.Registry();
                    Horse.Horse.Listen(9000);
                });
            }
        }

        private void StartApiListenHost()
        {
            if (!Horse.Horse.IsRunning)
            {
                Task.Run(() =>
                {
                    Controllers.Api.Registry();
                    Horse.Horse.Listen("0.0.0.0");
                });
            }
        }

        private void StartApiListens()
        {
            if (!Horse.Horse.IsRunning)
            {
                Task.Run(() =>
                {
                    Controllers.Api.Registry();
                    Horse.Horse.Listen(
                        (horse) => { },
                        (horse) => { }
                    );
                });
            }
        }

        private void StartApiPortListens()
        {
            if (!Horse.Horse.IsRunning)
            {
                Task.Run(() =>
                {
                    Controllers.Api.Registry();
                    Horse.Horse.Listen(9000,
                        (horse) => { },
                        (horse) => { }
                    );
                });
            }
        }

        private void StopApiListen()
        {
            Horse.Horse.StopListen();
        }

        private void StopApi()
        {
            Horse.Horse.StopListen();
        }

        [TearDown]
        public void TearDown()
        {
            FJSONObject?.Dispose();
            FJSONArray?.Dispose();
        }

        [Test]
        public void TestGet()
        {
            StartApiListen();
            var response = RestRequest.New().BaseURL("http://localhost:9000/Api/Test").Accept("application/json").Get();
            FJSONArray = JArray.Parse(response.Content);
            Assert.AreEqual(9000, Horse.Horse.Port);
            Assert.AreEqual("0.0.0.0", Horse.Horse.Host);
            Assert.AreEqual(10, Horse.Horse.MaxConnections);
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual(3, FJSONArray.Count);
            StopApiListen();
        }

        [Test]
        [TestCase("POST request test")]
        public void TestPost(string AValue)
        {
            StartApiListenPort();
            var response = RestRequest.New().BaseURL("http://localhost:9000/Api/Test").Accept("application/json")
                .AddBody("{\"value\": \"" + AValue + "\"}").Post();
            FJSONObject = JObject.Parse(response.Content);
            Assert.AreEqual(201, response.StatusCode);
            if (FJSONObject["value"] != null && FJSONObject["value"].Type != JTokenType.Null)
                Assert.AreEqual(AValue, FJSONObject["value"].ToString());
            else
                Assert.Fail("The return is not without correct format.");
            StopApiListen();
        }

        [Test]
        [TestCase("PUT request test")]
        public void TestPut(string AValue)
        {
            StartApiListenHost();
            var response = RestRequest.New().BaseURL("http://localhost:9000/Api/Test").Accept("application/json")
                .AddBody("{\"value\": \"" + AValue + "\"}").Put();
            FJSONObject = JObject.Parse(response.Content);
            Assert.AreEqual(200, response.StatusCode);
            if (FJSONObject["value"] != null && FJSONObject["value"].Type != JTokenType.Null)
                Assert.AreEqual(AValue, FJSONObject["value"].ToString());
            else
                Assert.Fail("The return is not in the correct format.");
            StopApiListen();
        }

        [Test]
        [TestCase("1")]
        public void TestDelete(string AValue)
        {
            StartApiListens();
            var response = RestRequest.New().BaseURL("http://localhost:9000/Api/Test/" + AValue).Accept("application/json").Delete();
            FJSONObject = JObject.Parse(response.Content);
            Assert.AreEqual(200, response.StatusCode);
            if (FJSONObject["value"] != null && FJSONObject["value"].Type != JTokenType.Null)
                Assert.AreEqual(AValue, FJSONObject["value"].ToString());
            else
                Assert.Fail("The return is not in the correct format.");
            StopApiListen();
        }

        [Test]
        public void TestGStartApiPortListens()
        {
            StartApiPortListens();
            StopApi();
        }

        [Test]
        public void TestCreateApi()
        {
            Assert.Throws<Exception>(() => CreateApi(), "The Horse instance has already been created");
        }

        [Test]
        public void TestToHorse()
        {
            Assert.IsNotNull(Horse.Horse.ToModule.ToHorse(), "Module instance must not be null");
        }
    }
}
