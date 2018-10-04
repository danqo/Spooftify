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
            AddRemoveSongMsg.Visibility = Visibility.Hidden;
            SearchTextBox.Text = "";
            searchQuery = AccountManager.instance.AllSongs;
            SearchListBox.ItemsSource = searchQuery.Songs;
            SearchListBox.Items.Refresh();
        }

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

        private void MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            if (SearchListBox.SelectedItem != null)
            {
                MenuItem cmItem = sender as MenuItem;
                AccountManager.instance.Acct.FindPlaylist(cmItem.Header.ToString()).addSong(SearchListBox.SelectedItem as Song);
                AddRemoveSongMsg.Content = String.Format("{0} added to {1}!", (SearchListBox.SelectedItem as Song).Title, cmItem.Header);
                AddRemoveSongMsg.Visibility = Visibility.Visible;
            }
        }

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
    }
}
