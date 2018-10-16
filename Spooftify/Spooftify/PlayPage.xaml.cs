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

        private Song prevSong;
        private Song curSong;
        private Song nextSong;
        private List<Song> shuffledPL = new List<Song>();

        private System.Timers.Timer myTimer;
        private TimeSpan timestamp = TimeSpan.Zero;
        private int currentIndex;
        private int currentShuffleIndex;
        private int timeBuffer;
        private int prevShuffle;
        private int nextShuffle;

        private ContextMenu cm;

        public static Boolean isChanged = false;
        public static Boolean isPlaylistChange = false;
        private Boolean isShuffle = false;
        private Boolean isLoop = false;

        public async Task randomGeneratorAsync()
        {
            Random rnd = new Random();

            if (curSong != null && AccountManager.instance.CurrentPlaylist.Songs.Count > 2)
            {
                if (!isLoop)
                {
                    int[] shuffleIndex = new int[AccountManager.instance.CurrentPlaylist.Songs.Count];
                    int songCount = 0;
                    int songIndex;
                    shuffleIndex[0] = currentIndex;
                    songCount++;
                    shuffledPL.Clear();
                    while (songCount < shuffleIndex.Length - 1)
                    {
                        songIndex = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                        System.Diagnostics.Debug.WriteLine(songIndex);
                        if (!shuffleIndex.Contains(songIndex))
                        {
                            shuffleIndex[songCount] = songIndex;
                            songCount++;
                        }
                    }
                    for (int i = 0; i < shuffleIndex.Length; i++)
                    {
                        shuffledPL.Add(AccountManager.instance.CurrentPlaylist.Songs[shuffleIndex[i]]);
                    }

                    prevSong = null;
                    curSong = shuffledPL[0];
                    nextSong = shuffledPL[1];
                }
                else
                {
                    prevShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    while (prevShuffle == currentIndex)
                    {
                        prevShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    }
                    nextShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    while (nextShuffle == currentIndex || nextShuffle == prevShuffle)
                    {
                        nextShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    }
                    prevSong = AccountManager.instance.CurrentPlaylist.Songs[prevShuffle];
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle];
                }
            }
        }

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
                if (isPlaylistChange)
                {
                    if(curSong != null)
                    {
                        SocketClientOut.buffering = false;
                        SocketClientOut.stopSong();
                    }
                    SeekBar.Value = 0;
                    myTimer.Stop();
                    timestamp = TimeSpan.Zero;
                    CurrentTimestampLabel.Content = timestamp.ToString(TIMESTAMP_FORMAT);
                    TotalTimestampLabel.Content = timestamp.ToString(TIMESTAMP_FORMAT);
                    prevSong = null;
                    curSong = null;
                    nextSong = null;
                    PlayerPlayPauseImage.Source = PlayButtonImg;
                    isPlaylistChange = false;
                    isShuffle = false;
                    ShuffleButton.Content = "Shuffle: Off";
                    ShuffleButton.Background = Brushes.Black;
                    isLoop = false;
                    LoopButton.Content = "Loop: Off";
                    LoopButton.Background = Brushes.Black;
                    displayControls();
                }
                if (isChanged)
                {
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1];
                    if (currentIndex == 0 && isLoop)
                        prevSong = AccountManager.instance.CurrentPlaylist.Songs[AccountManager.instance.CurrentPlaylist.Songs.Count-1];
                    if (!isShuffle)
                        displayControls();
                    if (!isLoop && isShuffle)
                    {
                        shuffledPL.Add(AccountManager.instance.CurrentPlaylist.Songs[shuffledPL.Count - 1]);
                        displayShuffleControls();
                    }
                    isChanged = false;
                }
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
                int deleteIndex = AccountManager.instance.CurrentPlaylist.FindSongIndex(SongListbox.SelectedItem as Song);
                if (deleteIndex == currentIndex)
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
                    prevSong = null;
                    curSong = null;
                    nextSong = null;
                }
                else
                {
                    AccountManager.instance.CurrentPlaylist.deleteSong(SongListbox.SelectedItem as Song);
                    SongListbox.ItemsSource = AccountManager.instance.CurrentPlaylist.Songs;
                    SongListbox.Items.Refresh();
                    if(deleteIndex < currentIndex)
                        currentIndex--;
                    if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                        nextSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1];
                    if (currentIndex - 1 >= 0)
                        prevSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1];
                }

                AccountManager.instance.CurrentPlaylist.deleteSong(SongListbox.SelectedItem as Song);
                SongListbox.ItemsSource = AccountManager.instance.CurrentPlaylist.Songs;
                SongListbox.Items.Refresh();
                displayControls();
            }
        }

        private void displayShuffleControls()
        {
            if (curSong != null)
            {
                BitmapImage imageCurrent = new BitmapImage();
                imageCurrent.BeginInit();
                imageCurrent.UriSource = new Uri(curSong.AlbumArt);
                imageCurrent.EndInit();
                curImg.Source = imageCurrent;
                currTitle.Content = curSong.Title;
                currArtist.Content = curSong.Artist;
                currAlbum.Content = curSong.Album;
                currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                if (!isLoop && currentShuffleIndex - 1 >= 0)
                {
                    BitmapImage imagePrev = new BitmapImage();
                    imagePrev.BeginInit();
                    imagePrev.UriSource = new Uri(prevSong.AlbumArt);
                    imagePrev.EndInit();
                    prevImg.Source = imagePrev;
                    PlayerPrevImage.IsEnabled = true;
                    PlayerPrevImage.Visibility = System.Windows.Visibility.Visible;
                    PlayerControlPrev.IsEnabled = true;
                    PlayerControlPrev.Visibility = System.Windows.Visibility.Visible;
                    prevTitle.Content = shuffledPL[currentShuffleIndex - 1].Title;
                    prevArtist.Content = shuffledPL[currentShuffleIndex - 1].Artist;
                    prevAlbum.Content = shuffledPL[currentShuffleIndex - 1].Album;
                }
                else if (!isLoop && currentShuffleIndex - 1 == -1)
                {
                    prevImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative));
                    PlayerPrevImage.IsEnabled = false;
                    PlayerPrevImage.Visibility = System.Windows.Visibility.Hidden;
                    PlayerControlPrev.IsEnabled = false;
                    PlayerControlPrev.Visibility = System.Windows.Visibility.Hidden;
                    prevTitle.Content = "None";
                    prevArtist.Content = "None";
                    prevAlbum.Content = "None";
                }
                if (!isLoop && currentShuffleIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                {
                    BitmapImage imageNext = new BitmapImage();
                    imageNext.BeginInit();
                    imageNext.UriSource = new Uri(nextSong.AlbumArt);
                    imageNext.EndInit();
                    nextImg.Source = imageNext;
                    PlayerNextImage.IsEnabled = true;
                    PlayerNextImage.Visibility = System.Windows.Visibility.Visible;
                    PlayerControlNext.IsEnabled = true;
                    PlayerControlNext.Visibility = System.Windows.Visibility.Visible;
                    nextTitle.Content = shuffledPL[currentShuffleIndex + 1].Title;
                    nextArtist.Content = shuffledPL[currentShuffleIndex + 1].Artist;
                    nextAlbum.Content = shuffledPL[currentShuffleIndex + 1].Album;
                }
                else if (!isLoop && currentShuffleIndex + 1 == AccountManager.instance.CurrentPlaylist.Songs.Count)
                {
                    nextImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative));
                    PlayerNextImage.IsEnabled = false;
                    PlayerNextImage.Visibility = System.Windows.Visibility.Hidden;
                    PlayerControlNext.IsEnabled = false;
                    PlayerControlNext.Visibility = System.Windows.Visibility.Hidden;
                    nextTitle.Content = "None";
                    nextArtist.Content = "None";
                    nextAlbum.Content = "None";
                }
                else if (isLoop)
                {
                    BitmapImage imagePrev = new BitmapImage();
                    imagePrev.BeginInit();
                    imagePrev.UriSource = new Uri(prevSong.AlbumArt);
                    imagePrev.EndInit();
                    prevImg.Source = imagePrev;
                    PlayerPrevImage.IsEnabled = true;
                    PlayerPrevImage.Visibility = System.Windows.Visibility.Visible;
                    PlayerControlPrev.IsEnabled = true;
                    PlayerControlPrev.Visibility = System.Windows.Visibility.Visible;
                    prevTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[prevShuffle].Title;
                    prevArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[prevShuffle].Artist;
                    prevAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[prevShuffle].Album;
                    BitmapImage imageNext = new BitmapImage();
                    imageNext.BeginInit();
                    imageNext.UriSource = new Uri(nextSong.AlbumArt);
                    imageNext.EndInit();
                    nextImg.Source = imageNext;
                    PlayerNextImage.IsEnabled = true;
                    PlayerNextImage.Visibility = System.Windows.Visibility.Visible;
                    PlayerControlNext.IsEnabled = true;
                    PlayerControlNext.Visibility = System.Windows.Visibility.Visible;
                    nextTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle].Title;
                    nextArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle].Artist;
                    nextAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle].Album;
                }
            }
            else
            {
                prevImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative));
                curImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative));
                nextImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative));
                PlayerPlayPauseImage.Source = PlayButtonImg;
                PlayerNextImage.IsEnabled = true;
                PlayerNextImage.Visibility = System.Windows.Visibility.Visible;
                PlayerControlNext.IsEnabled = true;
                PlayerControlNext.Visibility = System.Windows.Visibility.Visible;
                PlayerPrevImage.IsEnabled = true;
                PlayerPrevImage.Visibility = System.Windows.Visibility.Visible;
                PlayerControlPrev.IsEnabled = true;
                PlayerControlPrev.Visibility = System.Windows.Visibility.Visible;
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

        private void displayControls()
        {
            if(curSong != null)
            {
                BitmapImage imageCurrent = new BitmapImage();
                imageCurrent.BeginInit();
                imageCurrent.UriSource = new Uri(curSong.AlbumArt);
                imageCurrent.EndInit();
                curImg.Source = imageCurrent;
                currTitle.Content = curSong.Title;
                currArtist.Content = curSong.Artist;
                currAlbum.Content = curSong.Album;
                currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                if (currentIndex - 1 >= 0)
                {
                    BitmapImage imagePrev = new BitmapImage();
                    imagePrev.BeginInit();
                    imagePrev.UriSource = new Uri(prevSong.AlbumArt);
                    imagePrev.EndInit();
                    prevImg.Source = imagePrev;
                    PlayerPrevImage.IsEnabled = true;
                    PlayerPrevImage.Visibility = System.Windows.Visibility.Visible;
                    PlayerControlPrev.IsEnabled = true;
                    PlayerControlPrev.Visibility = System.Windows.Visibility.Visible;
                    prevTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Title;
                    prevArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Artist;
                    prevAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1].Album;
                }
                else if(!isLoop && currentIndex - 1 == -1)
                {
                    prevImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative));
                    PlayerPrevImage.IsEnabled = false;
                    PlayerPrevImage.Visibility = System.Windows.Visibility.Hidden;
                    PlayerControlPrev.IsEnabled = false;
                    PlayerControlPrev.Visibility = System.Windows.Visibility.Hidden;
                    prevTitle.Content = "None";
                    prevArtist.Content = "None";
                    prevAlbum.Content = "None";
                }
                else if(isLoop && currentIndex - 1 == -1)
                {
                    BitmapImage imagePrev = new BitmapImage();
                    imagePrev.BeginInit();
                    imagePrev.UriSource = new Uri(prevSong.AlbumArt);
                    imagePrev.EndInit();
                    prevImg.Source = imagePrev;
                    PlayerPrevImage.IsEnabled = true;
                    PlayerPrevImage.Visibility = System.Windows.Visibility.Visible;
                    PlayerControlPrev.IsEnabled = true;
                    PlayerControlPrev.Visibility = System.Windows.Visibility.Visible;
                    prevTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[AccountManager.instance.CurrentPlaylist.Songs.Count - 1].Title;
                    prevArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[AccountManager.instance.CurrentPlaylist.Songs.Count - 1].Artist;
                    prevAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[AccountManager.instance.CurrentPlaylist.Songs.Count - 1].Album;
                }

                if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                {
                    BitmapImage imageNext = new BitmapImage();
                    imageNext.BeginInit();
                    imageNext.UriSource = new Uri(nextSong.AlbumArt);
                    imageNext.EndInit();
                    nextImg.Source = imageNext;
                    PlayerNextImage.IsEnabled = true;
                    PlayerNextImage.Visibility = System.Windows.Visibility.Visible;
                    PlayerControlNext.IsEnabled = true;
                    PlayerControlNext.Visibility = System.Windows.Visibility.Visible;
                    nextTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Title;
                    nextArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Artist;
                    nextAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1].Album;
                }
                else if(!isLoop && currentIndex + 1 == AccountManager.instance.CurrentPlaylist.Songs.Count)
                {
                    nextImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative));
                    PlayerNextImage.IsEnabled = false;
                    PlayerNextImage.Visibility = System.Windows.Visibility.Hidden;
                    PlayerControlNext.IsEnabled = false;
                    PlayerControlNext.Visibility = System.Windows.Visibility.Hidden;
                    nextTitle.Content = "None";
                    nextArtist.Content = "None";
                    nextAlbum.Content = "None";
                }
                else if(isLoop && currentIndex + 1 == AccountManager.instance.CurrentPlaylist.Songs.Count)
                {
                    BitmapImage imageNext = new BitmapImage();
                    imageNext.BeginInit();
                    imageNext.UriSource = new Uri(nextSong.AlbumArt);
                    imageNext.EndInit();
                    nextImg.Source = imageNext;
                    PlayerNextImage.IsEnabled = true;
                    PlayerNextImage.Visibility = System.Windows.Visibility.Visible;
                    PlayerControlNext.IsEnabled = true;
                    PlayerControlNext.Visibility = System.Windows.Visibility.Visible;
                    nextTitle.Content = AccountManager.instance.CurrentPlaylist.Songs[0].Title;
                    nextArtist.Content = AccountManager.instance.CurrentPlaylist.Songs[0].Artist;
                    nextAlbum.Content = AccountManager.instance.CurrentPlaylist.Songs[0].Album;
                }
            }
            else
            {
                prevImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative));
                curImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative));
                nextImg.Source = new BitmapImage(new Uri("Images/music-logo-design.png", UriKind.Relative)); 
                PlayerPlayPauseImage.Source = PlayButtonImg;
                PlayerNextImage.IsEnabled = true;
                PlayerNextImage.Visibility = System.Windows.Visibility.Visible;
                PlayerControlNext.IsEnabled = true;
                PlayerControlNext.Visibility = System.Windows.Visibility.Visible;
                PlayerPrevImage.IsEnabled = true;
                PlayerPrevImage.Visibility = System.Windows.Visibility.Visible;
                PlayerControlPrev.IsEnabled = true;
                PlayerControlPrev.Visibility = System.Windows.Visibility.Visible;
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
        private async void SongListbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var b = sender as ListBox;
            if (b.SelectedValue != null)
            {
                SeekBar.Value = 0;
                
                string songName = b.SelectedValue.ToString();
                curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                if (b.SelectedItem != null)
                {
                    if (SocketClientOut.waveOut == null)
                    {
                        if (isShuffle)
                            await randomGeneratorAsync();
                        songPlay(songName);
                    }

                    else
                    {
                        if (isShuffle)
                            await randomGeneratorAsync();
                        otherSongPlay(songName);
                    }
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
            if (!isShuffle)
            {
                if (isLoop && currentIndex == 0)
                {
                    currentIndex = AccountManager.instance.CurrentPlaylist.Songs.Count;
                    SongListbox.SelectedIndex = currentIndex;
                }
                if (currentIndex > 0)
                {
                    SongListbox.SelectedIndex = currentIndex - 1;
                    currentIndex = currentIndex - 1;
                }
            }
            else if (isShuffle)
            {
                if (isLoop)
                {
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex];
                    nextShuffle = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(nextSong);
                    curSong = AccountManager.instance.CurrentPlaylist.Songs[prevShuffle];
                    currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                    SongListbox.SelectedIndex = currentIndex;
                    Random rnd = new Random();
                    prevShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    while (prevShuffle == nextShuffle || prevShuffle == currentIndex)
                    {
                        prevShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    }
                    prevSong = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle];
                    SongListbox.SelectedIndex = currentIndex;
                }
                else
                {
                    currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(prevSong);
                    SongListbox.SelectedIndex = currentIndex;
                }
            }
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
                try
                {
                    string songName = SongListbox.SelectedItem.ToString();
                    songPlay(songName);
                }
                catch
                {
                    MessageBox.Show("PICK A DAMM SONG");
                }
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
            if (!isShuffle)
            {
                if (isLoop && currentIndex == AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                {
                    SongListbox.SelectedIndex = 0;
                    currentIndex = 0;
                }
                else if (currentIndex < AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                {
                    SongListbox.SelectedIndex = currentIndex + 1;
                }
            }
            else if (isShuffle)
            {
                if (isLoop)
                {
                    prevSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex];
                    prevShuffle = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(prevSong);
                    curSong = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle];
                    currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                    SongListbox.SelectedIndex = currentIndex;
                    Random rnd = new Random();
                    nextShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    while (nextShuffle == prevShuffle || nextShuffle == currentIndex)
                    {
                        nextShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    }
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle];
                    SongListbox.SelectedIndex = currentIndex;
                }
                else
                {
                    currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(nextSong);
                    SongListbox.SelectedIndex = currentIndex;
                }
            }
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
                timeBuffer = (int)SeekBar.Value;
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
                if(SeekBar.Value <= timeBuffer+6 && SeekBar.Value > timeBuffer-2)
                {
                    SeekBar.Value = timeBuffer + 6;
                }
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
                    receiveThread.Abort();
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
                if (CurrentTimestampLabel.Content.Equals(TotalTimestampLabel.Content) && !isShuffle)
                {
                    myTimer.Stop();
                    SeekBar.Value = 0;
                    timestamp = TimeSpan.Zero;
                    CurrentTimestampLabel.Content = timestamp.ToString(TIMESTAMP_FORMAT);
                    if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                    {
                        playNextSong();
                    }
                    else
                    {
                        if (!isLoop)
                        {
                            myTimer.Stop();
                            CurrentTimestampLabel.Content = (new TimeSpan(0, 0, 0)).ToString(TIMESTAMP_FORMAT);
                            PlayerPlayPauseImage.Source = PlayButtonImg;
                            SocketClientOut.waveOut.Stop();
                            displayControls();
                            SeekBar.Value = 0;
                        }
                        else
                        {
                            currentIndex = -1;
                            playNextSong();
                        }                        
                    }                    
                }
                else if (CurrentTimestampLabel.Content.Equals(TotalTimestampLabel.Content) && isShuffle)
                {
                    myTimer.Stop();
                    SeekBar.Value = 0;
                    timestamp = TimeSpan.Zero;
                    CurrentTimestampLabel.Content = timestamp.ToString(TIMESTAMP_FORMAT);
                    if (currentShuffleIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                    {
                        playNextSong();
                    }
                    else
                    {
                        if (!isLoop)
                        {
                            myTimer.Stop();
                            CurrentTimestampLabel.Content = (new TimeSpan(0, 0, 0)).ToString(TIMESTAMP_FORMAT);
                            PlayerPlayPauseImage.Source = PlayButtonImg;
                            SocketClientOut.waveOut.Stop();
                            displayShuffleControls();
                            SeekBar.Value = 0;
                        }
                        else
                        {
                            playNextSong();
                        }
                    }
                }
            });
        }

        public void playNextSong()
        {
            SeekBar.IsEnabled = true;
            myTimer.Stop();
            if (!isShuffle)
            {
                SongListbox.SelectedIndex = currentIndex + 1;
                curSong = (Song)SongListbox.Items[currentIndex + 1];
                currentIndex++;
                if (currentIndex > 0)
                {
                    prevSong = (Song)SongListbox.Items[currentIndex - 1];
                }
                else if (isLoop && currentIndex == 0)
                {
                    prevSong = (Song)SongListbox.Items[AccountManager.instance.CurrentPlaylist.Songs.Count - 1];
                }
                else if (!isLoop && currentIndex == 0)
                {
                    prevSong = null;
                }
                if (currentIndex < AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                    nextSong = (Song)SongListbox.Items[currentIndex + 1];
                else if (isLoop && currentIndex == AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                    nextSong = (Song)SongListbox.Items[0];
                else if (!isLoop && currentIndex == AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                    nextSong = null;
                SeekBar.Value = 0;
                displayControls();
            }
            else
            {
                if (!isLoop)
                {
                    currentShuffleIndex++;
                    curSong = shuffledPL[currentShuffleIndex];
                    prevSong = shuffledPL[currentShuffleIndex - 1];
                    nextSong = shuffledPL[currentShuffleIndex + 1];
                    currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                    SongListbox.SelectedIndex = currentIndex;
                    displayShuffleControls();
                }
                else
                {
                    prevSong = curSong;
                    prevShuffle = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(prevSong);
                    curSong = nextSong;
                    currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                    SongListbox.SelectedIndex = currentIndex;
                    Random rnd = new Random();
                    nextShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    while(nextShuffle == prevShuffle || nextShuffle == currentIndex)
                    {
                        nextShuffle = rnd.Next(AccountManager.instance.CurrentPlaylist.Songs.Count);
                    }
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle];
                    displayShuffleControls();
                }


            }
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
                SeekBar.Minimum = 0;
                SeekBar.Maximum = (total.Minutes * 60) + total.Seconds;
                SeekBar.TickFrequency = 1;
                receiveThread.Start();
                CurrentTimestampLabel.Content = (new TimeSpan(0, 0, 0)).ToString(TIMESTAMP_FORMAT);
                myTimer.Start();
                int a = receiveThread.ManagedThreadId;
            }

            PlayerPlayPauseImage.Source = PauseButtonImg;
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
                if (!isShuffle)
                {
                    curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                    currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                    if (currentIndex - 1 >= 0)
                        prevSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1];
                    else if (currentIndex - 1 == -1 && isLoop)
                        prevSong = AccountManager.instance.CurrentPlaylist.Songs[AccountManager.instance.CurrentPlaylist.Songs.Count - 1];
                    if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                        nextSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1];
                    else if (currentIndex + 1 == AccountManager.instance.CurrentPlaylist.Songs.Count && isLoop)
                        nextSong = AccountManager.instance.CurrentPlaylist.Songs[0];
                }
                else if (isShuffle)
                {
                    //curSong = shuffledPL.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                    if (!isLoop)
                    {
                        currentShuffleIndex = shuffledPL.IndexOf(curSong);
                        currentIndex = shuffledPL.IndexOf(curSong);
                        if (currentShuffleIndex - 1 >= 0)
                            prevSong = shuffledPL[currentShuffleIndex - 1];
                        else
                            prevSong = null;
                        if (currentShuffleIndex + 1 < shuffledPL.Count)
                            nextSong = shuffledPL[currentShuffleIndex + 1];
                        else
                            nextSong = null;
                    }
                    else
                    {
                        prevSong = AccountManager.instance.CurrentPlaylist.Songs[prevShuffle];
                        nextSong = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle];
                    }
                }
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
                    if (!isShuffle)
                        displayControls();
                    if (isShuffle)
                        displayShuffleControls();
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
            if (!isShuffle)
            {
                curSong = AccountManager.instance.CurrentPlaylist.Songs.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                currentIndex = AccountManager.instance.CurrentPlaylist.Songs.IndexOf(curSong);
                if (currentIndex - 1 >= 0)
                    prevSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1];
                else if (currentIndex == 0 && isLoop)
                    prevSong = AccountManager.instance.CurrentPlaylist.Songs[AccountManager.instance.CurrentPlaylist.Songs.Count - 1];
                if (currentIndex + 1 < AccountManager.instance.CurrentPlaylist.Songs.Count)
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1]; 
                else if (currentIndex + 1 == AccountManager.instance.CurrentPlaylist.Songs.Count && isLoop)
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[0];
            }
            else if (isShuffle)
            {
                //curSong = shuffledPL.Where(x => (x.Artist + " (" + x.Album + ") - " + x.Title).Equals(songName)).SingleOrDefault();
                if (!isLoop)
                {
                    currentShuffleIndex = shuffledPL.IndexOf(curSong);
                    currentIndex = shuffledPL.IndexOf(curSong);
                    if (currentShuffleIndex - 1 >= 0)
                        prevSong = shuffledPL[currentShuffleIndex - 1];
                    else
                        prevSong = null;
                    if (currentShuffleIndex + 1 < shuffledPL.Count)
                        nextSong = shuffledPL[currentShuffleIndex + 1];
                    else
                        nextSong = null;
                }
                else
                {
                    prevSong = AccountManager.instance.CurrentPlaylist.Songs[prevShuffle];
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[nextShuffle];
                }
            }
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
                if(!isShuffle)
                    displayControls();
                if (isShuffle)
                    displayShuffleControls();
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

        private async void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if(ShuffleButton.Content.ToString() == "Shuffle: Off")
            {
                currentShuffleIndex = 0;
                ShuffleButton.Content = "Shuffle: On";
                ShuffleButton.Background = Brushes.DarkGreen;
                isShuffle = true;
                await randomGeneratorAsync();
                displayShuffleControls();
            }
            else if(ShuffleButton.Content.ToString() == "Shuffle: On")
            {
                isShuffle = false;
                ShuffleButton.Content = "Shuffle: Off";
                ShuffleButton.Background = Brushes.Black;
                if (currentIndex >= 0)
                    prevSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex - 1];
                if (!isLoop && currentIndex == 0)
                    prevSong = null;
                else if (isLoop && currentIndex == 0)
                    prevSong = AccountManager.instance.CurrentPlaylist.Songs[AccountManager.instance.CurrentPlaylist.Songs.Count - 1];
                if (currentIndex < AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[currentIndex + 1];
                if (!isLoop && currentIndex == AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                    nextSong = null;
                else if (isLoop && currentIndex == AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                    nextSong = AccountManager.instance.CurrentPlaylist.Songs[0];
                displayControls();
            }
        }

        private async void LoopButton_Click(object sender, RoutedEventArgs e)
        {
            if(LoopButton.Content.ToString() == "Loop: Off")
            {
                isLoop = true;
                LoopButton.Content = "Loop: On";
                LoopButton.Background = Brushes.DarkGreen;
                if (!isShuffle && curSong != null)
                {
                    if (currentIndex == 0)
                    {
                        prevSong = AccountManager.instance.CurrentPlaylist.Songs[AccountManager.instance.CurrentPlaylist.Songs.Count - 1];
                    }
                    if (currentIndex == AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                    {
                        nextSong = AccountManager.instance.CurrentPlaylist.Songs[0];
                    }
                    displayControls();
                }
                else if(isShuffle && curSong != null)
                {
                    await randomGeneratorAsync();
                    displayShuffleControls();
                }
            }
            else if(LoopButton.Content.ToString() == "Loop: On")
            {
                isLoop = false;
                LoopButton.Content = "Loop: Off";
                LoopButton.Background = Brushes.Black;
                if(!isShuffle && curSong != null)
                {
                    if (currentIndex == 0)
                    {
                        prevSong = null;
                    }
                    if (currentIndex == AccountManager.instance.CurrentPlaylist.Songs.Count - 1)
                    {
                        nextSong = null;
                    }
                    displayControls();
                }
                else if(isShuffle && curSong != null)
                {
                    await randomGeneratorAsync();
                    displayShuffleControls();
                }
            }
        }
    }
}
