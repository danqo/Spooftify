using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using WpfApp1;

namespace Spooftify
{
    /// <summary>
    /// Handles signing into the application and connecting to the server
    /// Saves any changes made to the account on logging out or exiting
    /// </summary>
    public class ApplicationManager
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        public static ApplicationManager instance;

        public bool connectionError = false;
        public bool explicitShutdown = false;
        public Login LoginPage { get => loginPage; set => loginPage = value; }
        

        private Login loginPage;
        

        /// <summary>
        /// Constructor
        /// </summary>
        public ApplicationManager()
        {
            instance = this;
        }

        /// <summary>
        /// Sends the inputted usernames and passwords to the server for verification.
        /// </summary>
        /// <param name="username">inputted username</param>
        /// <param name="password">inputted password</param>
        /// <returns>true if the login information has a match, false if the login information was incorrect or if the connection timed out</returns>
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
                AllocConsole();
                
                Console.WriteLine("Deer1 Login Successfully ");
                Console.ReadLine();

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

        /// <summary>
        /// sends any changes made to the account to the server and returns the user to the login page
        /// </summary>
        public void Logout()
        {
            AccountManager.instance.Clear();
            explicitShutdown = true;
            
            explicitShutdown = false;
            
            loginPage.Show();
        }

        /// <summary>
        /// Asks the user to confirm shutdown and sends the account information to the server on shutdown
        /// </summary>
        /// <returns>true if shutdown has been confirmed, false if cancelled</returns>
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
