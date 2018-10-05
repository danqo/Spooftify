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
    /// Interaction logic for PlayPage.xaml
    /// </summary>
    public partial class PlayPage : Page
    {
        private const string ALBUM_LABEL = "Album: ";
        private const string ARTIST_LABEL = "Artist: ";
        private const string TITLE_LABEL = "Title: ";
        private Song curSong;

        public PlayPage()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            if (AccountManager.instance.CurrentPlaylist != null)
            {
                PlaylistName.Content = AccountManager.instance.CurrentPlaylist.Name;
                SongListbox.ItemsSource = AccountManager.instance.CurrentPlaylist.Songs;
                SongListbox.Items.Refresh();
            }
        }

        private void SongListbox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(TextBlock) && r.VisualHit.GetType() != typeof(Border))
            {
                SongListbox.SelectedItem = null;
            }
            else
            {
                ContextMenu cm = this.FindResource("playlistContextMenu") as ContextMenu;
                cm.PlacementTarget = sender as ListBox;
                cm.IsOpen = true;
            }
        }

        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (SongListbox.SelectedItem != null)
            {
                AccountManager.instance.CurrentPlaylist.deleteSong(SongListbox.SelectedItem as Song);
                SongListbox.ItemsSource = AccountManager.instance.CurrentPlaylist.Songs;
                SongListbox.Items.Refresh();
            }
        }

        private void SongListbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var b = sender as ListBox;
            if (b.SelectedItem != null)
            {
                if (SocketClientOut.waveOut == null)
                {

                    string songName = b.SelectedItem.ToString();
                    curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {
                        ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                        Thread receiveThread = new Thread(receiveStart);
                        SocketClientOut.buffering = true;
                        currTitle.Content = curSong.Title;
                        currArtist.Content = curSong.Artist;
                        currAlbum.Content = curSong.Album;
                        receiveThread.Start();
                        //SocketClientOut.playSong();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(msg);
                    }
                }
                else
                {
                    SocketClientOut.stopSong();
                    SocketClientOut.waveOut.Dispose();
                    b = sender as ListBox;
                    string songName = b.SelectedItem.ToString();
                    curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {
                        ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                        Thread receiveThread = new Thread(receiveStart);
                        SocketClientOut.buffering = true;
                        currTitle.Content = curSong.Title;
                        currArtist.Content = curSong.Artist;
                        currAlbum.Content = curSong.Album;
                        receiveThread.Start();
                        //SocketClientOut.playSong();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(msg);
                    }
                }
            }
        }

        private void PlayerControlPrev_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayerControlPlayPause_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayerControlStop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayerControlNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SeekBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SeekBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
