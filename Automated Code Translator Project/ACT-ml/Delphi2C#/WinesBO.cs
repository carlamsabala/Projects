using System.Collections.Generic;
using MVCFramework.Serializer.Commons;

namespace WinesBO
{
    [MVCNameCase("ncLowerCase")]
    public class Wine
    {
        private string _year;
        private string _name;
        private string _picture;
        private string _grapes;
        private int _id;
        private string _description;
        private string _country;
        private string _region;

        public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Year { get => _year; set => _year = value; }
        public string Grapes { get => _grapes; set => _grapes = value; }
        public string Country { get => _country; set => _country = value; }
        public string Region { get => _region; set => _region = value; }
        public string Description { get => _description; set => _description = value; }
        public string Picture { get => _picture; set => _picture = value; }
    }

    public class Wines : List<Wine>
    {
        public Wines() : base() { }
    }
}
