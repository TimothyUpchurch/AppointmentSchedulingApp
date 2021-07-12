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
    /// Interaction logic for CustomerWindow.xaml
    /// </summary>
    public partial class CustomerWindow : Window
    {
        public CustomerWindow()
        {
            InitializeComponent();
            LoadCustomers();
            DataContext = Customer.Customers;
        }
        // DB init
        MySqlConnection connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySqlKey"].ConnectionString);
        MySqlCommand cmd;
        MySqlDataAdapter da;
        DataTable dt;

        private void LoadCustomers()
        {
            // Select customers - grab the address and city - join address on addressId and city on cityId
            da = new MySqlDataAdapter("SELECT customerId, customerName, address.address AS address, address.phone AS phone, city.city AS city, address.addressId AS addressId FROM customer JOIN address ON customer.addressId = address.addressId JOIN city on address.cityId = city.cityId", connection);
            dt = new DataTable();

            // Clear customer list to ensure no duplicates
            Customer.Customers.Clear();

            GetCustomers(da, dt);
            PushCustomersToList(dt);

        }
        private async void GetCustomers(MySqlDataAdapter da, DataTable dt)
        {
            // Async/Await to ensure data is retrieved before continuing with PushCustomersToList
            try
            {
                connection.Open();
                await da.FillAsync(dt);
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
        private void PushCustomersToList(DataTable dt)
        {
            // Add customer to customer list for each row returned
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Customer.Customers.Add(new Customer(Int32.Parse(dt.Rows[i]["customerId"].ToString()), dt.Rows[i]["customerName"].ToString(), dt.Rows[i]["address"].ToString(), dt.Rows[i]["phone"].ToString(), dt.Rows[i]["city"].ToString(), Int32.Parse(dt.Rows[i]["addressId"].ToString())));
            }
        }
        private void ExitToDashboard(object sender, MouseButtonEventArgs e)
        {
            // Return back to the main dashboard and close the customer window
            Dashboard dashboard = new Dashboard();
            dashboard.Show();
            Close();
        }
        private void DraggableWindow(object sender, MouseButtonEventArgs e)
        {
            // Allows the user to move the window when left clicking on the window
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void AddCustomer(object sender, RoutedEventArgs e)
        {
            // Open up AddCustomerWindow
            AddCustomerWindow addCustomerWindow = new AddCustomerWindow();
            addCustomerWindow.Show();
            Close();
        }
        private void UpdateCustomer(object sender, RoutedEventArgs e)
        {
            // Get the users selected row from the datagrid
            // Cast the selected row to a customer object to access customer properties
            Customer customer = (Customer)customerGridView.SelectedItem;

            // Check if a row is selected before passing the values of the row to variables. Otherwise a NullReferenceException will occur.
            if (customerGridView.SelectedItem != null)
            {
                try
                {
                    int indexToUpdate = customer.CustomerID;
                    string name = customer.CustomerName;
                    string address = customer.Address;
                    string phone = customer.Phone;
                    string city = customer.City;
                    int addressId = customer.AddressID;

                    // If selected item pass fields to UpdatecustomerWindow constructor. 

                    // Open UpdateCustomerWindow and pass the information needed to update a customer
                    UpdateCustomerWindow updateCustomerWindow = new UpdateCustomerWindow(indexToUpdate, name, address, phone, city, addressId);
                    updateCustomerWindow.Show();
                    Close();
                }
                catch (NullReferenceException ex)
                {
                    MessageBox.Show(ex.Message + ex.StackTrace);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + ex.StackTrace);
                }
            }
            else // if no row is selected and the update customer button is clicked
            {
                MessageBox.Show("Please Select A User To Be Updated.");
            }
        }
        private void DeleteCustomer(object sender, RoutedEventArgs e)
        {
            // Get the users selected row from the datagrid
            // Cast the selected row to a customer object to access customer properties
            Customer customer = (Customer)customerGridView.SelectedItem;

            // Check if a row is selected before passing the values of the row to variables. Otherwise a NullReferenceException will occur
            if (customerGridView.SelectedItem != null)
            {
                try
                {
                    // the customer index that will be used when deleteing a customer from the database
                    int indexToDelete = customer.CustomerID;
                    // the address index used to delete an address associated with a customer being deleted.
                    int addressToDelete = customer.AddressID;

                    // Ask user if they are sure if they want to delete
                    var Result = MessageBox.Show("Are You Sure You Want To Delete This Item?", "Delete Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (Result == MessageBoxResult.Yes)
                    {
                        // Delete customer from DB
                        // Set Foreign key checks to 0 then delete customer. Appointments associated with customer will also be deleted. Set foreign key checks back to 1.
                        cmd = new MySqlCommand("SET FOREIGN_KEY_CHECKS=0; DELETE FROM customer WHERE customerId = @customerId; SET FOREIGN_KEY_CHECKS=1;", connection);
                        cmd.Parameters.AddWithValue("@customerId", indexToDelete);
                        try
                        {
                            connection.Open();                         
                            dt = new DataTable();
                            dt.Load(cmd.ExecuteReader());  
                            MessageBox.Show("Customer Has Been Deleted.");
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
                        // If customer is deleted reload customers into the datagrid.
                        LoadCustomers();
                        // delete any appointments associated with customer as well as the associated address
                        DeleteCustomerAppointments(indexToDelete);
                        DeleteCustomerAddress(addressToDelete);
                    }
                    else
                    {
                        // Reset Selected Item: Otherwise when the user selectes delete again it will use the last selected item even though the datagrid UI gives to indication that it is still registered as selected.
                        customerGridView.SelectedItem = null;
                    }
                }
                // If statement covers exception though by not running the code unless a row has been selected.
                catch(NullReferenceException ex)
                {
                    MessageBox.Show(ex.Message + ex.StackTrace);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message + ex.StackTrace);
                }
            }
            else // User has not selected a row before clicking the delete button
            {
                MessageBox.Show("Please Select A User To Be Deleted.");
            }
        }
        private void DeleteCustomerAppointments(int customerId)
        {
            // Delete appointments associated with the customer being deleted.
            cmd = new MySqlCommand("DELETE FROM appointment WHERE customerId = @customerId", connection);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            try
            {
                connection.Open();
                dt = new DataTable();
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
        private void DeleteCustomerAddress(int addressId)
        {
            // Delete address associated with the customer being deleted.
            cmd = new MySqlCommand("DELETE FROM address WHERE addressId = @addressId", connection);
            cmd.Parameters.AddWithValue("@addressId", addressId);
            try
            {
                connection.Open();
                dt = new DataTable();
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
}
