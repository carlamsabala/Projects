namespace YourNamespace
{
    public interface IUsersService
    {
        string GetUserNameByID(int id);
    }

    public interface ICustomersService
    {
        string GetCustomerNameByID(int id);
    }

    public interface ICommonService
    {
        string GetID();
    }
}
