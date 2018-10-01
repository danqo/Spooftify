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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for PlaylistsPage.xaml
    /// </summary>
    public partial class PlaylistsPage : Page
    {
        private const string duplicatePlaylistMsg = "Playlist with that name already exists!";
        private const string addedPlaylistMsg = "Playlist added!";

        public PlaylistsPage()
        {
            
            InitializeComponent();
            PlaylistListBox.ItemsSource = AccountManager.instance.Acct.Playlists;   //observable collection?
            //PlaylistListBox.SetBinding
        }

        private void AddPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            
            // check for duplicate
            // AddPlaylistMsg.Visibility = Visibility.Visible;
        }

        private void AddPlaylistTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AccountManager.instance.Acct.Playlists.Add(new Playlist(AddPlaylistTextBox.Text));
                PlaylistListBox.Items.Refresh();
                AddPlaylistTextBox.Text = "";
            }
            // check for duplicate
            // AddPlaylistMsg.Visibility = Visibility.Visible;
        }

        private void AddPlaylistTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Tag != null)
            {
                textBox.Tag = (!String.IsNullOrWhiteSpace(textBox.Text)).ToString();
            }
        }
        // set currplaylist to null if the playlist to delete is curr playlist
        public void Reset()
        {
            AddPlaylistMsg.Visibility = Visibility.Hidden;
            AddPlaylistTextBox.Text = "";
        }
    }
}
