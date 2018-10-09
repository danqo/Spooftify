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

        private ContextMenu cm;

        public static Boolean isChanged = false;

        /// <summary>
        /// constructor
        /// </summary>
        public PlayPage()
        {
            InitializeComponent();
            myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(DisplayTimeEvent);
            myTimer.Interval = 1000; // 1000 ms is one second
            displayControls();
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

        /// <summary>
        /// right clicking on a song show a context menu that allows the user to remove a song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongListbox_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(TextBlock) && r.VisualHit.GetType() != typeof(Border))
            {
                SongListbox.SelectedItem = null;
            }
            else
            {
                cm = this.FindResource("playlistContextMenu") as ContextMenu;
                cm.PlacementTarget = sender as ListBox;
                cm.IsOpen = true;
            }
        }

        /// <summary>
        /// handler that removes the selected song from the user's playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (SongListbox.SelectedItem != null)
            {
                if(AccountManager.instance.CurrentPlaylist.FindSongIndex(SongListbox.SelectedItem as Song) == currentIndex)
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
                            SeekBar.IsEnabled = false;
                        }
                    }
                }
                
                AccountManager.instance.CurrentPlaylist.deleteSong(SongListbox.SelectedItem as Song);
                SongListbox.ItemsSource = AccountManager.instance.CurrentPlaylist.Songs;
                SongListbox.Items.Refresh();
                displayControls();
            }
        }

        private void displayControls()
        {
            if(curSong != null)
            {
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
            }
            else
            {
                PlayerPlayPauseImage.Source = PlayButtonImg;
                prevAlbum.Content = "None";
                prevTitle.Content = "None";
                prevArtist.Content = "None";
                currAlbum.Content = "None";
                currArtist.Content = "None";
                currTitle.Content = "None";
                nextTitle.Content = "None";
                nextAlbum.Content = "None";
                nextArtist.Content = "None";
            }
            
        }

        /// <summary>
        /// plays the current song by sending a request to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongListbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SeekBar.Value = 0;
            var b = sender as ListBox;
            string songName = b.SelectedValue.ToString();
            if (b.SelectedItem != null)
            {
                if (SocketClientOut.waveOut == null)
                {
                    songPlay(songName);
                }

                else
                {
                    otherSongPlay(songName);
                }
            }
        }

        /// <summary>
        /// updates the displayed timestamp for the song
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void DisplayTimeEvent(object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                SeekBar.Value += 1;
                timestamp = timestamp.Add(TimeSpan.FromSeconds(1));
                CurrentTimestampLabel.Content = timestamp.ToString(TIMESTAMP_FORMAT);
            });
        }

        /// <summary>
        /// Plays the previous song on the playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerControlPrev_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Prev Clicked");
            myTimer.Stop();
            SongListbox.SelectedIndex = currentIndex - 1;
            SeekBar.Value = 0;
            string songName = SongListbox.SelectedValue.ToString();
            if (SongListbox.SelectedItem != null)
            {
                if (SocketClientOut.waveOut == null)
                {
                    songPlay(songName);
                }

                else
                {
                    otherSongPlay(songName);
                }
            }
        }

        /// <summary>
        /// Pauses the currently played song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerControlPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (SocketClientOut.waveOut != null)
            {
                if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                {
                    SocketClientOut.buffering = false;
                    myTimer.Stop();
                    try
                    {
                        SocketClientOut.stopSong();
                    }
                    catch
                    {
                        SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("no more"));
                        if(SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Paused)
                            PlayerPlayPauseImage.Source = PlayButtonImg;
                        else
                            PlayerPlayPauseImage.Source = PauseButtonImg;
                        MessageBox.Show("DON'T SPAM CLICK");
                    }
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
                string songName = SongListbox.SelectedItem.ToString();
                songPlay(songName);
               
            }

        }

        /// <summary>
        /// resets the timer and stops the currently played song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                else
                {
                    if(SeekBar.Value!= 0 )
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
        }

        /// <summary>
        /// plays the next song on the playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerControlNext_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Next Clicked");
            myTimer.Stop();
            SongListbox.SelectedIndex = currentIndex + 1;
            SeekBar.Value = 0;

            string songName = SongListbox.SelectedValue.ToString();
            if (SongListbox.SelectedItem != null)
            {
                if (SocketClientOut.waveOut == null)
                {
                    songPlay(songName);
                }

                else
                {
                    otherSongPlay(songName);
                }
            }

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
            if (curSong != null)
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
            else
            {
                SeekBar.Value = 0;
                MessageBox.Show("no song is playing atm");
            }
        }

        /// <summary>
        /// updates the timestamp value when dragging the handle on the seek bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timestamp = new TimeSpan(0, (int)(SeekBar.Value / 60), (int)(SeekBar.Value % 60));
            CurrentTimestampLabel.Content = timestamp.ToString(TIMESTAMP_FORMAT);
            this.Dispatcher.Invoke(() =>
            {
                if (CurrentTimestampLabel.Content.Equals(TotalTimestampLabel.Content))
                {
                    if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                    {
                        SeekBar.IsEnabled = true;
                        myTimer.Stop();
                        SongListbox.SelectedIndex = currentIndex + 1;
                        curSong = (Song) SongListbox.Items[currentIndex + 1];
                        SeekBar.Value = 0;
                        SocketClientOut.buffering = true;
                        SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                        SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(curSong.Artist + " (" + curSong.Album + ") - " + curSong.Title));
                        SocketClientOut.currentLocation = (int)SeekBar.Value;
                        SocketClientOut.sendStartTime(Encoding.ASCII.GetBytes(SocketClientOut.currentLocation.ToString()));
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
                            CurrentTimestampLabel.Content = (new TimeSpan(0, 0, 0)).ToString(TIMESTAMP_FORMAT);
                            SeekBar.Minimum = 0;
                            SeekBar.Maximum = (total.Minutes * 60) + total.Seconds;
                            SeekBar.TickFrequency = 1;
                            myTimer.Start();
                            receiveThread.Start();
                            int a = receiveThread.ManagedThreadId;
                        }

                        PlayerPlayPauseImage.Source = PauseButtonImg;
                        displayControls();
                        //pauseResume.Content = "Pause";
                        
                    }
                    else
                    {
                        SeekBar.IsEnabled = false;
                        myTimer.Stop();
                        CurrentTimestampLabel.Content = (new TimeSpan(0, 0, 0)).ToString(TIMESTAMP_FORMAT);
                        PlayerPlayPauseImage.Source = PlayButtonImg;
                        SocketClientOut.stopSong();
                        displayControls();
                        SeekBar.Value = 0;
                    }

                }
            });
        }

        /// <summary>
        /// handles the udp request to play a specified song
        /// </summary>
        /// <param name="selectSong">song information of the selected song</param>
        public void songPlay(string selectSong)
        {
            if (SocketClientOut.waveOut == null)
            {
                SocketClientOut.buffering = true;
                string songName = selectSong;
                curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                SocketClientOut.sendStartTime(Encoding.ASCII.GetBytes(SeekBar.Value.ToString()));
                var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                if (msg == "granted")
                {
                    SeekBar.IsEnabled = true;
                    msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                    TimeSpan total = new TimeSpan();
                    TimeSpan.TryParse(msg, out total);
                    TotalTimestampLabel.Content = total.ToString(TIMESTAMP_FORMAT);
                    PlayerPlayPauseImage.Source = PauseButtonImg;
                    ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                    receiveThread = new Thread(receiveStart);
                    SocketClientOut.buffering = true;
                    displayControls();
                    CurrentTimestampLabel.Content = (new TimeSpan(0, 0, 0)).ToString(TIMESTAMP_FORMAT);
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
        }

        /// <summary>
        /// handles the procedure for playing another song if a song is already being played
        /// </summary>
        /// <param name="songSelected"></param>
        private void otherSongPlay(string songSelected)
        {
            if (SocketClientOut.buffering == true)
                Thread.Sleep(150);
            SocketClientOut.stopSong();
            //SocketClientOut.waveOut.Dispose();
            string songName = songSelected;
            curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
            SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
            SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
            SocketClientOut.sendStartTime(Encoding.ASCII.GetBytes(SeekBar.Value.ToString()));
            var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
            if (msg == "granted")
            {
                msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                SeekBar.IsEnabled = true;
                PlayerPlayPauseImage.Source = PauseButtonImg;
                TimeSpan total = new TimeSpan();
                TimeSpan.TryParse(msg, out total);
                TotalTimestampLabel.Content = total.ToString(TIMESTAMP_FORMAT);
                PlayerPlayPauseImage.Source = PauseButtonImg;
                ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                receiveThread = new Thread(receiveStart);
                SocketClientOut.buffering = true;
                displayControls();
                CurrentTimestampLabel.Content = (new TimeSpan(0, 0, 0)).ToString(TIMESTAMP_FORMAT);
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
