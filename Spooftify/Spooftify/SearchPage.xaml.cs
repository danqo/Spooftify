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

namespace Spooftify
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        private Playlist searchQuery;

        public SearchPage()
        {
            InitializeComponent();
            searchQuery = AccountManager.instance.AllSongs;
            SearchListBox.ItemsSource = searchQuery.Songs;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Tag != null)
            {
                textBox.Tag = (!String.IsNullOrWhiteSpace(textBox.Text)).ToString();
            }
            FilterSearch();
        }

        private void SearchByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterSearch();
        }

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
            AddSongMsg.Visibility = Visibility.Hidden;
            SearchTextBox.Text = "";
            searchQuery = AccountManager.instance.AllSongs;
            SearchListBox.ItemsSource = searchQuery.Songs;
            SearchListBox.Items.Refresh();
        }

        // used by nhan to test, should be empty
        private void SearchListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                string songName = ((SearchListBox.SelectedItem) as Song).ToString();
                SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                //SocketClientOut.sendSongName(Encoding.ASCII.GetBytes("haha"));
                var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                if (msg == "granted")
                {
                    ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                    Thread receiveThread = new Thread(receiveStart);
                    SocketClientOut.buffering = true;
                    receiveThread.Start();
                    //SocketClientOut.playSong();
                }
                else
                {
                    MessageBox.Show(msg);
                }
            }
        }
    }
}
