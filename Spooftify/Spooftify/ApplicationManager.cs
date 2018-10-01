using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Spooftify
{
    public class ApplicationManager
    {
        public static ApplicationManager instance;

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

        public void SignIn()
        {
            mainPage = new SpooftifyMain();
            mainPage.Show();
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
                AccountManager.instance.SaveAccount();
                Application.Current.Shutdown();
            }
            return explicitShutdown;
        }
    }
}
