using System;
using ServicesInterfaceU;
using Spring.Container.Common;

namespace ServiceNamespace
{
    public class UsersService : IUsersService
    {
        [Inject]
        private ICommonService commonService;

        public string GetUserNameByID(int id)
        {
            return string.Format("User #{0} (CommonServiceID = {1})", id, commonService.GetID);
        }
    }
}
