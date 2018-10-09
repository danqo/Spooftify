using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Spooftify
{
    /// <summary>
    /// Interaction logic for PlaylistsPage.xaml
    /// </summary>
    public partial class PlaylistsPage : Page
    {
        private const string ADDED_PLAYLIST_MSG = "Playlist added!";
        private const string DUPLICATE_PLAYLIST_MSG = "Playlist with that name already exists!";
        private const string EMPTY_NAME_MSG = "Playlist name cannot be empty!";
        private const string REMOVED_PLAYLIST_MSG = "Playlist removed!";

        /// <summary>
        /// constructor
        /// </summary>
        public PlaylistsPage()
        {
            InitializeComponent();
            PlaylistListBox.ItemsSource = AccountManager.instance.Acct.Playlists;
        }

        /// <summary>
        /// Creates another playlist for the user and displays it on the listbox
        /// IF: the inputted playlistname is not empty and not a duplicate playlist name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(AddPlaylistTextBox.Text))
            {
                AddPlaylistMsg.Content = EMPTY_NAME_MSG;
                AddPlaylistMsg.Foreground = Brushes.Tomato;
                AddPlaylistMsg.Visibility = Visibility.Visible;
            }
            else if (AccountManager.instance.Acct.FindPlaylist(AddPlaylistTextBox.Text) == null)
            {
                AccountManager.instance.Acct.Playlists.Add(new Playlist(AddPlaylistTextBox.Text));
                RefreshPlaylists();
                AddPlaylistTextBox.Text = "";
                AddPlaylistMsg.Content = ADDED_PLAYLIST_MSG;
                AddPlaylistMsg.Foreground = Brushes.Green;
                AddPlaylistMsg.Visibility = Visibility.Visible;
            }
            else
            {
                AddPlaylistMsg.Content = DUPLICATE_PLAYLIST_MSG;
                AddPlaylistMsg.Foreground = Brushes.Tomato;
                AddPlaylistMsg.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Creates another playlist for the user and displays it on the listbox
        /// IF: the inputted playlistname is not empty and not a duplicate playlist name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddPlaylistTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(String.IsNullOrWhiteSpace(AddPlaylistTextBox.Text))
                {
                    AddPlaylistMsg.Content = EMPTY_NAME_MSG;
                    AddPlaylistMsg.Foreground = Brushes.Tomato;
                    AddPlaylistMsg.Visibility = Visibility.Visible;
                }
                else if (AccountManager.instance.Acct.FindPlaylist(AddPlaylistTextBox.Text) == null)
                {
                    AccountManager.instance.Acct.Playlists.Add(new Playlist(AddPlaylistTextBox.Text));
                    RefreshPlaylists();
                    AddPlaylistTextBox.Text = "";
                    AddPlaylistMsg.Content = ADDED_PLAYLIST_MSG;
                    AddPlaylistMsg.Foreground = Brushes.Green;
                    AddPlaylistMsg.Visibility = Visibility.Visible;
                }
                else
                {
                    AddPlaylistMsg.Content = DUPLICATE_PLAYLIST_MSG;
                    AddPlaylistMsg.Foreground = Brushes.Tomato;
                    AddPlaylistMsg.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Loads the selected playlist onto the playpage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaylistListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(TextBlock) && r.VisualHit.GetType() != typeof(Border))
            {
                PlaylistListBox.SelectedItem = null;
            }
            if (PlaylistListBox.SelectedItem != null)
            {
                AccountManager.instance.CurrentPlaylist = PlaylistListBox.SelectedItem as Playlist;
                ApplicationManager.instance.MainPage.LoadPlayPage();
            }
        }

        /// <summary>
        /// brings up the context menu that allows the user to remove a playlist from their account
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaylistListBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(TextBlock) && r.VisualHit.GetType() != typeof(Border))
            {
                PlaylistListBox.SelectedItem = null;
            }
            else
            {
                ContextMenu cm = this.FindResource("playlistContextMenu") as ContextMenu;
                cm.PlacementTarget = sender as ListBox;
                cm.IsOpen = true;
            }
        }

        /// <summary>
        /// called when the user clicks "remove" on the context menu to remove a playlist from their account
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistListBox.SelectedItem != null)
            {
                if (AccountManager.instance.CurrentPlaylist != null && (PlaylistListBox.SelectedItem as Playlist).Name.Equals(AccountManager.instance.CurrentPlaylist.Name))
                {
                    AccountManager.instance.CurrentPlaylist = null;
                }
                AccountManager.instance.Acct.Playlists.Remove(PlaylistListBox.SelectedItem as Playlist);
                RefreshPlaylists();
                ApplicationManager.instance.MainPage.TogglePlayPageButton();
                AddPlaylistMsg.Content = REMOVED_PLAYLIST_MSG;
                AddPlaylistMsg.Foreground = Brushes.Green;
                AddPlaylistMsg.Visibility = Visibility.Visible;
            }
        }

        private void AddPlaylistTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Tag != null)
            {
                textBox.Tag = (!String.IsNullOrWhiteSpace(textBox.Text)).ToString();
            }
        }

        private void RefreshPlaylists()
        {
            PlaylistListBox.SelectedItem = null;
            PlaylistListBox.Items.Refresh();
        }

        public void Reset()
        {
            AddPlaylistMsg.Visibility = Visibility.Hidden;
            AddPlaylistTextBox.Text = "";
            RefreshPlaylists();
        }
    }
}
