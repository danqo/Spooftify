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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1;
using System.Threading;
using Newtonsoft.Json;
namespace Spooftify
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        private Playlist searchQuery;

        /// <summary>
        /// constructor
        /// </summary>
        public SearchPage()
        {
            InitializeComponent();
            searchQuery = AccountManager.instance.AllSongs;
            SearchListBox.ItemsSource = searchQuery.Songs;
        }

        /// <summary>
        /// Event triggered to perform a search by keyword
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Tag != null)
            {
                textBox.Tag = (!String.IsNullOrWhiteSpace(textBox.Text)).ToString();
            }
            //FilterSearch();
        }

        /// <summary>
        /// Event triggered to filter by the specified search criteria
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //FilterSearch();
        }

        /// <summary>
        /// Filters based on keyword and search criteria and updates the listbox contents with the results
        /// </summary>
        private void FilterSearch()
        {
            if(String.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                searchQuery = AccountManager.instance.AllSongs;
            }
            else
            {
                searchQuery = new Playlist("searchQuery");
                foreach(Song s in AccountManager.instance.AllSongs.Songs)
                {
                    if(SearchByComboBox.SelectedItem.ToString().Equals("System.Windows.Controls.ComboBoxItem: Title") && s.Title.ToLower().Contains(SearchTextBox.Text.ToLower()))
                    {
                        searchQuery.addSong(s);
                    }
                    if (SearchByComboBox.SelectedItem.ToString().Equals("System.Windows.Controls.ComboBoxItem: Artist") && s.Artist.ToLower().Contains(SearchTextBox.Text.ToLower()))
                    {
                        searchQuery.addSong(s);
                    }
                    if (SearchByComboBox.SelectedItem.ToString().Equals("System.Windows.Controls.ComboBoxItem: Album") && s.Album.ToLower().Contains(SearchTextBox.Text.ToLower()))
                    {
                        searchQuery.addSong(s);
                    }
                }
            }
            if(SearchListBox != null)
            {
                SearchListBox.ItemsSource = searchQuery.Songs;
                SearchListBox.Items.Refresh();
            }
        }

        public void Reset()
        {
            AddRemoveSongMsg.Visibility = Visibility.Hidden;
            SearchTextBox.Text = "";
            searchQuery = AccountManager.instance.AllSongs;
            SearchListBox.ItemsSource = searchQuery.Songs;
            SearchListBox.Items.Refresh();
        }

        /// <summary>
        /// Brings up the context menu when right clicking on a listbox item
        /// Allows the user to add/remove a song to any of the playlists shown on the context menu
        /// A checkmark means the song exists on that playlist. Clicking on that playlist will remove it.
        /// No checkmark means the song does not exist on the playlist. Clicking on that playlist will add it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchListBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(TextBlock) && r.VisualHit.GetType() != typeof(Border))
            {
                SearchListBox.SelectedItem = null;
            }
            else if (SearchListBox.SelectedItem != null)
            {
                ContextMenu cm = new ContextMenu();
                foreach (Playlist p in AccountManager.instance.Acct.Playlists)
                {
                    MenuItem cmItem = new MenuItem();
                    cmItem.Header = p.Name;
                    if (p.ContainsSong(SearchListBox.SelectedItem as Song))
                    {
                        cmItem.IsChecked = true;
                        cmItem.Click += MenuItem_Remove_Click;
                    }
                    else
                    {
                        cmItem.IsChecked = false;
                        cmItem.Click += MenuItem_Add_Click;
                    }
                    cm.Items.Add(cmItem);
                }
                cm.IsOpen = true;
            }
        }

        /// <summary>
        /// Called by the context menu playlist without a checkmark to add the song to the playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            if (SearchListBox.SelectedItem != null)
            {
                MenuItem cmItem = sender as MenuItem;
                AccountManager.instance.Acct.FindPlaylist(cmItem.Header.ToString()).addSong(SearchListBox.SelectedItem as Song);
                AddRemoveSongMsg.Content = String.Format("{0} added to {1}!", (SearchListBox.SelectedItem as Song).Title, cmItem.Header);
                AddRemoveSongMsg.Visibility = Visibility.Visible;
                PlayPage.isChanged = true;
            }
        }

        /// <summary>
        /// called by teh context menu playlist with a checkmark to remove the song from the playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (SearchListBox.SelectedItem != null)
            {
                MenuItem cmItem = sender as MenuItem;
                AccountManager.instance.Acct.FindPlaylist(cmItem.Header.ToString()).RemoveEquivSong(SearchListBox.SelectedItem as Song);
                AddRemoveSongMsg.Content = String.Format("{0} removed from {1}!", (SearchListBox.SelectedItem as Song).Title, cmItem.Header);
                AddRemoveSongMsg.Visibility = Visibility.Visible;
            }
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if(!String.IsNullOrWhiteSpace((string)SearchTextBox.Text))
                {
                    Playlist availableSongs ;
                    var asen = new ASCIIEncoding();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("searchTitle"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes((string)SearchTextBox.Text));
                    var found = SocketClientOut.receiveAccess();
                    if(asen.GetString(found) == "Found")
                    {
                        var playlistofSong = SocketClientOut.receiveAccess();
                        availableSongs = JsonConvert.DeserializeObject<Playlist>(asen.GetString(playlistofSong));
                        foreach (var b in availableSongs.Songs)
                        {
                            searchQuery.addSong(b);
                        }

                    }
                    else if (asen.GetString(found) == "NotFound")
                    {
                        MessageBox.Show("no song title was matched with the keywork: " + (string)SearchTextBox.Text);

                    }
                    FilterSearch();
                    

                }      
            }
        }
    }
}
