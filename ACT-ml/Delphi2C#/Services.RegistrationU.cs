using MVCFramework.Container;
using Services.PeopleU;
using Services.InterfacesU;
using Services.ConnectionU;

namespace Services.RegistrationU
{
    public static class ServicesRegistration
    {
        public static void RegisterServices(IMVCServiceContainer container)
        {
            container.RegisterType<IPeopleService, PeopleService>();
            container.RegisterType<IConnectionService, ConnectionService>(RegistrationType.SingletonPerRequest);
        }
    }
}
