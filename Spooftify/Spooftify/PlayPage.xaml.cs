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
using System.Timers;

namespace Spooftify
{
    /// <summary>
    /// Interaction logic for PlayPage.xaml
    /// </summary>
    public partial class PlayPage : Page
    {
        Thread receiveThread;

        private const string TIMESTAMP_FORMAT = @"mm\:ss";

        private BitmapImage PlayButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyPlayButton.png"));
        private BitmapImage PauseButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyPauseButton.png"));
        private BitmapImage StopButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyStopButton.png"));
        private BitmapImage PrevButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyPrevButton.png"));
        private BitmapImage NextButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyNextButton.png"));

        private Song curSong;

        private System.Timers.Timer myTimer;
        private TimeSpan timestamp = TimeSpan.Zero;
        private int currentIndex;

        public PlayPage()
        {
            InitializeComponent();
            myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(DisplayTimeEvent);
            myTimer.Interval = 1000; // 1000 ms is one second
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

        /*private void SongListbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SeekBar.Value = 0;
            var b = sender as ListBox;
            if (b.SelectedItem != null)
            {
                if (SocketClientOut.waveOut == null)
                {
                    SocketClientOut.buffering = true;
                    string songName = b.SelectedItem.ToString();
                    curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                    SocketClientOut.sendStartTime(Encoding.ASCII.GetBytes(SeekBar.Value.ToString()));
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {
                        msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                        TimeSpan total = new TimeSpan();
                        TimeSpan.TryParse(msg, out total);
                        TotalTimestampLabel.Content = total.Minutes + ":" + total.Seconds;
                        PlayerPlayPauseImage.Source = PauseButtonImg;
                        ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                        receiveThread = new Thread(receiveStart);
                        SocketClientOut.buffering = true;
                        currTitle.Content = curSong.Title;
                        currArtist.Content = curSong.Artist;
                        currAlbum.Content = curSong.Album;
                        currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                        if (currentIndex - 1 >= 0)
                        {
                            prevTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Title;
                            prevArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Artist;
                            prevAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Album;
                        }
                        else
                        {
                            prevTitle.Content = "None";
                            prevArtist.Content = "None";
                            prevAlbum.Content = "None";
                        }

                        if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                        {
                            nextTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Title;
                            nextArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Artist;
                            nextAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Album;
                        }
                        else
                        {
                            nextTitle.Content = "None";
                            nextArtist.Content = "None";
                            nextAlbum.Content = "None";
                        }

                        myTimer.Start();
                        SeekBar.Minimum = 0;
                        SeekBar.Maximum = (total.Minutes * 60) + total.Seconds;
                        SeekBar.TickFrequency = 1;
                        receiveThread.Start();
                        int a = receiveThread.ManagedThreadId;

                        //SocketClientOut.playSong();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(msg);
                    }
                }

                else
                {
                    if (SocketClientOut.buffering == true)
                        Thread.Sleep(150);
                    SocketClientOut.stopSong();
                    //SocketClientOut.waveOut.Dispose();
                    b = sender as ListBox;
                    string songName = b.SelectedItem.ToString();
                    curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                    SocketClientOut.sendStartTime(Encoding.ASCII.GetBytes(SeekBar.Value.ToString()));
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {
                        msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                        PlayerPlayPauseImage.Source = PauseButtonImg;
                        TimeSpan total = new TimeSpan();
                        TimeSpan.TryParse(msg, out total);
                        TotalTimestampLabel.Content = total.Minutes + ":" + total.Seconds;
                        ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                        receiveThread = new Thread(receiveStart);
                        SocketClientOut.buffering = true;
                        currTitle.Content = curSong.Title;
                        currArtist.Content = curSong.Artist;
                        currAlbum.Content = curSong.Album;
                        currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                        if (currentIndex - 1 >= 0)
                        {
                            prevTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Title;
                            prevArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Artist;
                            prevAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Album;
                        }
                        else
                        {
                            prevTitle.Content = "None";
                            prevArtist.Content = "None";
                            prevAlbum.Content = "None";
                        }

                        if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                        {
                            nextTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Title;
                            nextArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Artist;
                            nextAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Album;
                        }
                        else
                        {
                            nextTitle.Content = "None";
                            nextArtist.Content = "None";
                            nextAlbum.Content = "None";
                        }

                        myTimer.Start();
                        SeekBar.Minimum = 0;
                        SeekBar.Maximum = (total.Minutes * 60) + total.Seconds;
                        SeekBar.TickFrequency = 1;

                        receiveThread.Start();
                        //SocketClientOut.playSong();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(msg);
                    }
                }
            }
        }*/

        public void DisplayTimeEvent(object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                SeekBar.Value += 1;
                timestamp = timestamp.Add(TimeSpan.FromSeconds(1));
                CurrentTimestampLabel.Content = timestamp.ToString(TIMESTAMP_FORMAT);
            });


        }

        private void PlayerControlPrev_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Prev Clicked");
            myTimer.Stop();
            SongListbox.SelectedIndex = currentIndex - 1;
        }

        private void PlayerControlPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (SocketClientOut.waveOut != null)
            {
                if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                {
                    SocketClientOut.buffering = false;
                    myTimer.Stop();
                    SocketClientOut.stopSong();
                    PlayerPlayPauseImage.Source = PlayButtonImg;

                }
                else if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
                {
                    SocketClientOut.buffering = true;
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(curSong.Artist + " (" + curSong.Album + ") - " + curSong.Title));
                    SocketClientOut.currentLocation = (int)SeekBar.Value;
                    SocketClientOut.sendStartTime(Encoding.ASCII.GetBytes(SocketClientOut.currentLocation.ToString()));
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {

                        msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                        PlayerPlayPauseImage.Source = PauseButtonImg;
                        ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                        receiveThread = new Thread(receiveStart);
                        SocketClientOut.buffering = true;
                        myTimer.Start();
                        receiveThread.Start();
                        int a = receiveThread.ManagedThreadId;
                    }

                    PlayerPlayPauseImage.Source = PauseButtonImg;
                    //pauseResume.Content = "Pause";
                }
            }
            else
            {
                if (SongListbox.SelectedItem != null)
                {
                    string songName = SongListbox.SelectedItem.ToString();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {
                        PlayerPlayPauseImage.Source = PauseButtonImg;
                        ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                        receiveThread = new Thread(receiveStart);
                        SocketClientOut.buffering = true;
                        SeekBar.TickFrequency = 1;
                        myTimer.Start();
                        receiveThread.Start();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(msg);
                    }
                }
            }

        }

        private void PlayerControlStop_Click(object sender, RoutedEventArgs e)
        {
            if (SocketClientOut.waveOut != null)
            {
                if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing || SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Paused)
                {
                    SocketClientOut.buffering = false;
                    SeekBar.Value = 0;
                    myTimer.Stop();
                    timestamp = TimeSpan.Zero;
                    CurrentTimestampLabel.Content = timestamp.ToString(TIMESTAMP_FORMAT);
                    SocketClientOut.stopSong();
                    PlayerPlayPauseImage.Source = PlayButtonImg;
                }
            }
        }


        private void PlayerControlNext_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Next Clicked");
            myTimer.Stop();
            SongListbox.SelectedIndex = currentIndex + 1;
        }

        private void SeekBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                SocketClientOut.buffering = false;
                myTimer.Stop();
                SocketClientOut.stopSong();
                PlayerPlayPauseImage.Source = PlayButtonImg;
            }
        }

        private void SeekBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SocketClientOut.buffering = true;
            SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
            SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(curSong.Artist + " (" + curSong.Album + ") - " + curSong.Title));
            SocketClientOut.currentLocation = (int)SeekBar.Value;
            SocketClientOut.sendStartTime(Encoding.ASCII.GetBytes(SocketClientOut.currentLocation.ToString()));
            var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
            if (msg == "granted")
            {

                msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                PlayerPlayPauseImage.Source = PauseButtonImg;
                ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                receiveThread = new Thread(receiveStart);
                SocketClientOut.buffering = true;
                myTimer.Start();
                receiveThread.Start();
                int a = receiveThread.ManagedThreadId;
            }
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timestamp = new TimeSpan(0, (int)(SeekBar.Value / 60), (int)(SeekBar.Value % 60));
            CurrentTimestampLabel.Content = timestamp.ToString(TIMESTAMP_FORMAT);
            this.Dispatcher.Invoke(() =>
            {
                if (CurrentTimestampLabel.Content.Equals(TotalTimestampLabel.Content))
                {
                    if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                        SongListbox.SelectedIndex = currentIndex + 1;
                    myTimer.Stop();
                    PlayerPlayPauseImage.Source = PlayButtonImg;


                }
            });
        }

        private void SongListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SeekBar.Value = 0;
            var b = sender as ListBox;
            if (b.SelectedItem != null)
            {
                if (SocketClientOut.waveOut == null)
                {
                    SocketClientOut.buffering = true;
                    string songName = b.SelectedItem.ToString();
                    curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                    SocketClientOut.sendStartTime(Encoding.ASCII.GetBytes(SeekBar.Value.ToString()));
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {
                        msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                        TimeSpan total = new TimeSpan();
                        TimeSpan.TryParse(msg, out total);
                        TotalTimestampLabel.Content = total.ToString(TIMESTAMP_FORMAT);
                        PlayerPlayPauseImage.Source = PauseButtonImg;
                        ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                        receiveThread = new Thread(receiveStart);
                        SocketClientOut.buffering = true;
                        currTitle.Content = curSong.Title;
                        currArtist.Content = curSong.Artist;
                        currAlbum.Content = curSong.Album;
                        currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                        if (currentIndex - 1 >= 0)
                        {
                            PlayerPrevImage.IsEnabled = true;
                            PlayerPrevImage.Visibility = System.Windows.Visibility.Visible;
                            PlayerControlPrev.IsEnabled = true;
                            PlayerControlPrev.Visibility = System.Windows.Visibility.Visible;
                            prevTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Title;
                            prevArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Artist;
                            prevAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Album;
                        }
                        else
                        {
                            PlayerPrevImage.IsEnabled = false;
                            PlayerPrevImage.Visibility = System.Windows.Visibility.Hidden;
                            PlayerControlPrev.IsEnabled = false;
                            PlayerControlPrev.Visibility = System.Windows.Visibility.Hidden;
                            prevTitle.Content = "None";
                            prevArtist.Content = "None";
                            prevAlbum.Content = "None";
                        }

                        if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                        {
                            PlayerNextImage.IsEnabled = true;
                            PlayerNextImage.Visibility = System.Windows.Visibility.Visible;
                            PlayerControlNext.IsEnabled = true;
                            PlayerControlNext.Visibility = System.Windows.Visibility.Visible;
                            nextTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Title;
                            nextArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Artist;
                            nextAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Album;
                        }
                        else
                        {
                            PlayerNextImage.IsEnabled = false;
                            PlayerNextImage.Visibility = System.Windows.Visibility.Hidden;
                            PlayerControlNext.IsEnabled = false;
                            PlayerControlNext.Visibility = System.Windows.Visibility.Hidden;
                            nextTitle.Content = "None";
                            nextArtist.Content = "None";
                            nextAlbum.Content = "None";
                        }

                        myTimer.Start();
                        SeekBar.Minimum = 0;
                        SeekBar.Maximum = (total.Minutes * 60) + total.Seconds;
                        SeekBar.TickFrequency = 1;
                        receiveThread.Start();
                        int a = receiveThread.ManagedThreadId;

                        //SocketClientOut.playSong();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(msg);
                    }
                }

                else
                {
                    if (SocketClientOut.buffering == true)
                        Thread.Sleep(150);
                    SocketClientOut.stopSong();
                    //SocketClientOut.waveOut.Dispose();
                    b = sender as ListBox;
                    string songName = b.SelectedItem.ToString();
                    curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                    SocketClientOut.sendStartTime(Encoding.ASCII.GetBytes(SeekBar.Value.ToString()));
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {
                        msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                        PlayerPlayPauseImage.Source = PauseButtonImg;
                        TimeSpan total = new TimeSpan();
                        TimeSpan.TryParse(msg, out total);
                        TotalTimestampLabel.Content = total.ToString(TIMESTAMP_FORMAT);
                        ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                        receiveThread = new Thread(receiveStart);
                        SocketClientOut.buffering = true;
                        currTitle.Content = curSong.Title;
                        currArtist.Content = curSong.Artist;
                        currAlbum.Content = curSong.Album;
                        currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                        if (currentIndex - 1 >= 0)
                        {
                            PlayerPrevImage.IsEnabled = true;
                            PlayerPrevImage.Visibility = System.Windows.Visibility.Visible;
                            PlayerControlPrev.IsEnabled = true;
                            PlayerControlPrev.Visibility = System.Windows.Visibility.Visible;
                            prevTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Title;
                            prevArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Artist;
                            prevAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Album;
                        }
                        else
                        {
                            PlayerPrevImage.IsEnabled = false;
                            PlayerPrevImage.Visibility = System.Windows.Visibility.Hidden;
                            PlayerControlPrev.IsEnabled = false;
                            PlayerControlPrev.Visibility = System.Windows.Visibility.Hidden;
                            prevTitle.Content = "None";
                            prevArtist.Content = "None";
                            prevAlbum.Content = "None";
                        }

                        if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                        {
                            PlayerNextImage.IsEnabled = true;
                            PlayerNextImage.Visibility = System.Windows.Visibility.Visible;
                            PlayerControlNext.IsEnabled = true;
                            PlayerControlNext.Visibility = System.Windows.Visibility.Visible;
                            nextTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Title;
                            nextArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Artist;
                            nextAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Album;
                        }
                        else
                        {
                            PlayerNextImage.IsEnabled = false;
                            PlayerNextImage.Visibility = System.Windows.Visibility.Hidden;
                            PlayerControlNext.IsEnabled = false;
                            PlayerControlNext.Visibility = System.Windows.Visibility.Hidden;
                            nextTitle.Content = "None";
                            nextArtist.Content = "None";
                            nextAlbum.Content = "None";
                        }

                        myTimer.Start();
                        SeekBar.Minimum = 0;
                        SeekBar.Maximum = (total.Minutes * 60) + total.Seconds;
                        SeekBar.TickFrequency = 1;

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
    }
}
