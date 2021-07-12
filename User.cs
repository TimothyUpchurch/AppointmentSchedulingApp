using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AppointmentsC969_TimothyUpchurch
{
    class User
    {
        public int UserID { get; set; }
        public string UserFirstName { get; set; }
        public static List<User> Users { get; set; } = new List<User>();
        public static User MainUser { get; set; } = new User();
        public User() { }
        public User(int userID, string userFirstName)
        {
            UserID = userID;
            UserFirstName = userFirstName;
        }
        public static void LogUserLogin(string userFirstName)
        {
            //string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //StreamWriter file = new StreamWriter(@".\UserLogs.txt", append: true);
            //TimeZone localZone = TimeZone.CurrentTimeZone;
            //try
            //{
            //    await file.WriteLineAsync(userFirstName + " Logged in at: " + localZone.ToLocalTime(DateTime.UtcNow) + " " + localZone.StandardName + " " + localZone.DaylightName);
            //}
            //catch(FileNotFoundException fn)
            //{
            //    throw fn;
            //}
            //finally
            //{
            //    file.Close();
            //}

            // Using (using) instead of try-catch-finally.
            TimeZone localZone = TimeZone.CurrentTimeZone;
            using (StreamWriter file = new StreamWriter(File.Open(@".\UserLogs.txt", FileMode.Append)))
            {
                //await file.WriteLineAsync(userFirstName + " Logged in at: " + localZone.ToLocalTime(DateTime.UtcNow) + " " + localZone.StandardName + " " + localZone.DaylightName);
                file.WriteLine(userFirstName + " Logged in at: " + localZone.ToLocalTime(DateTime.UtcNow) + " " + localZone.StandardName + " " + localZone.DaylightName);
            }
        }
        public string UsersUpcomingAppointments()
        {
            // Look through each appointment where user id matches.
            foreach (Appointment appointment in Appointment.Appointments)
            {
                if (appointment.UserID == UserID)
                {
                    if (appointment.StartDateTime >= DateTime.Now && appointment.StartDateTime <= DateTime.Now.AddMinutes(15))
                    {
                        return $"Hey, {UserFirstName}. You have an appointment at {appointment.StartDateTime} with {appointment.Customer}.";
                    }
                }
            }
            return "No Upcoming Appointments";
        }
        public bool UserLoggedIn { get; set; } = false;       
        public static void UserSchedule(string userName)
        {
            // Loop through each appointment
            // Sort by Username and order by date
            // set appointmentsbyuser to filtered list of appointments
            Appointment.AppointmentsByUser.Clear();
            Appointment.AppointmentsByUser = new ObservableCollection<Appointment>(Appointment.Appointments.Where(appointment => appointment.User == userName).OrderBy(appointment => appointment.StartDateTime));
        }
    }
}
