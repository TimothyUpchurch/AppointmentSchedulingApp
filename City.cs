using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentsC969_TimothyUpchurch
{
    class City
    {
        public int CityID { get; set; }
        public string CityName { get; set; }
        public int CountryID { get; set; }
        public static List<City> Cities { get; set; } = new List<City>();
        public City(int cityID, string cityName, int countryID)
        {
            CityID = cityID;
            CityName = cityName;
            CountryID = countryID;
        }
    }
}
