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
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();

            helloUser.Text = "Hello, " + UppercaseFirst(User.MainUser.UserFirstName) + ".";

            // Load data from the Database to use as datasources throughout the application
            LoadCities();
            LoadUsers();
            LoadAppointments();
            LoadCustomers();

            // Only wanting to show the "You have an appointment at x" message upon logging in.
            // Created a UserLoggedIn property to keep track of user logins as a boolean value to only show message once.
            if (!User.MainUser.UserLoggedIn){ MessageBox.Show(User.MainUser.UsersUpcomingAppointments());}

            // Setting UserLoggedIn = true after displaying the UsersUpcomingAppointments message
            // Otherwise the message would show every time the user navigates back to the dashboard.
            User.MainUser.UserLoggedIn = true;
        }

        // DB init
        MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySqlKey"].ConnectionString);
        MySqlDataAdapter da;
        DataTable dt;

        // Using to convert database time to users time - also accounts for daylightsavings time.
        TimeZone localZone = TimeZone.CurrentTimeZone;

        private void DraggableWindow(object sender, MouseButtonEventArgs e)
        {
            // allows the user to drag the window around the screen when holding left click on the window.
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void ExitDashboard(object sender, MouseButtonEventArgs e)
        {
            // Shut down the application instead of going back to the login screen
            // Makes on less click the user has to perform when trying to exit the application
            System.Windows.Application.Current.Shutdown();
        }
        private string UppercaseFirst(string str)
        {
            // Uppercase the first letter of any string passed.
            // Check for empty string.
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        private void LoadCustomerWindow(object sender, MouseButtonEventArgs e)
        {
            // Opens up the customer window when the customer ui element is selected
            CustomerWindow customerWindow = new CustomerWindow();
            customerWindow.Show();
            Close();
        }
        private void LoadAppointmentWindow(object sender, MouseButtonEventArgs e)
        {
            // Opens up the appointment window when the appointment ui element is selected
            AppointmentWindow appointmentWindow = new AppointmentWindow();
            appointmentWindow.Show();
            Close();
        }
        private void LoadReportWindow(object sender, MouseButtonEventArgs e)
        {
            // Opens up the report window when the report ui element is selected
            ReportWindow report = new ReportWindow();
            report.Show();
            Close();
        }
        // Using Async/Await when loading data from the database to ensure that all data is properly loaded
        private async void LoadCities()
        {
            // Instantiating City objects and populating Cities list to use as a datasource through the application
            try
            {
                connection.Open();
                da = new MySqlDataAdapter("SELECT * FROM city", connection);
                dt = new DataTable();
                await da.FillAsync(dt);

                // Clear List so duplicates are not created when exiting and returning to the dashboard
                City.Cities.Clear();

                // Loop through each row in the data table and add a City for each row. Populate Cities List with each City.
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    City.Cities.Add(new City(Int32.Parse(dt.Rows[i]["cityId"].ToString()), dt.Rows[i]["city"].ToString(), Int32.Parse(dt.Rows[i]["countryId"].ToString())));
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
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
        private async void LoadUsers()
        {
            // Instantiating User objects and populating Users list to use as a datasource through the application
            try
            {
                connection.Open();
                da = new MySqlDataAdapter("SELECT * FROM user", connection);
                dt = new DataTable();
                await da.FillAsync(dt);

                // Clear List so duplicates are not created when exiting and returning to the dashboard
                User.Users.Clear();

                // Loop through each row in the data table and add a User for each row. 
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    User.Users.Add(new User(Int32.Parse(dt.Rows[i]["userId"].ToString()), dt.Rows[i]["userName"].ToString()));
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
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
        private async void LoadCustomers()
        {
            // Instantiating Customer objects and populating Customers list to use as a datasource through the application
            try
            {
                connection.Open();
                da = new MySqlDataAdapter("SELECT * FROM customer", connection);
                dt = new DataTable();
                await da.FillAsync(dt);

                // Clear List so duplicates are not created when exiting and returning to the dashboard
                Customer.Customers.Clear();

                // Loop through each row in the data table and add a Customer for each row
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // Add customer to Customer list for each row
                    Customer.Customers.Add(new Customer(Int32.Parse(dt.Rows[i]["customerId"].ToString()), dt.Rows[i]["customerName"].ToString()));
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
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
        private async void LoadAppointments()
        {
            // Instantiating Appointment objects and populating Appointments list to use as a datasource through the application
            try
            {
                connection.Open();
                da = new MySqlDataAdapter("SELECT appointmentId, customer.customerName as customer, user.userName as user, type, start, end, customerId, userId from appointment JOIN customer USING(customerId) JOIN user USING(userId)", connection);
                dt = new DataTable();
                await da.FillAsync(dt);

                // Clear List so duplicates are not created when exiting and returning to the dashboard
                Appointment.Appointments.Clear();

                // Add appointment to appointmens list for each row
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // localZone.ToLocalTime(Convert.ToDateTime(dt.Rows[i]["start"])) ---> Converts time from DB UTC to users local timezone also accounts for daylightsavings time.
                    // Add each row to Appointments list. Appointments list will be used as the data source for the appointsmentsGridView. 
                    Appointment.Appointments.Add(new Appointment(Int32.Parse(dt.Rows[i]["appointmentId"].ToString()), dt.Rows[i]["customer"].ToString(), dt.Rows[i]["user"].ToString(), dt.Rows[i]["type"].ToString(), localZone.ToLocalTime(Convert.ToDateTime(dt.Rows[i]["start"])), localZone.ToLocalTime(Convert.ToDateTime(dt.Rows[i]["end"])), Int32.Parse(dt.Rows[i]["customerId"].ToString()), Int32.Parse(dt.Rows[i]["userId"].ToString())));
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
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
    }
}
