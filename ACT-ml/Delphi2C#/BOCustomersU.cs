using System;
using System.Collections.Generic;

namespace BOCustersU
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MVCNameCaseAttribute : Attribute
    {
        public string NameCase { get; }
        public MVCNameCaseAttribute(string nameCase)
        {
            NameCase = nameCase;
        }
    }

   
    [MVCNameCase("lowercase")]
    public class Customer
    {
        private string _firstName;
        private string _middleName;
        private string _surname;

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        public string MiddleName
        {
            get { return _middleName; }
            set { _middleName = value; }
        }

        public string Surname
        {
            get { return _surname; }
            set { _surname = value; }
        }
    }

        public class Customers : List<Customer>
    {
    }
}
