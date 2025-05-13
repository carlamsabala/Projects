using BOCustomersU;

namespace YourNamespace
{
    public class WSHelperCustomers
    {
        public TCustomers GetCustomers()
        {
            TCustomers customers = new TCustomers();
            TCustomer customer = new TCustomer();
            customer.FirstName = "Joe";
            customer.MiddleName = "M";
            customer.Surname = "Bloggs";
            customers.Add(customer);
            customer = new TCustomer();
            customer.FirstName = "Mary";
            customer.MiddleName = "J";
            customer.Surname = "Jones";
            customers.Add(customer);
            return customers;
        }
    }
}
