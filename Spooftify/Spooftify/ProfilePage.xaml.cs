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
using System.Net;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Net.Http;

namespace Spooftify
{
    /// <summary>
    /// Interaction logic for ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            UsernameLabel.Content = AccountManager.instance.Acct.Username;
            NameLabel.Content = AccountManager.instance.Acct.Name;
            EmailLabel.Content = AccountManager.instance.Acct.Email;
            BirthdayLabel.Content = AccountManager.instance.Acct.Birthday;
            FetchImage(new Uri(AccountManager.instance.Acct.AvatarURI));
        }

        private void FetchImage(Uri uri)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uri;
<<<<<<< HEAD
            image.DownloadFailed += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine("Avatar Image download failed: " + AccountManager.instance.Acct.AvatarURI);
            };
            image.DownloadCompleted += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine("Downloading Complete!");
                image.Freeze();
                context.Post(_ => AvatarImage.Source = image, null);
            };
            
=======
>>>>>>> 95fab05f9696def0622745c562ba8487a95b9054
            image.EndInit();
            AvatarImage.Source = image;
        }
    }
}
