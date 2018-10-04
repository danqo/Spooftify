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
using System.Windows.Forms;
using WpfApp1;
using ListBox = System.Windows.Controls.ListBox;
using System.Threading;

namespace Spooftify
{
    /// <summary>
    /// Interaction logic for PlayPage.xaml
    /// </summary>
    public partial class PlayPage : Page
    {
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
            var b = ((ListBox)sender).SelectedItem as Song;
            if (b != null)
            {
                ContextMenuStrip cMS = new ContextMenuStrip();
                cMS.Name = "Playlist Control";
                ToolStripMenuItem RemoveSong = new ToolStripMenuItem("Remove");
                RemoveSong.Tag = b;
                //RemoveSong.Tag
                RemoveSong.Click += RemoveSongCLick;
                cMS.Items.Add(RemoveSong);
                System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
                cMS.Show(pt);
            }

        }

        private void RemoveSongCLick(object sender, EventArgs e)
        {
            var b = sender as ToolStripMenuItem;
            AccountManager.instance.CurrentPlaylist.deleteSong((Song)b.Tag);
            SongListbox.ItemsSource = AccountManager.instance.CurrentPlaylist.Songs;
            SongListbox.Items.Refresh();
            //selectedListBox = ListofListBox.Where(x => (string)x.Tag == selectedPlaylist.mName).Single();
            //SearchWindow b = new SearchWindow();
            //b.Show();
        }

        private void SongListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

        }

        private void SongListbox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
                
                if (SocketClientOut.waveOut == null)
                {
                    var b = sender as ListBox;
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
                var b = sender as ListBox;
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
}
