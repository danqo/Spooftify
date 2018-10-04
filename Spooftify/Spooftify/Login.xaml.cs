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
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        ApplicationManager applicationManager;
        AccountManager accountManager;

        public Login()
        {
            InitializeComponent();
            applicationManager = new ApplicationManager();
            ApplicationManager.instance.LoginPage = this;
            accountManager = new AccountManager();
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if ((user.Text == "" || pass.Password == "") || !ApplicationManager.instance.SignIn(user.Text, pass.Password))
            {
                InvalidLogin.Visibility = Visibility.Visible;
            }
        }

        private void SignIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((user.Text == "" || pass.Password == "") || !ApplicationManager.instance.SignIn(user.Text, pass.Password))
                {
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
