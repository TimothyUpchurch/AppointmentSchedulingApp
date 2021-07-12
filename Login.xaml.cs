using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppointmentsC969_TimothyUpchurch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Declaring local variables used throughout the login form
        string nameLabelText;
        string passwordLabelText;
        string loginButtonText;
        string emptyNamePassword;
        string noMatchNamePassword;

        private void DraggableWindow(object sender, MouseButtonEventArgs e)
        {
            // allows the user to drag the login window around the screen when holding left click on the window.
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void ExitLoginScreen(object sender, MouseButtonEventArgs e)
        {
            // No prior screen to the loging. Shutdown when clicked.
            System.Windows.Application.Current.Shutdown();
        }
        private void HideNameLabel(object sender, RoutedEventArgs e)
        {
            // Set the label content to empty to give room for the user input in the form field.
            nameLabel.Content = "";
        }
        private void ShowNameLabel(object sender, RoutedEventArgs e)
        {
            // check if the name label text box is empty. If empty, and the text box has lost focused show the label.
            if (nameTextBox.Text == "")
            {
                nameLabel.Content = nameLabelText;
            }
        }
        private void HidePasswordLabel(object sender, RoutedEventArgs e)
        {
            // Set the label content to empty to give room for the user input in the form field.
            passwordLabel.Content = "";
        }
        private void ShowPasswordLabel(object sender, RoutedEventArgs e)
        {
            // check if the password box is empty. If empty, and the password box has lost focused show the label.
            if (passwordTextBox.Password.Length == 0)
            {
                passwordLabel.Content = passwordLabelText;
            }
        }
        private void CheckUserCulture(object sender, RoutedEventArgs e)
        {
            // Runs when the window is loaded. Checks for US/Japan.
            // When changing Regional Format in Windows Region Settings to Japanese the login screen will be displayed in Japanese. As well as any messages related to the login screen.
            var cultureInfo = CultureInfo.CurrentCulture;
            Localize(cultureInfo.TwoLetterISOLanguageName);
        }    
        private void HandleLogin(object sender, RoutedEventArgs e)
        {
            // Retrieve data from login form
            string name = nameTextBox.Text;
            string password = passwordTextBox.Password;

            // Db connection
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlKey"].ConnectionString;
            MySqlConnection connection = new MySqlConnection(connectionString);
            // String to query DB for user with matching username and password
            MySqlCommand cmd = new MySqlCommand("Select * From user where userName = @name and password = @password", connection);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@password", password);
            DataTable dt = new DataTable();

            // if both fields are occupied attempt to select the user from the DB
            CheckIfUserMatch(dt, cmd, connection, name, password);

            // if the DB returns a result the login has been successful and will procede to the Dashboard
            CheckSuccessfulLogin(dt, name, password);
        }
        private void Localize(string twoISOName)
        {
            // Switch the two letter ISO name obtained from Culture Info and set global variables accordingly.
            switch (twoISOName)
            {
                // if the detected language is English
                case "en":
                    nameLabelText = "Name";
                    passwordLabelText = "Password";
                    loginButtonText = "Login";
                    emptyNamePassword = "Please enter a user name and password";
                    noMatchNamePassword = "User name and password do not match";
                    break;
                // if the detected language is Japanase
                case "ja":
                    nameLabelText = "名前";
                    passwordLabelText = "パスワード";
                    loginButtonText = "ログイン";
                    emptyNamePassword = "ユーザー名とパスワードを入力してください";
                    noMatchNamePassword = "ユーザー名とパスワードが一致しません";
                    break;
                default:
                    nameLabelText = "Name";
                    passwordLabelText = "Password";
                    loginButtonText = "Login";
                    emptyNamePassword = "Please enter a user name and password";
                    noMatchNamePassword = "User name and password do not match";
                    break;
            }
            // Assign the UI element values based off of which language was detected.
            nameLabel.Content = nameLabelText;
            passwordLabel.Content = passwordLabelText;
            loginButton.Content = loginButtonText;
        }
        private void CheckIfUserMatch(DataTable dt, MySqlCommand cmd, MySqlConnection connection, string name, string password)
        {
            // Check if both fields have a value
            if (name.Length != 0 && password.Length != 0)
            {
                try
                {
                    connection.Open();
                    dt.Load(cmd.ExecuteReader());
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
            else // if an empty field is detected show error message
            {
                MessageBox.Show(emptyNamePassword);
            }
        }
        private void CheckSuccessfulLogin(DataTable dt, string name, string password)
        {
            // If a row exists the login has been successfull / Other wise username and password did not match.
            if (dt.Rows.Count > 0)
            {
                // Create a main user to keep track of user related content. Such as tracking log ins.
                User.MainUser = new User(Int32.Parse(dt.Rows[0]["userId"].ToString()), dt.Rows[0]["userName"].ToString());
                User.LogUserLogin(dt.Rows[0]["userName"].ToString());

                // If the login was successful and a user was loaded proceed to the main dashboard.
                Dashboard dashboard = new Dashboard();
                dashboard.Show();
                Close();
            }
            // If the row count is 0 and username and password were both filled in when the form was submitted the username and password did not match any existing user in the database.
            else if (dt.Rows.Count == 0 && (name.Length > 0 && password.Length > 0))
            {
                MessageBox.Show(noMatchNamePassword);
            }
        }
    }
}
