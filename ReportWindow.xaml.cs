using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            InitializeComponent();

            // Set the datacontext and itemsources
            DataContext = this;
            userNameComboBox.ItemsSource = User.Users;
            customerNameComboBox.ItemsSource = Customer.Customers;
        }
        private void ExitToDashboard(object sender, MouseButtonEventArgs e)
        {
            // Navigate back to the dashboard and close this window
            Dashboard dashboard = new Dashboard();
            dashboard.Show();
            Close();
        }
        private void DraggableWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void TypesByMonth(string month)
        {
            // Get user selected month from combobox
            string selectedMonth = month;

            // Need to filter Appointments list to a new list by user selected month. Linq + Lambda
            List<Appointment> appointmentsByMonth = new List<Appointment>(Appointment.Appointments.Where(appointment => appointment.StartDateTime.ToString().StartsWith(selectedMonth)));

            if (appointmentsByMonth.Count != 0) // If the list is not empty select the appointmentType and group by the type and count
            {
                var appointmentTypes = appointmentsByMonth.Select(appointment => appointment.AppointmentType).GroupBy(count => count).Select(type => new { AppointmentType = type.Key, Count = type.Count() });
                foreach (var appointmentType in appointmentTypes)
                {
                    string meetings = "";
                    string auxiliaryVerb = "";
                    // Shows the appointment type and count
                    if (appointmentType.Count == 1)
                    {
                        meetings = "Meeting";
                        auxiliaryVerb = "is";
                    }
                    else
                    {
                        meetings = "Meetings";
                        auxiliaryVerb = "are";
                    }
                    MessageBox.Show($"There {auxiliaryVerb} {appointmentType.Count} {appointmentType.AppointmentType} {meetings} This Month.");
                }
            }
            else
            {
                MessageBox.Show("No Appointments Scheduled For This Month.");
            }
        }
        private void AppointmentsPerCustomer(string customerName)
        {
            // Using LINQ and a Lambda expression to filter out the Appointments list where the customer name is equal to the selected customers name. 
            // Displaying the name and the count of all the appointments the selected customer has.
            var customerAppointments = Appointment.Appointments.Where(appointment => appointment.Customer.Equals(customerName)).Select(appointment => appointment.Customer).GroupBy(count => count).Select(customer => new { Customer = customer.Key, Count = customer.Count() });

            // If the count is 0 no appointment was found with a matching customer name.
            if (customerAppointments.Count() == 0)
            {
                MessageBox.Show($"{customerName} Has No Appointments.");
            }

            // If the count is not 0: Display the customer and the number of appointments the customer has.
            foreach (var appointment in customerAppointments)
            {
                string appointments = "Appointments";

                if (appointment.Count == 1)
                {
                    appointments = "Appointment";
                }
                else
                {
                    appointments = "Appointments";
                }
                MessageBox.Show($"{appointment.Customer} Has {appointment.Count} {appointments}.");
            }
        }
        private void HandleTypeSelectionChanged(object sender, SelectionChangedEventArgs e) // Determines the month from the index of the selected item. index 0 = January. Month = 1
        {
            string selectedMonth = "";
            switch (selectMonthComboBox.SelectedIndex)
            {
                case 0:
                    selectedMonth = "1";
                    break;
                case 1:
                    selectedMonth = "2";
                    break;
                case 2:
                    selectedMonth = "3";
                    break;
                case 3:
                    selectedMonth = "4";
                    break;
                case 4:
                    selectedMonth = "5";
                    break;
                case 5:
                    selectedMonth = "6";
                    break;
                case 6:
                    selectedMonth = "7";
                    break;
                case 7:
                    selectedMonth = "8";
                    break;
                case 8:
                    selectedMonth = "9";
                    break;
                case 9:
                    selectedMonth = "10";
                    break;
                case 10:
                    selectedMonth = "11";
                    break;
                case 11:
                    selectedMonth = "12";
                    break;
            }
            TypesByMonth(selectedMonth);
        }
        private void HandleUserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected user
            for (int i = 0; i < User.Users.Count; i++)
            {
                if (userNameComboBox.SelectedIndex == i) // Get the index for the user
                {
                    string userName = User.Users[i].UserFirstName;
                    User.UserSchedule($"{userName}");
                }
            }

            ReportWindowUserSchedule reportWindowUserSchedule = new ReportWindowUserSchedule();
            reportWindowUserSchedule.Show();
            Close();
        }
        private void HandleCustomerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
            for (int i = 0; i < Customer.Customers.Count; i++)
            {
                if (customerNameComboBox.SelectedIndex == i) // Gets the selected index for the customer
                {
                    string customerName = Customer.Customers[i].CustomerName;
                    AppointmentsPerCustomer(customerName);
                }
            }
        }
    }
}
