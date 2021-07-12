using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AppointmentsC969_TimothyUpchurch
{
    /// <summary>
    /// Interaction logic for UpdateAppointmentWindow.xaml
    /// </summary>
    public partial class UpdateAppointmentWindow : Window
    {
        public UpdateAppointmentWindow(string customerName, string userName, string appointmentType, DateTime date, string startTime, string endTime, int appointmentId)
        {
            InitializeComponent();
            // Set the previous appointment variables to the information obtained from the selected appointment
            previousCustomerName = customerName;
            previousUserName = userName;
            previousAppointmentType = appointmentType;
            previousDate = date;
            previousStartTime = startTime;
            previousEndTime = endTime;
            previousAppointmentId = appointmentId;

            // Loop through each appoint. and add every appoint. besides last to appointmentStartTimes list. (Appointment should not start at 5:00PM)
            for (int i = 0; i < Appointment.AppointmentTimes.Count; i++)
            {
                if (i != Appointment.AppointmentTimes.Count - 1)
                    appointmentStartTimes.Add(Appointment.AppointmentTimes[i].ToString("h:mm tt"));
            }

            // set itemsource & datacontext
            DataContext = this;
            customerNameComboBox.ItemsSource = Customer.Customers;
            userNameComboBox.ItemsSource = User.Users;
            startTimeComboBox.ItemsSource = appointmentStartTimes;
            endTimeComboBox.ItemsSource = appointmentEndTimes;

            // assign form fields to values obtained from the appointment window
            customerNameComboBox.Text = previousCustomerName;
            userNameComboBox.Text = previousUserName;
            appointmentTypeText.Text = previousAppointmentType;
            appointmentDatePicker.SelectedDate = previousDate;
            startTimeComboBox.Text = Convert.ToDateTime(previousStartTime).ToString("h:mm tt");
            endTimeComboBox.Text = Convert.ToDateTime(previousEndTime).ToString("h:mm tt");

            // Set start dart of datetime picker for todays date. Appointments can not be scheduled for the past.
            // If user is trying to update an appointment where the scheduled date has already past the select date will open up the calendar on that day and allow them to select a date prior to todays date.
            // This will be checked for when clicking the update appointment button. New appointments should be scheduled after or on the current day.
            appointmentDatePicker.DisplayDateStart = DateTime.Today;
        }

        string previousCustomerName = "";
        string previousUserName = "";
        string previousAppointmentType = "";
        DateTime previousDate = DateTime.Now;
        string previousStartTime = "";
        string previousEndTime = "";
        int previousAppointmentId = 0;

        // Start Times pull from the same data source regardless of end times. Standard List is fine.
        private List<string> appointmentStartTimes = new List<string>();
        // Observablecollection since appointments are added after the itemsource has been set. --Collection will be updated.
        private ObservableCollection<string> appointmentEndTimes = new ObservableCollection<string>();

        private void DraggableWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void HandleUpdateAppointment(object sender, RoutedEventArgs e)
        {
            // Retrieve data from add appointment form.
            string customerName = customerNameComboBox.Text;
            string userName = userNameComboBox.Text;
            string appointmentType = appointmentTypeText.Text;
            DateTime date = DateTime.Now;
            // Using if to avoid InvalidOperationException: 'Nullable object must have a value.'
            if (appointmentDatePicker.SelectedDate.HasValue) { date = appointmentDatePicker.SelectedDate.Value; }
            string startTime = startTimeComboBox.Text;
            string endTime = endTimeComboBox.Text;

            if (!CheckAllFieldsOccupied(customerName, userName, appointmentType, startTime, endTime))
            {
                MessageBox.Show("Please Select A Value For Each Field.");
            }
            else if (appointmentType.Any(char.IsDigit))
            {
                MessageBox.Show("Appointment Type Should Only Contain Letters.");
            }
            else if (Convert.ToDateTime(date.ToString("M/d/yyyy") + " " + startTime) < DateTime.Now) // checks the time from the drop down compared to the system time.
            {
                MessageBox.Show("Appointments Can Be Scheduled Today or After Today.");
            }
            else if (CheckConflictingTimes(startTime, endTime, date, previousAppointmentId)){}
            else
            {
                // creating database connection string as well as Data table.
                string connectionString = ConfigurationManager.ConnectionStrings["MySqlKey"].ConnectionString;
                MySqlConnection connection = new MySqlConnection(connectionString);
                DataTable dt = new DataTable();

                UpdateAppointment(connection, dt, GetCustomerID(customerName), GetUserID(userName), appointmentType, TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime($"{date.ToString("M/d/yyyy") + " " + startTime}")), TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime($"{date.ToString("M/d/yyyy") + " " + endTime}")));
            }
        }
        private bool CheckAllFieldsOccupied(string customerName, string userName, string appointmentType, string startTime, string endTime) // returns true if no field is empty
        {
            if (customerName.Length != 0 && userName.Length != 0 && appointmentType.Length != 0 && appointmentDatePicker.SelectedDate.HasValue && startTime.Length != 0 && endTime.Length != 0)
            {
                return true;
            }
            return false;
        }
        private void StartTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear previous end times.
            appointmentEndTimes.Clear();
            int startTimeIndex = startTimeComboBox.SelectedIndex;
            endTimeComboBox.IsEnabled = true;

            // Add times greater than the start time to the end time list.
            for (int i = 0; i < appointmentStartTimes.Count; i++)
            {
                if (i > startTimeIndex)
                {
                    appointmentEndTimes.Add(appointmentStartTimes[i]);
                }
            }
            // adds the last time in AppointmenTimes (5:00)
            appointmentEndTimes.Add(Appointment.AppointmentTimes[Appointment.AppointmentTimes.Count - 1].ToString("h:mm tt"));
        }
        private void ExitUpdateAppointmentWindow(object sender, MouseButtonEventArgs e)
        {
            AppointmentWindow appointmentWindow = new AppointmentWindow();
            appointmentWindow.Show();
            Close();
        }
        private void UpdateAppointment(MySqlConnection connection, DataTable dt, int customerId, int userId, string appointmentType, DateTime startTime, DateTime endTime)
        {
            MySqlCommand cmd = new MySqlCommand("UPDATE appointment SET customerId = @customerId, userId = @userId, type = @appointmentType, start = @startTime, end = @endTime, lastUpdate = @lastUpdate, lastUpdateBy = @lastUpdateBy WHERE appointmentId = @appointmentId", connection);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@appointmentType", appointmentType);
            cmd.Parameters.AddWithValue("@startTime", startTime);
            cmd.Parameters.AddWithValue("@endTime", endTime);
            // Convert time to UTC when adding to the database. When showing the time in the appointments grid view the time will be converted back to the users local timezone.
            cmd.Parameters.AddWithValue("@lastUpdate", TimeZoneInfo.ConvertTimeToUtc(DateTime.Now));
            cmd.Parameters.AddWithValue("@lastUpdateBy", User.MainUser.UserFirstName);
            cmd.Parameters.AddWithValue("@appointmentId", previousAppointmentId);
            
            try
            {
                connection.Open();
                dt.Load(cmd.ExecuteReader());
                MessageBox.Show("Appointment Updated Successfully.");

                // Close update window after updating appointment. Go back to the appointment window
                AppointmentWindow appointmentWindow = new AppointmentWindow();
                appointmentWindow.Show();
                Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        private int GetCustomerID(string customerName)
        {
            int customerId = 0;
            // Loops through each customer for a match. once found set customerId equal to the matched customer ID.
            foreach (Customer customer in Customer.Customers)
            {
                if (customer.CustomerName == customerName) { customerId = customer.CustomerID; }
            }
            return customerId;
        }
        private int GetUserID(string userName)
        {
            int userId = 0;
            foreach (User user in User.Users)
            {
                if (user.UserFirstName == userName) { userId = user.UserID; }
            }
            return userId;
        }
        private bool CheckConflictingTimes(string startTime, string endTime, DateTime date, int previousAppointmentId)
        {
            DateTime usersSelectedStart = Convert.ToDateTime(date.ToString("M/d/yyyy") + " " + startTime);
            DateTime usersSelectedEnd = Convert.ToDateTime(date.ToString("M/d/yyyy") + " " + endTime);

            // Using Linq and a lambda expression here to easily filter out the appointment that is currently being updated and assigning the rest to a new list.
            // The current appointment is taking up a time spot. Ex: 10:00 - 12:00. 
            // If the user wanted to update the appointment to 11:00-12:00 it would not be allowed without filtering the list of appointments first to exclude the appointment being updated.
            List<Appointment> appointments = new List<Appointment>(Appointment.Appointments.Where(appointment => appointment.AppointmentID != previousAppointmentId));
            foreach (Appointment appointment in appointments)
            {
                // If the start time is between the start and end time of an already occuring appointment return true - indicating a conflict has occured.
                if ((usersSelectedStart >= appointment.StartDateTime && usersSelectedStart < appointment.EndDateTime))
                {
                    MessageBox.Show($"Appointments Are All Filled Between {appointment.StartDateTime.ToString("h:mm tt")} and {appointment.EndDateTime.ToString("h:mm tt")}");
                    return true;
                }
                // checks for overlapping appointments.
                if ((usersSelectedEnd > appointment.StartDateTime && usersSelectedEnd <= appointment.EndDateTime))
                {
                    MessageBox.Show($"Appointments Are All Filled Between {appointment.StartDateTime.ToString("h:mm tt")} and {appointment.EndDateTime.ToString("h:mm tt")}");
                    return true;
                }
                if (usersSelectedEnd > appointment.StartDateTime && appointment.StartDateTime > usersSelectedStart && usersSelectedEnd.ToString("M/d/yyyy") == appointment.StartDateTime.ToString("M/d/yyyy"))
                {
                    MessageBox.Show($"An appointment is taking place between {appointment.StartDateTime.ToString("h:mm tt")} and {appointment.EndDateTime.ToString("h:mm tt")}. Try scheduling an appointment to end before {appointment.StartDateTime.ToString("h:mm tt")} or start after {appointment.EndDateTime.ToString("h:mm tt")}.");
                    return true;
                }
            }
            return false;
        }
    }
}
