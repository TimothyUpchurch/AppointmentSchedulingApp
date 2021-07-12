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
    /// Interaction logic for AddCustomerWindow.xaml
    /// </summary>
    public partial class AddCustomerWindow : Window
    {
        public AddCustomerWindow()
        {
            InitializeComponent();

            // set the datacontext and itemsource of the citycombobox to Cities list
            DataContext = this;
            cityComboBox.ItemsSource = City.Cities;
        }
        
        private void DraggableWindow(object sender, MouseButtonEventArgs e)
        {
            // allows the user to drag the window across the screen
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void ExitAddCustomerWindow(object sender, MouseButtonEventArgs e)
        {            
            // Exits the AddCustomer window and opens up the parent element customer window
            CustomerWindow customerWindow = new CustomerWindow();
            customerWindow.Show();
            Close();
        }
        private void HandleAddCustomer(object sender, RoutedEventArgs e)
        {
            // assign user input to variables
            string name = customerNameText.Text;
            string address = customerAddressText.Text;
            string phone = customerPhoneText.Text;
            string customersCity = cityComboBox.Text;
            DateTime createDate = DateTime.UtcNow;
            DateTime lastUpdate = DateTime.UtcNow;
            int addressId = 0;
            int cityId = 0;
            string mainUser = User.MainUser.UserFirstName;

            // Validation checks
            if(!CheckAllFieldsOccupied(name, address, phone, customersCity)) // all fields must be occupied
            {
                MessageBox.Show("Please Enter A Value In Each Field.");
            }
            else if (name.Any(char.IsDigit)) // checks if name contains any digits
            {
                MessageBox.Show("Name Should Not Contain Any Numbers.");
            }
            else if (phone.Any(char.IsLetter)) // checks if the phone number contains any letters
            {
                MessageBox.Show("Phone Number Should Only Contain Digits.");
            }
            else if (CheckCustomerExists(name, address, customersCity, phone)) // Checks if the customer has been previously submitted by the user to avoid duplicates
            {
                MessageBox.Show("Customer Already Exists.");
            }
            else { // If all validation passes
                // Loop through each city to match the selected city to one stored in the Cities list. Once match is found set cityId equal to the matched city objects Id.
                foreach (City city in City.Cities)
                {
                    if(customersCity == city.CityName)
                    {
                        cityId = city.CityID;
                    }
                }

                // creating database connection string as well as Data table.
                string connectionString = ConfigurationManager.ConnectionStrings["MySqlKey"].ConnectionString;
                MySqlConnection connection = new MySqlConnection(connectionString);
                DataTable dt = new DataTable();

                // Using lambdas here to encapsulate blocks of code that will not be used anywhere else in the form. 
                // Also using async/await to ensure the data is properly inserted into the DB before continuing with adding the customer to the DB since adding a customer requires a foreign key(addressId)
                Action addAddress = async () =>
                {
                    // Add address using the information in the form fields
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO address (address, address2, cityId, postalCode, phone, createDate, createdBy, lastUpdate, lastUpdateBy) VALUES(@address, '', @cityId, 62258, @phone, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)", connection);
                    cmd.Parameters.AddWithValue("@address", address);
                    cmd.Parameters.AddWithValue("@cityId", cityId);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@createDate", createDate);
                    cmd.Parameters.AddWithValue("@createdBy", mainUser);
                    cmd.Parameters.AddWithValue("@lastUpdate", lastUpdate);
                    cmd.Parameters.AddWithValue("@lastUpdateBy", mainUser);

                    // wrapping connection in try catch. If any exception is thrown connection will still close correctly.
                    try
                    {
                        connection.Open();
                        dt.Load((await cmd.ExecuteReaderAsync()));
                    }
                    catch(Exception connectionFailed)
                    {
                        MessageBox.Show(connectionFailed.Message);
                    }
                    finally
                    {                     
                        connection.Close();
                    }
                };          
                Action locateLastAddressId = async () =>
                {
                    // Command to select the addressId that contains a matching address and phone number of last added address. addressId is needed since it is a foreign key in the customer table.
                    MySqlCommand cmd = new MySqlCommand("SELECT * from address WHERE address = @address and phone = @phone", connection);
                    cmd.Parameters.AddWithValue("@address", address);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@createDate", createDate);
                    dt = new DataTable();

                    // wrapping connection in try catch. If any exception is thrown connection will still close correctly.
                    try
                    {
                        connection.Open();
                        dt.Load(await cmd.ExecuteReaderAsync());

                        if (dt.Rows.Count > 0) addressId = Int32.Parse(dt.Rows[0]["addressId"].ToString());
                    }
                    catch (Exception connectionFailed)
                    {
                        MessageBox.Show(connectionFailed.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                };
                Action addCustomer = async () =>
                {
                    // Command to insert customer into DB using fields from the AddCustomerWindow Form.
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO customer (customerName, addressId, active, createDate, createdBy, lastUpdate, lastUpdateBy) VALUES(@name, @addressId, 1, @createDate, @createdBy, @lastUpdate, @lastUpdateBy)", connection);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@addressId", addressId);
                    cmd.Parameters.AddWithValue("@createDate", createDate);
                    cmd.Parameters.AddWithValue("@createdBy", mainUser);
                    cmd.Parameters.AddWithValue("@lastUpdate", lastUpdate);
                    cmd.Parameters.AddWithValue("@lastUpdateBy", mainUser);

                    // wrapping connection in try catch. If any exception is thrown connection will still close correctly.
                    try
                    {
                        connection.Open();
                        dt.Load((await cmd.ExecuteReaderAsync()));
                        MessageBox.Show("Customer Has Been Added.");

                        // Close window
                        CustomerWindow customerWindow = new CustomerWindow();
                        customerWindow.Show();
                        Close();
                    }
                    catch (Exception connectionFailed)
                    {
                        MessageBox.Show(connectionFailed.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }

                    // after customer successfully added reset all form fields.
                    customerNameText.Text = "";
                    customerAddressText.Text = "";
                    customerPhoneText.Text = "";
                    cityComboBox.Text = "";
                };

                addAddress();
                locateLastAddressId();
                addCustomer();
            }
        }
        private bool CheckAllFieldsOccupied(string name, string address, string phone, string customersCity)
        {
            if (name.Length != 0 && address.Length != 0 && phone.Length != 0 && customersCity.Length != 0) // If the length of each field does not equal zero than all fields are occupied.
            {
                return true;
            }
            return false;
        }
        private bool CheckCustomerExists(string name, string address, string customersCity, string phone)
        {
            // Loop through each customer in the customers list and determine if the customer has already been added.
            foreach (Customer customer in Customer.Customers)
            {
                if (customer.CustomerName == name && customer.Address == address && customer.City == customersCity && customer.Phone == phone) // Match has been found
                {
                    return true;
                }
            }
            return false; // Customer does not exist
        }
    }
}
