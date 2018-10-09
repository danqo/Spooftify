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
    /// Container for the application with navigation buttons
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

        /// <summary>
        /// constructor that initializes all the pages for the application
        /// </summary>
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

        /// <summary>
        /// navigation procedure to the playlist or home page
        /// correctly highlights the current page
        /// </summary>
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

        /// <summary>
        /// navigation procedure to the playpage
        /// correctly highlights the current page
        /// </summary>
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

        /// <summary>
        /// navigation procedure to the search page
        /// correctly highlights the current page
        /// </summary>
        public void LoadSearchPage()
        {
            PageFrame.Content = searchPage;
            SetNavImagesDefault();
            SearchButtonImage.Source = SearchActive;
            ResetPages();
        }

        /// <summary>
        /// navigation procedure to the profile page
        /// correctly highlights the current page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = profilePage;
            SetNavImagesDefault();
            ProfileButtonImage.Source = ProfileActive;
            ResetPages();
        }

        /// <summary>
        /// send account information to the server to be overwritten
        /// return the user to the login page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            SocketClientOut.logout();
            ApplicationManager.instance.Logout();
        }

        /// <summary>
        /// resets the navigation buttons to their default state
        /// </summary>
        private void SetNavImagesDefault()
        {
            TogglePlayPageButton();
            PlaylistButtonImage.Source = HomeDefault;
            SearchButtonImage.Source = SearchDefault;
            ProfileButtonImage.Source = ProfileDefault;
        }

        /// <summary>
        /// Disables the play page button if current playlist is not selected
        /// May be triggered if the current playlist selected is removed by the user
        /// </summary>
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
