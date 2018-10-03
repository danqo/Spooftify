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
        private BitmapImage HomeActive = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyHomeActive.png"));
        private BitmapImage HomeDefault = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyHomeDefault.png"));
        private BitmapImage PlayPageActive = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyPlayActive.png"));
        private BitmapImage PlayPageDefault = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyPlayDefault.png"));
        private BitmapImage PlayPageDisabled = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyPlayDisabled.png"));
        private BitmapImage SearchActive = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifySearchActive.png"));
        private BitmapImage SearchDefault = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifySearchDefault.png"));
        private BitmapImage ProfileActive = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyProfileActive.png"));
        private BitmapImage ProfileDefault = new BitmapImage(new Uri("pack://application:,,,/Images/SpooftifyProfileDefault.png"));

        private PlaylistsPage playlistsPage;
        private PlayPage playPage;
        private ProfilePage profilePage;
        private SearchPage searchPage;

        public SpooftifyMain()
        {
            InitializeComponent();
            playlistsPage = new PlaylistsPage();
            playPage = new PlayPage();
            profilePage = new ProfilePage();
            searchPage = new SearchPage();
            PageFrame.Content = playlistsPage;
        }

        private void PlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPlaylistsPage();
        }

        public void LoadPlaylistsPage()
        {
            PageFrame.Content = playlistsPage;
            SetNavImagesDefault();
            PlaylistButtonImage.Source = HomeActive;
            ResetPages();
        }

        private void PlayPageButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPlayPage();
        }

        public void LoadPlayPage()
        {
            PageFrame.Content = playPage;
            SetNavImagesDefault();
            PlayPageButtonImage.Source = PlayPageActive;
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
            SearchButtonImage.Source = SearchActive;
            ResetPages();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = profilePage;
            SetNavImagesDefault();
            ProfileButtonImage.Source = ProfileActive;
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
            TogglePlayPageButton();
            PlaylistButtonImage.Source = HomeDefault;
            SearchButtonImage.Source = SearchDefault;
            ProfileButtonImage.Source = ProfileDefault;
        }

        // call this if a playlist is deleted
        public void TogglePlayPageButton()
        {
            PlayPageButton.IsEnabled = AccountManager.instance.CurrentPlaylist != null;
            PlayPageButtonImage.Source = PlayPageButton.IsEnabled ? PlayPageDefault : PlayPageDisabled;
        }

        public void ResetPages()
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
