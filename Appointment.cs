using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentsC969_TimothyUpchurch
{
    class Appointment
    {
        public int AppointmentID { get; set; }
        public int CustomerID { get; set; }
        public string Customer { get; set; }
        public int UserID { get; set; }
        public string User { get; set; }
        public string AppointmentType { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public static ObservableCollection<Appointment> Appointments { get; set; } = new ObservableCollection<Appointment>();
        public static List<DateTime> AppointmentTimes { get; set; } = new List<DateTime>(new DateTime[]
        {
            Convert.ToDateTime("8:00 am"), Convert.ToDateTime("8:30 am"), Convert.ToDateTime("9:00 am"), Convert.ToDateTime("9:30 am"), Convert.ToDateTime("10:00 am"), Convert.ToDateTime("10:30 am"),
            Convert.ToDateTime("11:00 am"), Convert.ToDateTime("11:30 am"), Convert.ToDateTime("12:00 pm"),
            Convert.ToDateTime("12:30 pm"), Convert.ToDateTime("1:00 pm"), Convert.ToDateTime("1:30 pm"), Convert.ToDateTime("2:00 pm"), Convert.ToDateTime("2:30 pm"),
            Convert.ToDateTime("3:00 pm"), Convert.ToDateTime("3:30 pm"), Convert.ToDateTime("4:00 pm"), Convert.ToDateTime("4:30 pm"), Convert.ToDateTime("5:00 pm")
        });
        public static ObservableCollection<Appointment> AppointmentsByUser { get; set; } = new ObservableCollection<Appointment>();
        public Appointment(int appointmentId, string customer, string user, string type, DateTime start, DateTime end, int customerId, int userId)
        {
            AppointmentID = appointmentId;
            Customer = customer;
            User = user;
            AppointmentType = type;
            StartDateTime = start;
            EndDateTime = end;
            CustomerID = customerId;
            UserID = userId;
        }
    }
}
