using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentsC969_TimothyUpchurch
{
    class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public int AddressID { get; set; }
        public static ObservableCollection<Customer> Customers { get; set; } = new ObservableCollection<Customer>();
        public Customer(int customerId, string customerName)
        {
            CustomerID = customerId;
            CustomerName = customerName;
        }
        public Customer(int customerId, string customerName, string address, string phone, string city, int addressId)
        {
            CustomerID = customerId;
            CustomerName = customerName;
            Address = address;
            Phone = phone;
            City = city;
            AddressID = addressId;
        }
    }
}
