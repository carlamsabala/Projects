using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace InjectorTest
{
    #region Service Interfaces and Implementations

    public interface IServiceA { }
    public interface IServiceB { }
    public interface IServiceC
    {
        IServiceA GetServiceA();
        IServiceB GetServiceB();
    }

    public class ServiceA : IServiceA
    {
        
    }

    public class ServiceB : IServiceB
    {
        

    
    public class ServiceAB : IServiceA, IServiceB
    {
        // Implementation details...
    }

    public class ServiceC : IServiceC
    {
        private readonly IServiceA _serviceA;
        private readonly IServiceB _serviceB;

        public ServiceC(IServiceA serviceA, IServiceB serviceB)
        {
            _serviceA = serviceA;
            _serviceB = serviceB;
        }

        public IServiceA GetServiceA() => _serviceA;
        public IServiceB GetServiceB() => _serviceB;
    }

    #endregion

    #region Container and Resolver Stubs

    
    public enum RegistrationType
    {
        Transient,
        Singleton,
        SingletonPerRequest
    }

    
    public class MVCContainerErrorUnknownServiceException : Exception
    {
        public MVCContainerErrorUnknownServiceException(string message) : base(message) { }
    }

        public class MVCServiceContainer
    {
        private readonly Dictionary<(Type serviceType, string key), Func<object>> _registrations = new Dictionary<(Type, string), Func<object>>();
        private bool _built = false;

        
        public MVCServiceContainer RegisterType<TImplementation, TService>(RegistrationType regType = RegistrationType.Transient, string key = null)
            where TImplementation : TService, new()
        {
            
            _registrations[(typeof(TService), key ?? string.Empty)] = () => new TImplementation();
            return this;
        }

                public MVCServiceContainer RegisterType<TService>(Func<object> factory, RegistrationType regType = RegistrationType.Transient, string key = null)
        {
            _registrations[(typeof(TService), key ?? string.Empty)] = factory;
            return this;
        }

        
        public MVCServiceContainer Build()
        {
            _built = true;
            return this;
        }

        public bool IsBuilt => _built;
    }

    
    public class ServiceContainerResolver
    {
        private readonly MVCServiceContainer _container;
        private readonly Dictionary<(Type serviceType, string key), object> _perRequestInstances = new Dictionary<(Type, string), object>();

        public ServiceContainerResolver(MVCServiceContainer container)
        {
            _container = container;
        }

        
        public object Resolve(Type serviceType, string key = "")
        {
            
            if (_container == null)
                throw new Exception("Container not built");

            if (!_container.IsBuilt)
                throw new Exception("Container not built");

            var regKey = (serviceType, key ?? string.Empty);
            if (_container._registrations.TryGetValue(regKey, out Func<object> factory))
            {
                
                if (key == string.Empty) 
                {
                    
                    if (!_perRequestInstances.ContainsKey(regKey))
                        _perRequestInstances[regKey] = factory();
                    return _perRequestInstances[regKey];
                }
                else
                {
                    
                    if (!_perRequestInstances.ContainsKey(regKey))
                        _perRequestInstances[regKey] = factory();
                    return _perRequestInstances[regKey];
                }
            }
            else
            {
                throw new MVCContainerErrorUnknownServiceException("Unknown service");
            }
        }

        
        public T Resolve<T>(string key = null) where T : class
        {
            return Resolve(typeof(T), key) as T;
        }
    }

    #endregion

    [TestFixture]
    public class TestContainer
    {
        
        [Test]
        public void TestNotBuiltContainer()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>();
            var resolver = new ServiceContainerResolver(container);
            Assert.Throws<Exception>(() => resolver.Resolve(typeof(IServiceA)));
        }

        
        [Test]
        public void TestUnknownService()
        {
            var container = new MVCServiceContainer();
            Assert.Throws<MVCContainerErrorUnknownServiceException>(() =>
            {
                container.RegisterType<ServiceA, IServiceB>(); 
            });
            Assert.Throws<MVCContainerErrorUnknownServiceException>(() =>
            {
                container.RegisterType(() => new MVCJsonDataObjectsSerializer(), typeof(IServiceB));
            });
            Assert.Throws<MVCContainerErrorUnknownServiceException>(() =>
            {
                container.RegisterType<ServiceA, IMVCSerializer>(); 
            });
        }

        
        [Test]
        public void TestTransient()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>(RegistrationType.Transient);
            container.RegisterType<ServiceA, IServiceA>(RegistrationType.Transient, "Svc1");
            container.Build();

            var resolver = new ServiceContainerResolver(container);
            var s1 = resolver.Resolve<IServiceA>();
            var s2 = resolver.Resolve<IServiceA>();
            Assert.AreNotEqual(s1, s2);

            var s1a = resolver.Resolve<IServiceA>("Svc1");
            var s2a = resolver.Resolve<IServiceA>("Svc1");
            Assert.AreNotEqual(s1a, s2a);
        }

        
        [Test]
        public void TestTransientWithDelegate()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<IServiceA>(() => new ServiceA(), RegistrationType.Transient);
            container.RegisterType<IServiceA>(() => new ServiceA(), RegistrationType.Transient, "Svc1");
            container.Build();

            var resolver = new ServiceContainerResolver(container);
            var s1 = resolver.Resolve<IServiceA>();
            var s2 = resolver.Resolve<IServiceA>();
            Assert.AreNotEqual(s1, s2);

            var s1a = resolver.Resolve<IServiceA>("Svc1");
            var s2a = resolver.Resolve<IServiceA>("Svc1");
            Assert.AreNotEqual(s1a, s2a);
        }

        
        [Test]
        public void TestSingleton()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>(RegistrationType.Singleton);
            container.RegisterType<ServiceA, IServiceA>(RegistrationType.Singleton, "Svc1");
            container.Build();

            
            var resolver1 = new ServiceContainerResolver(container);
            var a1 = resolver1.Resolve<IServiceA>();
            var a2 = resolver1.Resolve<IServiceA>();
            Assert.AreEqual(a1, a2);

            var a1a = resolver1.Resolve<IServiceA>("Svc1");
            var a2a = resolver1.Resolve<IServiceA>("Svc1");
            Assert.AreEqual(a1a, a2a);

            
            var resolver2 = new ServiceContainerResolver(container);
            var b1 = resolver2.Resolve<IServiceA>();
            var b2 = resolver2.Resolve<IServiceA>();
            Assert.AreEqual(b1, b2);
            Assert.AreEqual(a1, b1);
            Assert.AreEqual(a2, b2);
        }

        
        [Test]
        public void TestSingletonPerRequest()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>(RegistrationType.SingletonPerRequest);
            container.RegisterType<ServiceA, IServiceA>(RegistrationType.SingletonPerRequest, "Svc1");
            container.Build();

            
            var resolver1 = new ServiceContainerResolver(container);
            var a1 = resolver1.Resolve<IServiceA>();
            var a2 = resolver1.Resolve<IServiceA>();
            Assert.AreEqual(a1, a2);

            var a1a = resolver1.Resolve<IServiceA>("Svc1");
            var a2a = resolver1.Resolve<IServiceA>("Svc1");
            Assert.AreEqual(a1a, a2a);

            
            var resolver2 = new ServiceContainerResolver(container);
            var b1 = resolver2.Resolve<IServiceA>();
            var b2 = resolver2.Resolve<IServiceA>();
            Assert.AreEqual(b1, b2);
            Assert.AreNotEqual(a1, b1);
            Assert.AreNotEqual(a2, b2);
        }

        
        [Test]
        public void TestSingletonPerRequestWithDelegate()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<IServiceA>(() => new ServiceA(), RegistrationType.SingletonPerRequest);
            container.RegisterType<IServiceA>(() => new ServiceA(), RegistrationType.SingletonPerRequest, "Svc1");
            container.Build();

            
            var resolver1 = new ServiceContainerResolver(container);
            var a1 = resolver1.Resolve<IServiceA>();
            var a2 = resolver1.Resolve<IServiceA>();
            Assert.AreEqual(a1, a2);

            var a1a = resolver1.Resolve<IServiceA>("Svc1");
            var a2a = resolver1.Resolve<IServiceA>("Svc1");
            Assert.AreEqual(a1a, a2a);

            
            var resolver2 = new ServiceContainerResolver(container);
            var b1 = resolver2.Resolve<IServiceA>();
            var b2 = resolver2.Resolve<IServiceA>();
            Assert.AreEqual(b1, b2);
            Assert.AreNotEqual(a1, b1);
            Assert.AreNotEqual(a2, b2);
        }

        
        [Test]
        public void TestSingletonWithDelegate()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<IServiceA>(() => new ServiceA(), RegistrationType.Singleton);
            container.RegisterType<IServiceA>(() => new ServiceA(), RegistrationType.Singleton, "Svc1");
            container.Build();

            
            var resolver1 = new ServiceContainerResolver(container);
            var a1 = resolver1.Resolve<IServiceA>();
            var a2 = resolver1.Resolve<IServiceA>();
            Assert.AreEqual(a1, a2);

            var a1a = resolver1.Resolve<IServiceA>("Svc1");
            var a2a = resolver1.Resolve<IServiceA>("Svc1");
            Assert.AreEqual(a1a, a2a);

            
            var resolver2 = new ServiceContainerResolver(container);
            var b1 = resolver2.Resolve<IServiceA>();
            var b2 = resolver2.Resolve<IServiceA>();
            Assert.AreEqual(b1, b2);
            Assert.AreEqual(a1, b1);
            Assert.AreEqual(a2, b2);
        }

        
        [Test]
        public void TestTransient()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>(RegistrationType.Transient);
            container.RegisterType<ServiceA, IServiceA>(RegistrationType.Transient, "Svc1");
            container.Build();

            var resolver = new ServiceContainerResolver(container);
            var a1 = resolver.Resolve<IServiceA>();
            var a2 = resolver.Resolve<IServiceA>();
            Assert.AreNotEqual(a1, a2);

            var a1a = resolver.Resolve<IServiceA>("Svc1");
            var a2a = resolver.Resolve<IServiceA>("Svc1");
            Assert.AreNotEqual(a1a, a2a);
        }

        
        [Test]
        public void TestTransientWithDelegate()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<IServiceA>(() => new ServiceA(), RegistrationType.Transient);
            container.RegisterType<IServiceA>(() => new ServiceA(), RegistrationType.Transient, "Svc1");
            container.Build();

            var resolver = new ServiceContainerResolver(container);
            var a1 = resolver.Resolve<IServiceA>();
            var a2 = resolver.Resolve<IServiceA>();
            Assert.AreNotEqual(a1, a2);

            var a1a = resolver.Resolve<IServiceA>("Svc1");
            var a2a = resolver.Resolve<IServiceA>("Svc1");
            Assert.AreNotEqual(a1a, a2a);
        }

        
        [Test]
        public void TestCascadeConstructorInjection()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>();
            container.RegisterType<ServiceB, IServiceB>(RegistrationType.SingletonPerRequest);
            container.RegisterType<ServiceC, IServiceC>();
            container.Build();

            
            var resolver1 = new ServiceContainerResolver(container);
            var serviceC1 = resolver1.Resolve<IServiceC>();
            Assert.IsNotNull(serviceC1.GetServiceA());
            Assert.IsNotNull(serviceC1.GetServiceB());

            
            var serviceC1b = resolver1.Resolve<IServiceC>();
            Assert.IsNotNull(serviceC1.GetServiceA());
            Assert.IsNotNull(serviceC1.GetServiceB());
            Assert.AreNotEqual(serviceC1.GetServiceA(), serviceC1b.GetServiceA());
            Assert.AreEqual(serviceC1.GetServiceB(), serviceC1b.GetServiceB());

            
            resolver1 = new ServiceContainerResolver(container);
            var serviceC2 = resolver1.Resolve<IServiceC>();
            Assert.IsNotNull(serviceC1.GetServiceA());
            Assert.IsNotNull(serviceC1.GetServiceB());
            Assert.AreNotEqual(serviceC1.GetServiceA(), serviceC2.GetServiceA());
            Assert.AreNotEqual(serviceC1.GetServiceB(), serviceC2.GetServiceB());
        }
    }

    #region Additional Types for Testing

   
    public class MVCJsonDataObjectsSerializer { }

    public class MVCContainerErrorUnknownServiceException : Exception
    {
        public MVCContainerErrorUnknownServiceException(string message) : base(message) { }
    }

    public interface IMVCSerializer { }

    public enum TRegistrationType
    {
        Transient,
        Singleton,
        SingletonPerRequest
    }

    public class MVCServiceContainer
    {
        internal readonly Dictionary<(Type, string), Func<object>> _registrations = new Dictionary<(Type, string), Func<object>>();
        private bool _built = false;

        public MVCServiceContainer RegisterType<TImplementation, TService>(TRegistrationType regType = TRegistrationType.Transient, string key = "")
            where TImplementation : TService, new()
        {
            _registrations[(typeof(TService), key ?? string.Empty)] = () => new TImplementation();
            return this;
        }

        public MVCServiceContainer RegisterType<TService>(Func<object> factory, TRegistrationType regType = TRegistrationType.Transient, string key = "")
        {
            _registrations[(typeof(TService), key ?? string.Empty)] = factory;
            return this;
        }

        public MVCServiceContainer Build()
        {
            _built = true;
            return this;
        }

        public bool IsBuilt => _built;
    }

    public class ServiceContainerResolver
    {
        private readonly MVCServiceContainer _container;
        private readonly Dictionary<(Type, string), object> _instances = new Dictionary<(Type, string), object>();

        public ServiceContainerResolver(MVCServiceContainer container)
        {
            _container = container;
        }

        public object Resolve(Type serviceType, string key = "")
        {
            var regKey = (serviceType, key ?? string.Empty);
            if (!_container._registrations.ContainsKey(regKey))
                throw new MVCContainerErrorUnknownServiceException("Unknown service");
            if (!_instances.ContainsKey(regKey))
                _instances[regKey] = _container._registrations[regKey]();
            return _instances[regKey];
        }

        public T Resolve<T>(string key = null) where T : class
        {
            return Resolve(typeof(T), key) as T;
        }
    }

    public class ServiceA : IServiceA
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    public class ServiceB : IServiceB
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    public class ServiceAB : IServiceA, IServiceB
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    public class ServiceC : IServiceC
    {
        private readonly IServiceA _serviceA;
        private readonly IServiceB _serviceB;
        public ServiceC(IServiceA serviceA, IServiceB serviceB)
        {
            _serviceA = serviceA;
            _serviceB = serviceB;
        }

        public IServiceA GetServiceA() => _serviceA;
        public IServiceB GetServiceB() => _serviceB;
    }

    public interface IServiceA { }
    public interface IServiceB { }
    public interface IServiceC
    {
        IServiceA GetServiceA();
        IServiceB GetServiceB();
    }

    #endregion

    [TestFixture]
    public class TestContainer
    {
        [Test]
        public void TestNotBuiltContainer()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>();
            var resolver = new ServiceContainerResolver(container);
            Assert.Throws<Exception>(() => resolver.Resolve(typeof(IServiceA)));
        }

        [Test]
        public void TestUnknownService()
        {
            var container = new MVCServiceContainer();
            Assert.Throws<MVCContainerErrorUnknownServiceException>(() =>
            {
                container.RegisterType<ServiceA, IServiceB>();
            });
            Assert.Throws<MVCContainerErrorUnknownServiceException>(() =>
            {
                container.RegisterType(() => new MVCJsonDataObjectsSerializer(), typeof(IServiceB));
            });
            Assert.Throws<MVCContainerErrorUnknownServiceException>(() =>
            {
                container.RegisterType<ServiceA, IMVCSerializer>();
            });
        }

        [Test]
        public void TestTransient()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>(TRegistrationType.Transient);
            container.RegisterType<ServiceA, IServiceA>(TRegistrationType.Transient, "Svc1");
            container.Build();

            var resolver = new ServiceContainerResolver(container);
            var a1 = resolver.Resolve<IServiceA>();
            var a2 = resolver.Resolve<IServiceA>();
            Assert.AreNotEqual(a1, a2);

            var a1a = resolver.Resolve<IServiceA>("Svc1");
            var a2a = resolver.Resolve<IServiceA>("Svc1");
            Assert.AreNotEqual(a1a, a2a);
        }

        [Test]
        public void TestTransientWithDelegate()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<IServiceA>(() => new ServiceA(), TRegistrationType.Transient);
            container.RegisterType<IServiceA>(() => new ServiceA(), TRegistrationType.Transient, "Svc1");
            container.Build();

            var resolver = new ServiceContainerResolver(container);
            var a1 = resolver.Resolve<IServiceA>();
            var a2 = resolver.Resolve<IServiceA>();
            Assert.AreNotEqual(a1, a2);

            var a1a = resolver.Resolve<IServiceA>("Svc1");
            var a2a = resolver.Resolve<IServiceA>("Svc1");
            Assert.AreNotEqual(a1a, a2a);
        }

        [Test]
        public void TestSingleton()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>(TRegistrationType.Singleton);
            container.RegisterType<ServiceA, IServiceA>(TRegistrationType.Singleton, "Svc1");
            container.Build();

            var resolver1 = new ServiceContainerResolver(container);
            var s1 = resolver1.Resolve<IServiceA>();
            var s2 = resolver1.Resolve<IServiceA>();
            Assert.AreEqual(s1, s2);
            var s1a = resolver1.Resolve<IServiceA>("Svc1");
            var s2a = resolver1.Resolve<IServiceA>("Svc1");
            Assert.AreEqual(s1a, s2a);

            var resolver2 = new ServiceContainerResolver(container);
            var s10 = resolver2.Resolve<IServiceA>();
            var s11 = resolver2.Resolve<IServiceA>();
            Assert.AreEqual(s10, s11);
            Assert.AreEqual(s1, s10);
            Assert.AreEqual(s2, s11);
        }

        [Test]
        public void TestSingletonPerRequest()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>(TRegistrationType.SingletonPerRequest);
            container.RegisterType<ServiceA, IServiceA>(TRegistrationType.SingletonPerRequest, "Svc1");
            container.Build();

            var resolver1 = new ServiceContainerResolver(container);
            var s1 = resolver1.Resolve<IServiceA>();
            var s2 = resolver1.Resolve<IServiceA>();
            Assert.AreEqual(s1, s2);
            var s1a = resolver1.Resolve<IServiceA>("Svc1");
            var s2a = resolver1.Resolve<IServiceA>("Svc1");
            Assert.AreEqual(s1a, s2a);

            var resolver2 = new ServiceContainerResolver(container);
            var s00 = resolver2.Resolve<IServiceA>();
            var s10 = resolver2.Resolve<IServiceA>();
            Assert.AreEqual(s00, s10);
            Assert.AreNotEqual(s1, s00);
            Assert.AreNotEqual(s2, s10);
        }

        [Test]
        public void TestSingletonPerRequestWithDelegate()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<IServiceA>(() => new ServiceA(), TRegistrationType.SingletonPerRequest);
            container.RegisterType<IServiceA>(() => new ServiceA(), TRegistrationType.SingletonPerRequest, "Svc1");
            container.Build();

            var resolver1 = new ServiceContainerResolver(container);
            var s1 = resolver1.Resolve<IServiceA>();
            var s2 = resolver1.Resolve<IServiceA>();
            Assert.AreEqual(s1, s2);
            var s1a = resolver1.Resolve<IServiceA>("Svc1");
            var s2a = resolver1.Resolve<IServiceA>("Svc1");
            Assert.AreEqual(s1a, s2a);

            var resolver2 = new ServiceContainerResolver(container);
            var s00 = resolver2.Resolve<IServiceA>();
            var s10 = resolver2.Resolve<IServiceA>();
            Assert.AreEqual(s00, s10);
            Assert.AreNotEqual(s1, s00);
            Assert.AreNotEqual(s2, s10);
        }

        [Test]
        public void TestSingletonWithDelegate()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<IServiceA>(() => new ServiceA(), TRegistrationType.Singleton);
            container.RegisterType<IServiceA>(() => new ServiceA(), TRegistrationType.Singleton, "Svc1");
            container.Build();

            var resolver1 = new ServiceContainerResolver(container);
            var s1 = resolver1.Resolve<IServiceA>();
            var s2 = resolver1.Resolve<IServiceA>();
            Assert.AreEqual(s1, s2);
            var s1a = resolver1.Resolve<IServiceA>("Svc1");
            var s2a = resolver1.Resolve<IServiceA>("Svc1");
            Assert.AreEqual(s1a, s2a);

            var resolver2 = new ServiceContainerResolver(container);
            var s10 = resolver2.Resolve<IServiceA>();
            var s11 = resolver2.Resolve<IServiceA>();
            Assert.AreEqual(s10, s11);
            Assert.AreEqual(s1, s10);
            Assert.AreEqual(s2, s11);
        }

        [Test]
        public void TestCascadeConstructorInjection()
        {
            var container = new MVCServiceContainer();
            container.RegisterType<ServiceA, IServiceA>();
            container.RegisterType<ServiceB, IServiceB>(TRegistrationType.SingletonPerRequest);
            container.RegisterType<ServiceC, IServiceC>();
            container.Build();

            var resolver1 = new ServiceContainerResolver(container);
            var c1 = resolver1.Resolve<IServiceC>();
            Assert.IsNotNull(c1.GetServiceA());
            Assert.IsNotNull(c1.GetServiceB());

            var c1b = resolver1.Resolve<IServiceC>();
            Assert.IsNotNull(c1b.GetServiceA());
            Assert.IsNotNull(c1b.GetServiceB());
            Assert.AreNotEqual(c1.GetServiceA(), c1b.GetServiceA());
            Assert.AreEqual(c1.GetServiceB(), c1b.GetServiceB());

            resolver1 = new ServiceContainerResolver(container);
            var c2 = resolver1.Resolve<IServiceC>();
            Assert.IsNotNull(c2.GetServiceA());
            Assert.IsNotNull(c2.GetServiceB());
            Assert.AreNotEqual(c1.GetServiceA(), c2.GetServiceA());
            Assert.AreNotEqual(c1.GetServiceB(), c2.GetServiceB());
        }
    }
}
