using System.Runtime.InteropServices;

namespace ServicesInterfaceU
{
    [Guid("D54AF728-7688-40DE-B10C-E6D63949531E")]
    public interface IUsersService
    {
        string GetUserNameByID(int id);
    }

    [Guid("DC94C34E-13A2-4406-8961-6A407B792DD3")]
    public interface ICustomersService
    {
        string GetCustomerNameByID(int id);
    }

    [Guid("EAA26199-4142-4698-9C17-5D241D9984AA")]
    public interface ICommonService
    {
        string GetID();
    }
}
