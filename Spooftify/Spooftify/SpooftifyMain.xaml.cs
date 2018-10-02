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
using WpfApp1;
namespace Spooftify
{
    /// <summary>
    /// Interaction logic for Spooftify.xaml
    /// </summary>
    public partial class SpooftifyMain : Window
    {
        //public Playlist CurrPlaylist { get => currPlaylist; set => currPlaylist = value; }

        //private Playlist currPlaylist;
        private PlaylistsPage playlistsPage;
        private PlayPage playPage;
        private ProfilePage profilePage;
        private SearchPage searchPage;

        public SpooftifyMain()
        {
            InitializeComponent();
            PlaylistButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyHomeActive.png"));
            playlistsPage = new PlaylistsPage();
            playPage = new PlayPage();
            profilePage = new ProfilePage();
            searchPage = new SearchPage();
            PageFrame.Content = playlistsPage;
            //currPlaylist = AccountManager.instance.CurrentPlaylist;
            //PlayPageButton.IsEnabled = AccountManager.instance.CurrentPlaylist != null; // color
            // save the pages, start on home page, bind play page enabled to currplaylist is not null for accountmanager
            // set frame = home, disable playlist button
            // children can reference this page's nav bar
        }

        private void PlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPlaylistsPage();
        }

        public void LoadPlaylistsPage()
        {
            PageFrame.Content = playlistsPage;
            SetNavImagesDefault();
            PlaylistButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyHomeActive.png"));
            ResetPages();
        }

        // bind button enabled to currplaylist

        private void PlayPageButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPlayPage();
        }

        public void LoadPlayPage()
        {
            PageFrame.Content = playPage;
            SetNavImagesDefault();
            PlayPageButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyPlayActive.png"));
            ResetPages();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSearchPage();
        }

        public void LoadSearchPage()
        {
            PageFrame.Content = searchPage;
            SetNavImagesDefault();
            SearchButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifySearchActive.png"));
            ResetPages();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = profilePage;
            SetNavImagesDefault();
            ProfileButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyProfileActive.png"));
            ResetPages();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {

            
            SocketClientOut.logout();
            AccountManager.instance.SaveAccount();
            ApplicationManager.instance.Logout();
        }

        private void SetNavImagesDefault()
        {
            PlaylistButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyHomeDefault.png"));
            PlayPageButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyPlayDefault.png"));
            SearchButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifySearchDefault.png"));
            ProfileButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyProfileDefault.png"));
        }

        private void ResetPages()
        {
            playlistsPage.Reset();
            playPage.Reset();
            searchPage.Reset();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!ApplicationManager.instance.explicitShutdown)
            {
                e.Cancel = !ApplicationManager.instance.ConfirmExit();
            }
        }
    }
}
