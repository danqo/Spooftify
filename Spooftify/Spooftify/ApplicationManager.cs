using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using WpfApp1;

namespace Spooftify
{
    public class ApplicationManager
    {
        public static ApplicationManager instance;

        public bool connectionError = false;
        public bool explicitShutdown = false;
        public Login LoginPage { get => loginPage; set => loginPage = value; }
        public SpooftifyMain MainPage { get => mainPage; set => mainPage = value; }

        private Login loginPage;
        private SpooftifyMain mainPage;

        public ApplicationManager()
        {
            instance = this;
        }

        public bool IsValidSignIn(String username, String password)
        {
            string accountsJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserJson\\UsernamesPasswords.json"));
            LoginCollection userpass = JsonConvert.DeserializeObject<LoginCollection>(accountsJson);
            foreach(LoginCollection.LoginInfo entry in userpass.LoginList)
            {
                if(entry.Username == username && entry.Password == password)
                {
                    return true;
                }
            }
            return false;
        }

        public bool SignIn(string username, string password)
        {
            connectionError = false;
            //SocketClientOut.accountEstablish();
            SocketClientOut.connectionEstablish();
            SocketClientOut.privatePort();
            SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("login"));
            SocketClientOut.sendIdAndPassword(Encoding.ASCII.GetBytes(username), Encoding.ASCII.GetBytes(password));
            var access = SocketClientOut.receiveAccess();
            if (Encoding.ASCII.GetString(access) == "granted")
            {
                var st = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                Account x = JsonConvert.DeserializeObject<Account>(st);
                AccountManager.instance.LoadAccount(x);
                st = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                AccountManager.instance.AllSongs = JsonConvert.DeserializeObject<Playlist>(st);
                loginPage.Reset();
                loginPage.Hide();
                mainPage = new SpooftifyMain();
                mainPage.Show();
                return true;
            }
            else if (Encoding.ASCII.GetString(access) == "denied")
            {
                return false;
            }
            else if (Encoding.ASCII.GetString(access) == "ServerTimeOut")
            {
                connectionError = true;
                SocketClientOut.client.Close();
                return false;
            }
            return false;
        }

        public void Logout()
        {
            AccountManager.instance.SaveAccount();
            AccountManager.instance.Clear();
            explicitShutdown = true;
            mainPage.Close();
            explicitShutdown = false;
            mainPage = null;
            loginPage.Show();
        }

        public bool ConfirmExit()
        {
            explicitShutdown = MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
            if (explicitShutdown)
            {
                //AccountManager.instance.SaveAccount();
                if(AccountManager.instance.Acct != null)
                {
                    SocketClientOut.logout();
                }
                Application.Current.Shutdown();
            }
            return explicitShutdown;
        }
    }
}
