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

using ListBox = System.Windows.Controls.ListBox;

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
    }
}
