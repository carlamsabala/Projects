using System;
using System.Collections.Generic;
using System.Threading;
using MVCFramework;
using MVCFramework.Commons;
using MVCFramework.Server;
using MVCFramework.Server.Impl;
using MVCFramework.RESTClient;
using MVCFramework.RESTClient.Intf;
using NUnit.Framework;
using StandAloneServerWebModuleTest;

namespace StandaloneServerTest
{
    [Route("/")]
    public class TestController : MVCController
    {
        [Route("hello")]
        [HttpGet]
        public void HelloWorld(WebContext ctx)
        {
            Render("Hello World called with GET");
        }
    }

    [TestFixture]
    public class TestServerContainer
    {
        [SetUp]
        public void SetUp() { }
        [TearDown]
        public void TearDown() { }

        [Test]
        public void TestListener()
        {
            IMVCListener listener = MVCListener.Create(
                MVCListenerProperties.New()
                .SetName("Listener1")
                .SetPort(5000)
                .SetMaxConnections(512)
                .SetWebModuleClass(TestWebModuleClass));
            Assert.IsNotNull(listener);
            listener.Start();
            Assert.IsTrue(listener.Active);
            listener.Stop();
            Assert.IsFalse(listener.Active);
        }

        [Test]
        public void TestServerListenerAndClient()
        {
            IMVCListener listener = MVCListener.Create(
                MVCListenerProperties.New()
                .SetName("Listener1")
                .SetPort(6000)
                .SetMaxConnections(1024)
                .SetWebModuleClass(TestWebModuleClass));
            Assert.IsNotNull(listener);
            listener.Start();
            Assert.IsTrue(listener.Active);
            IMVCRESTClient client = RESTClient.New().BaseURL("localhost", 6000);
            client.SetBasicAuthorization("dmvc", "123");
            Assert.AreEqual("Hello World called with GET", client.Get("/hello").Content);
            listener.Stop();
            Assert.IsFalse(listener.Active);
        }

        [Test]
        public void TestListenerContext()
        {
            IMVCListenersContext listenerContext = MVCListenersContext.Create();
            listenerContext.Add(
                MVCListenerProperties.New()
                .SetName("Listener2")
                .SetPort(6000)
                .SetMaxConnections(1024)
                .SetWebModuleClass(TestWebModuleClass));
            listenerContext.Add(
                MVCListenerProperties.New()
                .SetName("Listener3")
                .SetPort(7000)
                .SetMaxConnections(1024)
                .SetWebModuleClass(TestWebModuleClass2));
            Assert.IsNotNull(listenerContext.FindByName("Listener2"));
            Assert.IsNotNull(listenerContext.FindByName("Listener3"));
            listenerContext.StartAll();
            Assert.AreEqual(2, listenerContext.Count);
            Assert.IsTrue(listenerContext.FindByName("Listener2").Active);
            Assert.IsTrue(listenerContext.FindByName("Listener3").Active);
            listenerContext.StopAll();
            Assert.IsFalse(listenerContext.FindByName("Listener2").Active);
            Assert.IsFalse(listenerContext.FindByName("Listener3").Active);
            listenerContext.Remove("Listener2").Remove("Listener3");
            Assert.AreEqual(0, listenerContext.Count);
        }
    }
}
