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

        private BitmapImage PlayButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyPlayButton.png"));
        private BitmapImage PauseButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyPauseButton.png"));
        private BitmapImage StopButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyStopButton.png"));
        private BitmapImage PrevButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyPrevButton.png"));
        private BitmapImage NextButtonImg = new BitmapImage(new Uri("pack://application:,,,/Images/" + "SpooftifyNextButton.png"));

        private Song curSong;

        private System.Timers.Timer myTimer;
        private int seconds = 0;
        private int minutes = 0;

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

        private void SongListbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {
                        msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                        TimeSpan total = new TimeSpan();
                        TimeSpan.TryParse(msg, out total);
                        TotalTimestampLabel.Content = total.Minutes+ ":" + total.Seconds;
                        PlayerPlayPauseImage.Source = PauseButtonImg;
                        ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                        receiveThread = new Thread(receiveStart);
                        SocketClientOut.buffering = true;
                        currTitle.Content = curSong.Title;
                        currArtist.Content = curSong.Artist;
                        currAlbum.Content = curSong.Album;
                        
                        myTimer.Start();
                        SeekBar.Minimum = 0;
                        SeekBar.Maximum = (total.Minutes*60)+total.Seconds;
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
                    SocketClientOut.waveOut.Dispose();
                    b = sender as ListBox;
                    string songName = b.SelectedItem.ToString();
                    curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                    SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                    SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                    var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                    if (msg == "granted")
                    {
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

        public void DisplayTimeEvent(object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                SeekBar.Value += 1;
                seconds += 1;
                if (seconds == 60)
                {
                    seconds = 0;
                    minutes += 1;
                }
                CurrentTimestampLabel.Content = minutes + ":" + seconds;
            });
            

        }

        private void PlayerControlPrev_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Prev Clicked");
        }

        private void PlayerControlPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (SocketClientOut.waveOut != null)
            {
                if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                {
                    SocketClientOut.buffering = false;
                    myTimer.Stop();
                    SocketClientOut.pauseSong();
                    PlayerPlayPauseImage.Source = PlayButtonImg;
                    
                }
                else if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Paused)
                {
                    SocketClientOut.buffering = true;
                    ThreadStart receiveStart = new ThreadStart(SocketClientOut.resumeSong);
                    Thread receiveThread = new Thread(receiveStart);
                    SeekBar.TickFrequency = 1;
                    myTimer.Start();
                    receiveThread.Start();

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
                    minutes = 0;
                    seconds = 0;
                    CurrentTimestampLabel.Content = minutes + ":" + seconds;
                    SocketClientOut.stopSong();
                    PlayerPlayPauseImage.Source = PlayButtonImg;
                }
            }
        }


        private void PlayerControlNext_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Next Clicked");
        }

        private void SeekBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                SocketClientOut.buffering = false;
                myTimer.Stop();
                SocketClientOut.pauseSong();
                PlayerPlayPauseImage.Source = PlayButtonImg;
            }
        }

        private void SeekBar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SocketClientOut.buffering = true;
            SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
            SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(curSong.Artist+" ("+curSong.Album+") - "+curSong.Title));
            var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
            if (msg == "granted")
            {
                msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());

                TimeSpan total = new TimeSpan();
                TimeSpan.TryParse(msg, out total);
                TotalTimestampLabel.Content = total.Minutes + ":" + total.Seconds;
                PlayerPlayPauseImage.Source = PauseButtonImg;
                SocketClientOut.currentLocation = (int)SeekBar.Value;
                ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                receiveThread = new Thread(receiveStart);
                SocketClientOut.buffering = true;
                currTitle.Content = curSong.Title;
                currArtist.Content = curSong.Artist;
                currAlbum.Content = curSong.Album;

                myTimer.Start();
                SeekBar.Minimum = 0;
                SeekBar.Maximum = (total.Minutes * 60) + total.Seconds;
                SeekBar.TickFrequency = 1;
                receiveThread.Start();
                int a = receiveThread.ManagedThreadId;
            }
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            minutes = (int)(SeekBar.Value/60);
            seconds = (int)(SeekBar.Value % 60);
            CurrentTimestampLabel.Content = minutes + ":" + seconds;
        }
    }
}
