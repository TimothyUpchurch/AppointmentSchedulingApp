using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for AppointmentWindow.xaml
    /// </summary>
    public partial class AppointmentWindow : Window
    {
        public AppointmentWindow()
        {
            InitializeComponent();
            LoadAppointments();

            // Assign datacontext to list of Appointments
            DataContext = Appointment.Appointments;
        }

        // Db init
        MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySqlKey"].ConnectionString);
        MySqlDataAdapter da;
        DataTable dt;
        MySqlCommand cmd;
        TimeZone localZone = TimeZone.CurrentTimeZone;

        private void DraggableWindow(object sender, MouseButtonEventArgs e)
        {
            // Allows user to drag window across the screen when left clicking the window
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void ExitToDashboard(object sender, MouseButtonEventArgs e)
        {  
            // Exit Appointment window and navigate back to the main dashboard
            Dashboard dashboard = new Dashboard();
            dashboard.Show();
            Close();
        }
        private void LoadAppointments()
        {
            // Command to select appointments - joining customer table and user table to obtain relevant information from both tables
            da  = new MySqlDataAdapter("SELECT appointmentId, customer.customerName as customer, user.userName as user, type, start, end, customerId, userId from appointment JOIN customer USING(customerId) JOIN user USING(userId)", connection);
            dt = new DataTable();

            // Clear Appointments list to avoid duplicates
            Appointment.Appointments.Clear();

            // Get appointments from DB
            GetAppointments(connection, da, dt);
            // Create An Appointment object for each row returned from the DB and store them in appointments list.
            PushAppointmentsToList(dt);
        }
        private async void GetAppointments(MySqlConnection connection, MySqlDataAdapter da, DataTable dt)
        {
            // async await used so data table is filled correctly before trying to run PushAppointmentsToList which uses the data table to fill the appointments list.
            try
            {
                connection.Open();
                await da.FillAsync(dt);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        private void PushAppointmentsToList(DataTable dt)
        {
            // For each db.Row
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // localZone.ToLocalTime(Convert.ToDateTime(dt.Rows[i]["start"])) ---> Converts time from DB UTC to users local timezone also accounts for daylightsavings time.
                // Add each row to Appointments list. Appointments list will be used as the data source for the appointsmentsGridView. 
                Appointment.Appointments.Add(new Appointment(Int32.Parse(dt.Rows[i]["appointmentId"].ToString()), dt.Rows[i]["customer"].ToString(), dt.Rows[i]["user"].ToString(), dt.Rows[i]["type"].ToString(), localZone.ToLocalTime(Convert.ToDateTime(dt.Rows[i]["start"])), localZone.ToLocalTime(Convert.ToDateTime(dt.Rows[i]["end"])), Int32.Parse(dt.Rows[i]["customerId"].ToString()), Int32.Parse(dt.Rows[i]["userId"].ToString())));
            }            
        }
        private void DeleteAppointment(object sender, RoutedEventArgs e)
        {
            // Grabs the selected row and casts the datarow to an Appointment object in order to use Appointment properties
            Appointment appointment = (Appointment)appointmentGridView.SelectedItem;

            // dataRowView can be null due to unselected row by user. Check if null ((int)appointment?.AppointmentID)  
            try
            {
                // Using the Null Conditional Operator To Avoid NullReferenceExceptions
                // If exception is thrown code after will not be run.
                int indexToDelete = (int)appointment?.AppointmentID;

                // Ask user if they are sure if they want to delete
                var Result = MessageBox.Show("Are You Sure You Want To Delete This Appointment?", "Delete Appointment", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (Result == MessageBoxResult.Yes)
                {
                    // Delete appointment from DB where appointmentId = @appointmentId
                    cmd = new MySqlCommand("DELETE FROM appointment WHERE appointmentId = @appointmentId", connection);
                    cmd.Parameters.AddWithValue("@appointmentId", indexToDelete);
                    try
                    {
                        connection.Open();
                        dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        MessageBox.Show("Appointment Has Been Deleted.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                    // If appointment is deleted reload appointments into the datagrid.
                    LoadAppointments();
                }
                else
                {
                    // Reset selected item.
                    appointmentGridView.SelectedItem = null;
                }
            }
            catch // No row selected when clicking the delete button
            {
                MessageBox.Show("Please Select An Appointment To Be Deleted.");
            }
        }
        private void AddAppointment(object sender, RoutedEventArgs e)
        {
            // Open add appointment window and close the appointment window
            AddAppointmentWindow addAppointmentWindow = new AddAppointmentWindow();
            addAppointmentWindow.Show();
            Close();
        }
        private void UpdateAppointment(object sender, RoutedEventArgs e)
        {
            // Checks if a row has been selected
            if (appointmentGridView.SelectedItem != null) 
            {
                Appointment appointment = (Appointment)appointmentGridView.SelectedItem; // Cast selected row to Appointment object
                DateTime date = Convert.ToDateTime(appointment.StartDateTime.ToString("M/d/yyyy")); // Format the startdatetime to M/d/yyyy then convert back to datetime

                UpdateAppointmentWindow updateAppointmentWindow = new UpdateAppointmentWindow(appointment.Customer, appointment.User, appointment.AppointmentType, date, appointment.StartDateTime.ToString(), appointment.EndDateTime.ToString(), appointment.AppointmentID);
                updateAppointmentWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Please Select An Appointment To Update."); // No row selected 
            }
        }
        private void ShowAllAppointments(object sender, RoutedEventArgs e)
        {
            // Assign datacontext to list of All Appointments
            DataContext = Appointment.Appointments; // sets datacontext to unfiltered appointments list
        }
        private void HandleDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            // Event triggered each time a date is changed on the calendar. Depending on which radio button is clicked different appointments will show.
            if (appointmentByDay.IsChecked.Value)
            {
                AppointmentByDay(sender, e);
            }
            else if (appointmentByWeek.IsChecked.Value)
            {
                AppoitmentByWeek(sender, e);
            }
            else if (appointmentByMonth.IsChecked.Value)
            {
                AppointmentByMonth(sender, e);
            }
        }
        private void AppointmentByDay(object sender, RoutedEventArgs e)
        {
            string selectedDay = DateTime.Now.ToString("M/d/yyyy"); // If the user checks the Day radiobutton without changing the date on the calendar first - assign selectedDay to the current day and format the date M/d/yyyy
            if (appointmentCalendar.SelectedDate.HasValue)
            {
                selectedDay = appointmentCalendar.SelectedDate.Value.ToString("M/d/yyyy");
            }
            // Using Linq and a Lamda expression to filter appointments occuring on the selected day.
            DataContext = Appointment.Appointments.Where(appointment => appointment.StartDateTime.ToString().Contains(selectedDay));
        }
        private void AppoitmentByWeek(object sender, RoutedEventArgs e)
        {
            // Checks the selected day - adds 7 days and filters out the appointments that occur between the selected day and the selected day + 7 days
            string selectedDay = DateTime.Now.ToString("M/d/yyyy");
            DateTime spanDay = new DateTime();
            if (appointmentCalendar.SelectedDate.HasValue)
            {
                selectedDay = appointmentCalendar.SelectedDate.Value.ToString("M/d/yyyy");
                spanDay = appointmentCalendar.SelectedDate.Value.AddDays(7);
            }
            else
            {
                appointmentCalendar.SelectedDate = DateTime.Now;
                spanDay = appointmentCalendar.SelectedDate.Value.AddDays(7);
            }
            // Using Linq and a lamda expression to filter appointments between the start day and the startday + 7 days.
            DataContext = Appointment.Appointments.Where(appointment => appointment.StartDateTime >= Convert.ToDateTime(selectedDay) && appointment.StartDateTime <= spanDay);
        }
        private void AppointmentByMonth(object sender, RoutedEventArgs e)
        {
            string selectedMonth = DateTime.Now.Month.ToString("0");
            if (appointmentCalendar.SelectedDate.HasValue)
            {
                selectedMonth = appointmentCalendar.SelectedDate.Value.Month.ToString("0");
            }
            // Using Linq and a Lambda expression to filter all appointments starting with the selected month
            DataContext = Appointment.Appointments.Where(appointment => appointment.StartDateTime.ToString().StartsWith(selectedMonth));
        }
    }
}
