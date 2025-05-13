using System;
using System.Runtime.InteropServices;
using System.Web.Services;
using System.Web.Services.Protocols;

[Guid("9D4C2E66-F0AB-470E-9A48-2084DAD75FD3")]
[WebServiceBinding(Name = "ISOAPCustomerSoap", Namespace = "http://tempuri.org/")]
public interface ISOAPCustomer
{
    [SoapDocumentMethod]
    string GetCustomers();
}
