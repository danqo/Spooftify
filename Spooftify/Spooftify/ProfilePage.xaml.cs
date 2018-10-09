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
        /// <summary>
        /// constructor
        /// </summary>
        public ProfilePage()
        {
            InitializeComponent();
            UsernameLabel.Content = AccountManager.instance.Acct.Username;
            NameLabel.Content = AccountManager.instance.Acct.Name;
            EmailLabel.Content = AccountManager.instance.Acct.Email;
            BirthdayLabel.Content = AccountManager.instance.Acct.Birthday;
            FetchImage(new Uri(AccountManager.instance.Acct.AvatarURI));
        }

        /// <summary>
        /// Downloads the avatar image loaded from the account information from the server
        /// Updates the image with the downloaded image
        /// </summary>
        /// <param name="uri">uri used to download the image</param>
        private void FetchImage(Uri uri)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uri;
            image.EndInit();
            AvatarImage.Source = image;
        }
    }
}
