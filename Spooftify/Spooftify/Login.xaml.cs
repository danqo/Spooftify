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
        public static Playlist allSongs;
        public Login()
        {
            InitializeComponent();
            applicationManager = new ApplicationManager();
            ApplicationManager.instance.LoginPage = this;
            accountManager = new AccountManager();
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (user.Text == "" || pass.Password == "")
            {
                MessageBox.Show("id or password can't be empty");

            }
            else
            {
                //SocketClientOut.accountEstablish();
                SocketClientOut.connectionEstablish();
                SocketClientOut.privatePort();
                SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("login"));
                SocketClientOut.sendIdAndPassword(Encoding.ASCII.GetBytes(user.Text), Encoding.ASCII.GetBytes(pass.Password));
                var access = SocketClientOut.receiveAccess();
                if (Encoding.ASCII.GetString(access) == "granted")
                {

                    
                    var st = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    Account x = JsonConvert.DeserializeObject<Account>(st);
                    AccountManager.instance.LoadAccount(x);
                    st = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    allSongs = JsonConvert.DeserializeObject<Playlist>(st);
                    Reset();
                    this.Hide();
                    //b.Show();
                    ApplicationManager.instance.SignIn();
                }
                else if (Encoding.ASCII.GetString(access) == "denied")
                {

                    InvalidLogin.Visibility = Visibility.Visible;
                }
            }
        }

        private void SignIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (user.Text == "" || pass.Password == "")
                {
                    MessageBox.Show("id or password can't be empty");

                }
                else
                {
                    //SocketClientOut.accountEstablish();
                    SocketClientOut.connectionEstablish();
                    SocketClientOut.privatePort();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("login"));
                    SocketClientOut.sendIdAndPassword(Encoding.ASCII.GetBytes(user.Text), Encoding.ASCII.GetBytes(pass.Password));
                    var access = SocketClientOut.receiveAccess();
                    if (Encoding.ASCII.GetString(access) == "granted")
                    {

                        var st = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                        Account x = JsonConvert.DeserializeObject<Account>(st);
                        AccountManager.instance.LoadAccount(x);
                        st = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                        allSongs = JsonConvert.DeserializeObject<Playlist>(st);
                        Reset();
                        this.Hide();
                        //b.Show();
                        ApplicationManager.instance.SignIn();
                    }
                    else if (Encoding.ASCII.GetString(access) == "denied")
                    {

                        InvalidLogin.Visibility = Visibility.Visible;
                        InvalidLogin.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void Reset()
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
