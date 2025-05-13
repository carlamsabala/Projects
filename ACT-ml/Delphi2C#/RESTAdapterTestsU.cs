using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using MVCFramework.RESTAdapter;
using MVCFramework.RESTClient.Intf;
using MVCFramework.Commons;
using JsonDataObjects;
using BusinessObjectsU;

namespace REST
{
    [Headers("User-Agent", "RESTAdapter-Test")]
    public interface ITestService : IInvokable
    {
        [RESTResource(HttpMethod.Get, "/people")]
        [MVCListOf(typeof(TPerson))]
        List<TPerson> GetPeople();
        [RESTResource(HttpMethod.Get, "/people")]
        [MVCListOf(typeof(TPerson))]
        [Mapping(typeof(TPeople))]
        void GetPeopleAsynch(IAsynchRequest asynchRequest);
        [RESTResource(HttpMethod.Get, "/people/1")]
        TPerson GetTonyStark();
        [RESTResource(HttpMethod.Get, "/people/1")]
        [Mapping(typeof(TPerson))]
        void GetTonyStarkAsynch(IAsynchRequest asynchRequest);
        [RESTResource(HttpMethod.Get, "/people/{personid}")]
        TPerson GetPersonByID([Param("personid")] int personId);
        [RESTResource(HttpMethod.Post, "/people")]
        TPerson SendPerson([Body] TPerson body);
        [RESTResource(HttpMethod.Get, "/people")]
        TJSONArray GetPersonInJSONArray();
        [Headers("Accept", "application/json")]
        [Headers("ContentType", "application/json")]
        [RESTResource(HttpMethod.Get, "/adapter/testconsumejson")]
        TJsonBaseObject HeadersApplicationJSON();
        [Headers("Accept", "text/plain")]
        [Headers("ContentType", "text/plain")]
        [RESTResource(HttpMethod.Get, "/testconsumes")]
        string HeadersTextPlain();
        [Headers("Accept", "text/plain")]
        [Headers("ContentType", "text/plain")]
        [RESTResource(HttpMethod.Get, "/adapter/testconsumejson")]
        IMVCRESTResponse ApplicationJSONWithTextPlainHeader();
    }

    [TestFixture]
    public class TestRESTAdapter
    {
        private TRESTAdapter<ITestService> restAdapter;
        private ITestService testService;

        [SetUp]
        public void SetUp()
        {
            restAdapter = new TRESTAdapter<ITestService>();
            testService = restAdapter.Build(TestConsts.TEST_SERVER_ADDRESS, 8888);
        }

        [Test]
        public void TestGetPeople()
        {
            List<TPerson> listPerson = testService.GetPeople();
            Assert.IsTrue(listPerson.Count > 0);
            Assert.AreEqual("Tony", listPerson[0].FirstName);
            Assert.AreEqual("Stark", listPerson[0].LastName);
        }

        [Test]
        public void TestGetPeopleAsynch()
        {
            var evt = new EventWaitHandle(false, EventResetMode.AutoReset);
            TPeople people = null;
            IAsynchRequest asynchRequest = new TAsynchRequest(value => { people = value.AsType<TPeople>(); evt.Set(); });
            testService.GetPeopleAsynch(asynchRequest);
            Assert.IsTrue(evt.WaitOne(5000));
            Assert.IsNotNull(people);
            people.OwnsObjects = true;
            Assert.IsTrue(people.Count > 0);
            Assert.AreEqual("Tony", people[0].FirstName);
            Assert.AreEqual("Stark", people[0].LastName);
        }

        [Test]
        public void TestGetTonyStark()
        {
            TPerson person = testService.GetTonyStark();
            Assert.AreEqual("Tony", person.FirstName);
            Assert.AreEqual("Stark", person.LastName);
            Assert.IsTrue(person.Married);
        }

        [Test]
        public void TestGetTonyStarkAsynch()
        {
            var evt = new EventWaitHandle(false, EventResetMode.AutoReset);
            TPerson person = null;
            IAsynchRequest asynchRequest = new TAsynchRequest(value => { person = value.AsType<TPerson>(); evt.Set(); });
            testService.GetTonyStarkAsynch(asynchRequest);
            Assert.IsTrue(evt.WaitOne(5000));
            Assert.IsNotNull(person);
            Assert.AreEqual("Tony", person.FirstName);
            Assert.AreEqual("Stark", person.LastName);
            Assert.IsTrue(person.Married);
        }

        [Test]
        public void TestPostPerson()
        {
            TPerson person = TPerson.GetNew("Peter", "Parker", 0, false);
            TPerson retPerson = testService.SendPerson(person);
            Assert.AreEqual("Peter", retPerson.FirstName);
            Assert.AreEqual("Parker", retPerson.LastName);
            Assert.IsFalse(retPerson.Married);
        }

        [Test]
        public void TestGetPersonByID()
        {
            TPerson person = testService.GetPersonByID(1);
            Assert.AreEqual("Tony", person.FirstName);
            Assert.AreEqual("Stark", person.LastName);
            Assert.IsTrue(person.Married);
        }

        [Test]
        public void TestHeadersApplicationJSON()
        {
            TJSONObject res = testService.HeadersApplicationJSON() as TJSONObject;
            Assert.AreEqual("Hello World", res.S["key"]);
        }

        [Test]
        public void TestHeadersTextPlain()
        {
            string res = testService.HeadersTextPlain();
            Assert.AreEqual("Hello World", res);
        }

        [Test]
        public void TestApplicationJSONWithHeaderTextPlain()
        {
            IMVCRESTResponse resp = testService.ApplicationJSONWithTextPlainHeader();
            Assert.AreEqual(404, resp.StatusCode);
        }

        [Test]
        public void TestGetPersonInJSONArray()
        {
            TJSONArray jsonArray = testService.GetPersonInJSONArray();
            string jsonStr = jsonArray.ToString();
            Assert.IsTrue(jsonStr.Contains("Tony"));
            Assert.IsTrue(jsonStr.Contains("Stark"));
            Assert.IsTrue(jsonStr.Contains("Bruce"));
            Assert.IsTrue(jsonStr.Contains("Banner"));
        }
    }
}
