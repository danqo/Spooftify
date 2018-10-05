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
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
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
                        System.Windows.MessageBox.Show(msg);
                    }
                }
                else
                {
                    SocketClientOut.stopSong();
                    SocketClientOut.waveOut.Dispose();
                    b = sender as ListBox;
                    string songName = b.SelectedItem.ToString();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
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
                        System.Windows.MessageBox.Show(msg);
                    }
                }
            }
        }

        private void PlayerControlPrev_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayerControlStop_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SocketClientOut.waveOut != null)
            {
                if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing || SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Paused)
                {
                    SocketClientOut.buffering = false;
                    SocketClientOut.stopSong();

                }
            }
        }

        private void PlayerControlStop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
