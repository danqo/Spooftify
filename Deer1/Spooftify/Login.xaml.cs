using System;
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
using Newtonsoft.Json;
using WpfApp1;

namespace Spooftify
{
    /// <summary>
    // Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        ApplicationManager applicationManager;
        AccountManager accountManager;
        
        private const string CONNECTION_FAILURE = "Could not connect to the host. Please try later.";
        private const string EMPTY_USER_PASS = "Username or password cannot be empty!";
        private const string INVALID_LOGIN = "Incorrect username or password!";

        /// <summary>
        /// constructor
        /// </summary>
        public Login()
        {
            InitializeComponent();
           
            applicationManager = new ApplicationManager();
            ApplicationManager.instance.LoginPage = this;
            accountManager = new AccountManager();
        }

        /// <summary>
        /// Verifies that the inputted login information is valid
        /// Loads the application if login information is valid
        /// Displays an error message otherwise
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(user.Text) || String.IsNullOrWhiteSpace(pass.Password))
            {
                InvalidLogin.Content = EMPTY_USER_PASS;
                InvalidLogin.Visibility = Visibility.Visible;
            }
            else if (!ApplicationManager.instance.SignIn(user.Text, pass.Password))
            {
                InvalidLogin.Content = ApplicationManager.instance.connectionError ? CONNECTION_FAILURE : INVALID_LOGIN;
                InvalidLogin.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Verifies that the inputted login information is valid
        /// Loads the application if login information is valid
        /// Displays an error message otherwise 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SignIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (String.IsNullOrWhiteSpace(user.Text) || String.IsNullOrWhiteSpace(pass.Password))
                {
                    InvalidLogin.Content = EMPTY_USER_PASS;
                    InvalidLogin.Visibility = Visibility.Visible;
                }
                else if (!ApplicationManager.instance.SignIn(user.Text, pass.Password))
                {
                    InvalidLogin.Content = ApplicationManager.instance.connectionError ? CONNECTION_FAILURE : INVALID_LOGIN;
                    InvalidLogin.Visibility = Visibility.Visible;
                }
            }
        }

        public void Reset()
        {
            user.Text = "";
            pass.Password = "";
            SignInButton.Focus();
            InvalidLogin.Visibility = Visibility.Hidden;
        }

        private void OnUserChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Tag != null)
            {
                textBox.Tag = (!String.IsNullOrWhiteSpace(textBox.Text)).ToString();
            }
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passBox = sender as PasswordBox;
            if (passBox.Tag != null)
            {
                passBox.Tag = (!String.IsNullOrWhiteSpace(passBox.Password)).ToString();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ApplicationManager.instance.explicitShutdown)
            {
                e.Cancel = !ApplicationManager.instance.ConfirmExit();
            }
        }
    }
}
