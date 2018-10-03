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
        public SearchPage()
        {
            InitializeComponent();
            LoadAllSongs();
        }

        private void LoadAllSongs()
        {
            var newMediaFileList = new List<Song>();
            foreach (var b in Login.allSongs.Songs)
            {
                ListBoxItem itm = new ListBoxItem();
                itm.Content = b;
                itm.Foreground = Brushes.LightGray;
                itm.FontSize = 24;

                SearchListBox.Items.Add(itm);

            }

        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Tag != null)
            {
                textBox.Tag = (!String.IsNullOrWhiteSpace(textBox.Text)).ToString();
            }
        }

        private void SearchByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // check combobox contents with SelectedItem property
        }

        public void Reset()
        {
            AddSongMsg.Visibility = Visibility.Hidden;
            SearchTextBox.Text = "";
        }

        private void SearchListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                string songName = ((ListBoxItem)e.AddedItems[0]).Content.ToString();
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
            }
        }
    }
}
