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
using System.IO;
using Newtonsoft.Json;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        
        
        public static LoginScreen LoginWindow;
        public static UserCollection everyUser;
        public static Playlist allSongs;
        public Boolean closingFlag = false;
        public LoginScreen()
        {
            InitializeComponent();
            
            //string st = File.ReadAllText(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\Users.json"));
            //everyUser = JsonConvert.DeserializeObject<UserCollection>(st);
            //string allSongSt = File.ReadAllText(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\AllSongs.json"));
            //allSongs = JsonConvert.DeserializeObject<Playlist>(allSongSt);
            LoginWindow = this;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
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
                    User x = JsonConvert.DeserializeObject<User>(st);
                    st = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    allSongs = JsonConvert.DeserializeObject<Playlist>(st);
                    AfterLogin b = new AfterLogin(x);
                    Reset();
                    this.Hide();
                    b.Show();
                }
                else if (Encoding.ASCII.GetString(access) == "denied")
                {

                    InvalidLogin.Visibility = Visibility.Visible;
                }
            }
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
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
                        User x = JsonConvert.DeserializeObject<User>(st);
                        st = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                        allSongs = JsonConvert.DeserializeObject<Playlist>(st);
                        AfterLogin b = new AfterLogin(x);
                        Reset();
                        this.Hide();
                        b.Show();
                    }
                    else if (Encoding.ASCII.GetString(access) == "denied")
                    {

                        InvalidLogin.Visibility = Visibility.Visible;
                    }
                }
                /*
                var msg = "Incorrect Username or Password!";
                int counter = 0;
                foreach (var x in everyUser.Users)
                {
                    
                    if (user.Text == x.mUsername && pass.Password == x.mPassword)
                    {
                        AfterLogin b = new AfterLogin(x);
                        counter++;
                        Reset();
                        this.Hide();
                        b.Show();
                        break;
                    }

                }

                if (counter == 0)
                {
                    InvalidLogin.Visibility = Visibility.Visible;
                    //MessageBox.Show(msg);
                }
                */
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
            if (!closingFlag)
            {
                ConfirmExit();
            }
            if (closingFlag)
            {
                saveUsers();
                App.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void ConfirmExit()
        {
            MessageBoxResult mbResult = MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if(mbResult == MessageBoxResult.Yes)
            {
                closingFlag = true;
            }
        }

        private void saveUsers()
        {
            File.WriteAllText(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\Users.json"), JsonConvert.SerializeObject(LoginScreen.everyUser));
        }
    }
}