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
    /// Interaction logic for UpdateCustomerWindow.xaml
    /// </summary>
    public partial class UpdateCustomerWindow : Window
    {
        public UpdateCustomerWindow(int customerId, string name, string address, string phone, string city, int addressId)
        {
            InitializeComponent();

            // Set the fields to the text of the customer being updated
            customerNameText.Text = name;
            customerAddressText.Text = address;
            customerPhoneText.Text = phone;
            // Loop through cities list where city = city.name and assign cityId to cityComboBox selectedIndex
            for(int i = 0; i < City.Cities.Count; i++)
            {
                if (City.Cities[i].CityName == city)
                {
                    cityComboBox.SelectedIndex = i;
                }
            }

            // Store the previous customer information in local variables
            previousCustomerId = customerId;
            previousAddressId = addressId;
            previousCustomerName = name;
            previousCustomerAddress = address;
            previousCustomerPhone = phone;
            previousCustomerCity = city;

            // set data context and set item source to Cities list for the city combo box
            DataContext = this;
            cityComboBox.ItemsSource = City.Cities;
        }

        int previousCustomerId = 0;
        int previousAddressId = 0;
        int cityId = 0;
        string previousCustomerName;
        string previousCustomerAddress;
        string previousCustomerPhone;
        string previousCustomerCity;
       
        private void DraggableWindow(object sender, MouseButtonEventArgs e)
        {
            // allows the user to drag the window across the screen.
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void ExitUpdateCustomerWindow(object sender, MouseButtonEventArgs e)
        {
            CloseUpdateWindow();
        }
        private void HandleUpdateCustomer(object sender, RoutedEventArgs e)
        {
            // Validation before updating customer
            if (!CheckAllFieldsOccupied(customerNameText.Text, customerAddressText.Text, customerPhoneText.Text, cityComboBox.Text))
            {
                MessageBox.Show("All Fields Must Be Occupied.");
            }
            else if (customerNameText.Text.Any(char.IsDigit))
            {
                MessageBox.Show("Name Should Not Contain Any Numbers.");
            }
            else if (customerPhoneText.Text.Any(char.IsLetter))
            {
                MessageBox.Show("Phone Number Should Only Contain Digits.");
            }
            else if (!CheckChangesInFieldText(previousCustomerName, previousCustomerAddress, previousCustomerPhone, previousCustomerCity))
            {
                MessageBox.Show("No Changes Have Been Made.");
            }
            else if (CheckCustomerExists(customerNameText.Text, customerAddressText.Text, cityComboBox.Text, customerPhoneText.Text))
            {
                MessageBox.Show("Customer Already Exists.");
            }
            else
            {
                // Creating database connection string as well as Data table.
                string connectionString = ConfigurationManager.ConnectionStrings["MySqlKey"].ConnectionString;
                MySqlConnection connection = new MySqlConnection(connectionString);
                DataTable dt = new DataTable();

                // If address, city, or phone changed update address table.
                UpdateCustomerAddress(connection, dt);

                // Update customer table if customer name has been changed.
                UpdateCustomerName(connection, dt);

                // Out put message that customer has been updated if any text fields have been modified.
                // If fields have been modified updates have been made to the DB.
                if (CheckChangesInFieldText(previousCustomerName, previousCustomerAddress, previousCustomerPhone, previousCustomerCity))
                {
                    MessageBox.Show("Customer Has Been Updated Successfully.");
                    // Return to Customer Window
                    CloseUpdateWindow();
                }
            }  
        }
        private bool CheckAllFieldsOccupied(string name, string address, string phone, string city) // if all field lengths != 0 all fields are occupied.
        {
            if (name.Length != 0 && address.Length != 0 && phone.Length != 0 && city.Length != 0)
            {
                return true;
            }
            return false;
        }
        private bool CheckChangesInFieldText(string name, string address, string phone, string city)
        {
            // Compares the text of the form field to the previous stored customer information. If all fields are equal no changes have been made.
            if (name == customerNameText.Text && address == customerAddressText.Text && phone == customerPhoneText.Text && city == cityComboBox.Text)
            {
                return false;
            }
            return true;
        }
        private void UpdateCustomerAddress(MySqlConnection connection, DataTable dt)
        {
            // At least one field pertaining to the address must be different in order to update the customers address. 
            if (customerAddressText.Text != previousCustomerAddress || cityComboBox.Text != previousCustomerCity || customerPhoneText.Text != previousCustomerPhone)
            {
                // Loop through each city in City.cities and get ID of city where city name from user field matches city name in list.
                // Can replace this code by just simply adding in cityId in the SELECT statement in CustomerWindow; passing it in the constructor and assigning it to a local variable.
                foreach (City city in City.Cities)
                {
                    if (cityComboBox.Text == city.CityName)
                    {
                        cityId = city.CityID;
                    }
                }

                // Command to update user
                MySqlCommand cmd = new MySqlCommand("UPDATE address SET address = @address, phone = @phone, cityId = @cityId, lastUpdateBy = @lastUpdateBy WHERE addressId = @addressId", connection);
                cmd.Parameters.AddWithValue("@address", customerAddressText.Text);
                cmd.Parameters.AddWithValue("@phone", customerPhoneText.Text);
                cmd.Parameters.AddWithValue("@cityId", cityId);
                cmd.Parameters.AddWithValue("@addressId", previousAddressId);
                cmd.Parameters.AddWithValue("@lastUpdateBy", User.MainUser.UserFirstName);

                // wrapping connection in a try catch. If any exception is thrown the connection will still close properly.
                try
                {
                    connection.Open();
                    dt.Load((cmd.ExecuteReader()));
                }
                catch (Exception connectionFailed)
                {
                    MessageBox.Show(connectionFailed.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void UpdateCustomerName(MySqlConnection connection, DataTable dt)
        {
            if (customerNameText.Text != previousCustomerName)
            {
                // connect to DB and update where customerId = previousCustomerId;
                // Command to update customer name
                MySqlCommand cmd = new MySqlCommand("UPDATE customer SET customerName = @customerName, lastUpdateBy = @lastUpdateBy WHERE customerId = @customerId", connection);
                cmd.Parameters.AddWithValue("@customerName", customerNameText.Text);
                cmd.Parameters.AddWithValue("@customerId", previousCustomerId);
                cmd.Parameters.AddWithValue("@lastUpdateBy", User.MainUser.UserFirstName);

                try
                {
                    connection.Open();
                    dt.Load(cmd.ExecuteReader());
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
        private void CloseUpdateWindow()
        {
            // Close update window and navigate back to the Customer Window 
            CustomerWindow customerWindow = new CustomerWindow();
            customerWindow.Show();
            Close();
        }
        private bool CheckCustomerExists(string name, string address, string customersCity, string phone)
        {
            // Loop through each customer in the customers list and determine if the customer has already been added.
            foreach (Customer customer in Customer.Customers)
            {
                if (customer.CustomerName == name && customer.Address == address && customer.City == customersCity && customer.Phone == phone)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
