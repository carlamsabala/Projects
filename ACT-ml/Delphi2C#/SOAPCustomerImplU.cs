using System;
using System.Web.Services;
using SOAPCustomerIntfU;
using BOCustomersU;
using WSHelperCustomersU;
using MVCFramework.Serializer.Defaults;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class SOAPCustomer : WebService, ISOAPCustomer
{
    [WebMethod]
    public string GetCustomers()
    {
        WSHelperCustomers wsHelper = new WSHelperCustomers();
        try
        {
            var customers = wsHelper.GetCustomers();
            try
            {
                return GetDefaultSerializer().SerializeCollection(customers);
            }
            finally
            {
                if (customers is IDisposable disposableCustomers)
                {
                    disposableCustomers.Dispose();
                }
            }
        }
        finally
        {
            if (wsHelper is IDisposable disposableWsHelper)
            {
                disposableWsHelper.Dispose();
            }
        }
    }
}
