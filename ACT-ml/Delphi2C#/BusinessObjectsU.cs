using System;

namespace BusinessObjectsU
{
    public class Person
    {
        private string _firstName;
        private string _lastName;
        private DateTime _dob;
        private bool _married;

        public string FirstName
        {
            get => _firstName;
            set => SetFirstName(value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetLastName(value);
        }

        public DateTime DOB
        {
            get => _dob;
            set => SetDOB(value);
        }

        public bool Married
        {
            get => _married;
            set => SetMarried(value);
        }

        private void SetFirstName(string value)
        {
            _firstName = value;
        }

        private void SetLastName(string value)
        {
            _lastName = value;
        }

        private void SetDOB(DateTime value)
        {
            _dob = value;
        }

        private void SetMarried(bool value)
        {
            _married = value;
        }
    }
}
