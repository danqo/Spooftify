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
using System.Windows.Shapes;
using System.IO;
using WMPLib;
using AxWMPLib;
using System.Windows.Forms;
using TextBox = System.Windows.Controls.TextBox;
using MessageBox = System.Windows.MessageBox;
using ListBox = System.Windows.Controls.ListBox;
using System.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for AfterLogin.xaml
    /// </summary>
    public partial class AfterLogin : Window
    {
        public static string songName;
        public static ListBox selectedListBox;
        public static Playlist selectedPlaylist;
        public static List<Song> mediaFileList;
        public static User objectUser;
        public static AfterLogin afterLoginWindow;
        public static AxWMPLib.AxWindowsMediaPlayer ax;
        public static StackPanel AllPLaylist;
        public static List<System.Windows.Controls.ListBox> ListofListBox ;
        public static Expander currentExpander;
        string mediaFolder = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "MusicLibrary");

        public string currMediaName = "";
        public string currPlaylistName = "";

        public AfterLogin(User x)
        {
            ListofListBox = new List<System.Windows.Controls.ListBox>();
            afterLoginWindow = this;
            var newBox = new System.Windows.Controls.ListBox();

            
            objectUser = new User();
            mediaFileList = new List<Song>();
            InitializeComponent();
            username.Content = x.mUsername;
            ax =
                winsFormHost.Child as AxWMPLib.AxWindowsMediaPlayer;
            objectUser = x;
            profile.Source = new BitmapImage(new Uri("pack://application:,,,/Images/" + x.mUsername + ".png"));
            /*
            DirectoryInfo dir = new DirectoryInfo(mediaFolder);

            foreach (FileInfo file in dir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if (file.Extension == ".mp3" || file.Extension == ".mp4")
                    mediaFileList.Add(file.Name);
            }
            */
            foreach (var b in LoginScreen.allSongs.mSongs)
                mediaFileList.Add(b);
        
            /*if (mediaFileList != null)
            {
                newBox.ItemsSource = mediaFileList;
                ax.URL = mediaFolder + "\\" + mediaFileList[0];
            }
            var newstackPanel = new StackPanel { Name = "NewExpanderStackPanel" };
            newstackPanel.Children.Add(newBox);
            Expander exp = new Expander();
            exp.Content = newstackPanel;
            exp.Header = "Available Songs";
            exp.Foreground = Brushes.LightGray;
            allPlaylist.Children.Add(exp);
            newBox.Background = Brushes.Black;
            newBox.Foreground = Brushes.LightGray;
            foreach (var b in objectUser.mPlaylists)
            {
                Expander exp1 = new Expander();
                var newStack = new StackPanel { Name = "NewExpanderStackPanel" };
                var newListBox = new System.Windows.Controls.ListBox();
                newListBox.Background = Brushes.Black;
                newListBox.Foreground = Brushes.LightGray;
                var newMediaFileList = new List<string>();
                foreach (var d in b.mSongs)
                    newMediaFileList.Add(d.mArtist + "-" + d.mTitle + d.mExtension);
                newListBox.ItemsSource = newMediaFileList;
                newStack.Children.Add(newListBox);
                exp1.Content = newStack;
                exp1.Header = b.mName;
                exp1.Foreground = Brushes.LightGray;
                allPlaylist.Children.Add(exp1);

            }
            AllPLaylist = allPlaylist;*/
            AllPLaylist = new StackPanel();
            reloadPlaylists();
        }

        public void reloadPlaylists()
        {
            AllPLaylist.Children.Clear();
            /*
            var newBox = new System.Windows.Controls.ListBox();
            newBox.Tag = "Available Songs";
            if (mediaFileList != null)
            {
                newBox.ItemsSource = mediaFileList;
                //ax.URL = mediaFolder + "\\" + mediaFileList[0];
            }
            var newstackPanel = new StackPanel();
            newstackPanel.Children.Add(newBox);
            Expander exp = new Expander();
            exp.Content = newstackPanel;
            exp.Header = "Available Songs";
            
            exp.Foreground = Brushes.LightGray;
            allPlaylist.Children.Add(exp);
            newBox.Background = Brushes.Black;
            newBox.Foreground = Brushes.LightGray;
            newBox.Tag = "Available Songs";
            */
            foreach (var b in objectUser.mPlaylists)
            {
                Expander exp1 = new Expander();
                exp1.MouseRightButtonUp += Mouse_RightButtonClickPlaylist;
                var newStack = new StackPanel ();
                var newListBox = new System.Windows.Controls.ListBox();
                newListBox.Background = Brushes.Black;
                newListBox.Foreground = Brushes.LightGray;
                var newMediaFileList = new List<Song>();
                foreach (var d in b.mSongs)
                    newMediaFileList.Add(d);
                newListBox.ItemsSource = newMediaFileList;
                newListBox.Tag = b.mName;
                ListofListBox.Add(newListBox);
                newStack.Children.Add(newListBox);
                exp1.Content = newStack;
                exp1.Header = b.mName;
                exp1.Foreground = Brushes.LightGray;
                allPlaylist.Children.Add(exp1);
                

            }
            AllPLaylist = allPlaylist;
        }
        private void Mouse_RightButtonClickPlaylist(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(Expander))
            {
                if ((e.OriginalSource.GetType() == typeof(TextBlock) || e.OriginalSource.GetType() == typeof(Grid)) && ((TextBlock)e.OriginalSource).DataContext.GetType() != typeof(Song))
                {
                    selectedPlaylist = objectUser.mPlaylists.Where(x => x.mName == (string)((TextBlock)e.OriginalSource).DataContext).Single();
                    System.Windows.Controls.Expander ctrl = ((System.Windows.Controls.Expander)sender);
                    Brush highlight = new SolidColorBrush(Colors.Green);
                    ctrl.Background = highlight;
                    ContextMenuStrip cMS = new ContextMenuStrip();
                    cMS.Name = "Playlist Control";
                    ToolStripMenuItem addSong = new ToolStripMenuItem("Add Songs");
                    ToolStripMenuItem renamePlaylist = new ToolStripMenuItem("Rename");
                    ToolStripMenuItem removePlaylist = new ToolStripMenuItem("Remove Playlist");
                    //addSong.Click += AddSongClick;
                    renamePlaylist.Click += RenamePlaylistClick;
                    removePlaylist.Click += RemovePlaylistClick;

                    System.Windows.Controls.Button b2 = new System.Windows.Controls.Button();

                    addSong.DropDownItems.Add("Search");
                    addSong.DropDownItems[0].Click += AddSongClick;
                    cMS.Items.Add(addSong);
                    int i = 0;
                    /*
                    foreach (var b in LoginScreen.allSongs.mSongs)
                    {

                        if (selectedPlaylist.mSongs.FirstOrDefault(x=> x.ToString() == b.ToString()) == null)
                        {
                            (cMS.Items[0] as ToolStripMenuItem).DropDownItems.Add(b.ToString());
                            (cMS.Items[0] as ToolStripMenuItem).DropDownItems[i].Click += AddSongClick;
                            i++;
                        }
                    }*/
                    cMS.Items.Add(renamePlaylist);
                    cMS.Items.Add(removePlaylist);
                    System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
                    cMS.Show(pt);
                    cMS.Closed += RightClickMenu_Closed;
                    currentExpander = ctrl;
                }
                if (e.OriginalSource.GetType() == typeof(TextBlock) && ((TextBlock)e.OriginalSource).DataContext.GetType() == typeof(Song))
                {
                    selectedPlaylist = objectUser.mPlaylists.Where(x => x.mName == (string) ((Expander)sender).Header).Single(); 
                    System.Windows.Controls.Expander ctrl = ((System.Windows.Controls.Expander)sender);
                    Brush highlight = new SolidColorBrush(Colors.Green);
                    ctrl.Background = highlight;
                    ContextMenuStrip cMS = new ContextMenuStrip();
                    ToolStripMenuItem removeSong = new ToolStripMenuItem("Remove");
                    removeSong.Click += RemoveSong_Click;
                    removeSong.Name = ((TextBlock)e.OriginalSource).DataContext.ToString();
                    cMS.Items.Add(removeSong);
                    System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
                    cMS.Show(pt);
                    cMS.Closed += RightClickMenu_Closed;
                }


            }
            if(sender.GetType() == typeof(StackPanel) && e.OriginalSource.GetType() == typeof(StackPanel))
            {
                ContextMenuStrip cMS = new ContextMenuStrip();
                cMS.Name = "Playlist Control";
                ToolStripMenuItem addPlaylist = new ToolStripMenuItem("Add PlayList");
                addPlaylist.Click += AddPlaylist_Click;
                cMS.Items.Add(addPlaylist);
                System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
                cMS.Show(pt);
            }
        }

        private void RemoveSong_Click(object sender, EventArgs e)
        {
            if (objectUser.mPlaylists.Where(x => x.mName == selectedPlaylist.mName).Single().mSongs.Where(x => x.ToString() == ((ToolStripMenuItem)sender).Name).Single().ToString()
                 == ax.currentMedia.name)
                ax.close();
            objectUser.mPlaylists.Where(x => x.mName == selectedPlaylist.mName).Single().mSongs.Remove
                (objectUser.mPlaylists.Where(x => x.mName == selectedPlaylist.mName).Single().mSongs.Where(x => x.ToString() == ((ToolStripMenuItem)sender).Name).Single());
            
            List<Song> temp = new List<Song>();
            //bug here 2 playlists delete a playlist
            foreach (var b in selectedPlaylist.mSongs)
                temp.Add(b);
            AfterLogin.ListofListBox.Where(x => (string)x.Tag == selectedPlaylist.mName).Single().ItemsSource = temp;
        }

        private void RightClickMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            ContextMenuStrip cMS = (ContextMenuStrip)sender;
            Brush unhighlight = new SolidColorBrush(Colors.Black);
            if(currentExpander != null)
                currentExpander.Background = unhighlight;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count !=0)
            {
                songName = ((Song)e.AddedItems[0]).ToString();
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
                /*
                currPlaylistName = ((System.Windows.Controls.ListBox)e.OriginalSource).Tag.ToString();
                Playlist currentPL = objectUser.mPlaylists.Where(x => currPlaylistName.Equals(x.mName)).DefaultIfEmpty(null).Single();
                if (currentPL != null)
                {
                    var wmpPL = ax.playlistCollection.newPlaylist("Current Playlist - " + currPlaylistName);
                    var startMedia = ax.newMedia(mediaFolder + "\\" + ((Song)e.AddedItems[0]).Directory);
                    startMedia.name = ((Song)e.AddedItems[0]).ToString();
                    wmpPL.appendItem(startMedia);
                    foreach (Song cur in currentPL.mSongs)
                    {
                        if (!cur.mTitle.Equals(((Song)e.AddedItems[0]).mTitle))
                        {
                            var fileLocation = mediaFolder + "\\" + cur.Directory;
                            var mediaItem = ax.newMedia(fileLocation);
                            wmpPL.appendItem(mediaItem);
                        }
                    }
                    ax.currentPlaylist = wmpPL;
                   
                }
                
                

                //ax.URL = mediaFolder + "\\" + ((Song)e.AddedItems[0]).Directory;
                //currMediaName = ((Song)e.AddedItems[0]).Directory;
                */
            }
        }

        private void Button_Profile(object sender, RoutedEventArgs e)
        {
            
            afterLoginWindow = this;
            Profile b = new Profile();
            this.Hide();
            b.Show();
        }

        private void Button_Logout(object sender, RoutedEventArgs e)
        {
            ax.close();
            mediaFileList.Clear();
            this.Hide();
            LoginScreen.LoginWindow.Show();
        }

        private void RenamePlaylistClick(object sender, EventArgs e)
        {
            
            System.Windows.Forms.MessageBox.Show("Have a nice day.");
        }

        private void AddSongClick(object sender, EventArgs e)
        {
            selectedListBox= ListofListBox.Where(x => (string)x.Tag == selectedPlaylist.mName).Single();
            SearchWindow b = new SearchWindow();
            b.Show();
            /*
            objectUser.mPlaylists.Where(x=> x.mName == selectedPlaylist.mName).Single().mSongs.Add(LoginScreen.allSongs.mSongs.Where(x=> x.ToString() ==((ToolStripMenuItem)sender).Text).Single());
            List<Song> temp = new List<Song>();
            //bug here 2 playlists delete a playlist
            foreach (var b in selectedPlaylist.mSongs)
                temp.Add(b);
            AfterLogin.ListofListBox.Where(x => (string)x.Tag == selectedPlaylist.mName).Single().ItemsSource = temp;
            */
        }

        private void RemovePlaylistClick(object sender, EventArgs e)
        {
            objectUser.mPlaylists.Remove(selectedPlaylist);
            foreach(var b in allPlaylist.Children)
            {
                if ((string)((Expander)b).Header == selectedPlaylist.mName)
                {
                    allPlaylist.Children.Remove((Expander)b);
                    break;
                }
            }
            
            ListofListBox.Remove(ListofListBox.Where(x => (string)x.Tag ==  selectedPlaylist.mName).Single());
        }

        private void OpenAddingPlayList(object sender, RoutedEventArgs e)
        {
            afterLoginWindow = this;
            Create b = new Create();
            this.Hide();
            b.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!LoginScreen.LoginWindow.closingFlag)
            {
                LoginScreen.LoginWindow.ConfirmExit();
            }
            if (LoginScreen.LoginWindow.closingFlag)
            {
                App.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void allPlaylist_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

      

        private void AddPlaylist_Click(object sender, EventArgs e)
        {
            TextBox newPlayList = new TextBox();
            
            newPlayList.KeyUp += NewPlayList_KeyUp;
            allPlaylist.Children.Add(newPlayList);
            newPlayList.Focus();
        }

        private void NewPlayList_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
            if (e.Key == Key.Enter)
            {
                var d = (TextBox)sender;
                if (AfterLogin.objectUser.mPlaylists.FirstOrDefault(x => x.mName == d.Text) == null && d.Text != "")
                {
                    
                    
                    ListBoxItem itm = new ListBoxItem();
                    itm.Content = d.Text;
                    Expander exp1 = new Expander();
                    exp1.MouseRightButtonUp += Mouse_RightButtonClickPlaylist;
                    var newStack = new StackPanel();
                    var newListBox = new System.Windows.Controls.ListBox();
                    newListBox.Background = Brushes.Black;
                    newListBox.Foreground = Brushes.LightGray;
                    //var newMediaFileList = new List<Song>();
                    //foreach (var d in b.mSongs)
                    //  newMediaFileList.Add(d);
                    //newListBox.ItemsSource = newMediaFileList;
                    newListBox.Tag = d.Text;
                    ListofListBox.Add(newListBox);
                    newStack.Children.Add(newListBox);
                    exp1.Content = newStack;
                    exp1.Header = d.Text;
                    exp1.Foreground = Brushes.LightGray;
                    allPlaylist.Children.Add(exp1);
                    AfterLogin.objectUser.mPlaylists.Add(new Playlist(d.Text));
                }
                else
                    MessageBox.Show("Failed to create a playlist. Playlist name is already exist");

                foreach (var b in allPlaylist.Children)
                {
                    if (b.GetType() == typeof(TextBox))
                    {
                        allPlaylist.Children.Remove((TextBox)b);
                        break;
                    }
                }
            }
            if (e.Key == Key.Escape)
            {

                foreach (var b in allPlaylist.Children)
                {
                    if (b.GetType() == typeof(TextBox))
                    {
                        allPlaylist.Children.Remove((TextBox)b);
                        break;
                    }
                }
            }
        }

        private void StopPlayingClick(object sender, RoutedEventArgs e)

        {
            if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing || SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                SocketClientOut.buffering = false;
                SocketClientOut.stopSong();
                stopPlay.Content = "Play";
            }
            else if(SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
            {
                SocketClientOut.buffering = true;
                SocketClientOut.sendActionRequest(Encoding.ASCII.GetBytes("playMusic"));
                SocketClientOut.sendSongName(Encoding.ASCII.GetBytes(songName));
                //SocketClientOut.sendSongName(Encoding.ASCII.GetBytes("haha"));
                var msg = Encoding.ASCII.GetString(SocketClientOut.receiveAccess());
                if (msg == "granted")
                {
                    ThreadStart receiveStart = new ThreadStart(SocketClientOut.receivingSong);
                    Thread receiveThread = new Thread(receiveStart);
                    receiveThread.Start();
                    //SocketClientOut.playSong();
                }
                else
                {
                    MessageBox.Show(msg);
                }
                stopPlay.Content = "Stop";

            }
        }

        private void PausePlayingClick(object sender, RoutedEventArgs e)
        {
            if (SocketClientOut.waveOut != null)
            {
                if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                {
                    SocketClientOut.buffering = false;
                    SocketClientOut.pauseSong();
                    pauseResume.Content = "Resume";
                }
                else if (SocketClientOut.waveOut.PlaybackState == NAudio.Wave.PlaybackState.Paused)
                {
                    SocketClientOut.buffering = true;
                    ThreadStart receiveStart = new ThreadStart(SocketClientOut.resumeSong);
                    Thread receiveThread = new Thread(receiveStart);
                    receiveThread.Start();
                    pauseResume.Content = "Pause";
                }
            }
        
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        /**
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow sW = new SearchWindow();
            this.Hide();
            sW.Show();
        }*/
    }
}
