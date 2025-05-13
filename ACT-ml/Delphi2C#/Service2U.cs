using System;
using ServicesInterfaceU;
using Spring.Container.Common;

namespace ServiceNamespace
{
    public class CustomersService : ICustomersService
    {
        [Inject]
        private ICommonService commonService;

        public string GetCustomerNameByID(int id)
        {
            return string.Format("Customer #{0} (CommonServiceID = {1})", id, commonService.GetID);
        }
    }
}
