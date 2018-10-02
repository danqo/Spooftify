using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private List<Panel> songObjs = new List<Panel>();
        private Playlist currentPlayList;
        private List<string> songStr = new List<string>();
        public SearchWindow()
        {
            InitializeComponent();
            //listTitle.Content = "Adding to " + Create.selectedList.ToString() + "playlist";
            
            currentPlayList = AfterLogin.objectUser.mPlaylists.Where(x => x.mName == AfterLogin.selectedPlaylist.mName).Single();
            PlaylistName.Content = "Adding songs to: " + currentPlayList.mName + " (select a song and click add)";
            foreach (Song x in LoginScreen.allSongs.mSongs)
            {
                StackPanel sP = new StackPanel();
                sP.Orientation = Orientation.Horizontal;
                Label titleLbl = new Label();
                Label artistLbl = new Label();
                Label albumLbl = new Label();
                Label extensionLbl = new Label();
                titleLbl.Content = " Song Title: ";
                artistLbl.Content = " Artist(s): ";
                albumLbl.Content = " Album: ";
                extensionLbl.Content = " File Type: ";
                titleLbl.FontWeight = FontWeights.Bold;
                artistLbl.FontWeight = FontWeights.Bold;
                albumLbl.FontWeight = FontWeights.Bold;
                extensionLbl.FontWeight = FontWeights.Bold;
                Label dspTitle = new Label();
                Label dspArtist = new Label();
                Label dspAlbum = new Label();
                Label dspExtension = new Label();
                dspTitle.Content = x.mTitle;
                dspArtist.Content = x.mArtist;
                dspAlbum.Content = x.mAlbum;
                dspExtension.Content = x.mExtension;
                sP.Children.Add(titleLbl);
                sP.Children.Add(dspTitle);
                sP.Children.Add(artistLbl);
                sP.Children.Add(dspArtist);
                sP.Children.Add(albumLbl);
                sP.Children.Add(dspAlbum);
                sP.Children.Add(extensionLbl);
                sP.Children.Add(dspExtension);
                
                Label statusLbl = new Label();
                statusLbl.Content = " Status: ";
                statusLbl.FontWeight = FontWeights.Bold;
                Label dspStatusNeg = new Label();
                Label dspStatusPos = new Label();
                sP.Children.Add(statusLbl);
                string trythis = x.mArtist + "-" + x.mTitle + x.mExtension;
                if (AfterLogin.selectedPlaylist.mSongs.FirstOrDefault(y => (y.mArtist + "-" + y.mTitle + y.mExtension) == trythis) != null)
                {
                    dspStatusPos.Content = "In Playlist";
                    sP.Children.Add(dspStatusPos);
                }
                else
                {
                    dspStatusNeg.Content = "Not In Playlist";
                    sP.Children.Add(dspStatusNeg);
                }
                songObjs.Add(sP);
                ListBox NewListBox = new ListBox();
                
                songStr.Add(" Song Title: " + x.mTitle + " Artist(s): " + x.mArtist + " Album: " + x.mAlbum + " File Type: " + x.mExtension);

            }
            Titles.ItemsSource = songObjs;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            string searchVal = SearchText.Text;
            
            if (SearchBy.SelectedIndex == 0)
            {
                foreach(Song x in LoginScreen.allSongs.mSongs)
                {
                    if (x.mTitle.ToLower().Contains(searchVal.ToLower()))
                    {
                        StackPanel sP = new StackPanel();
                        sP.Orientation = Orientation.Horizontal;
                        Label titleLbl = new Label();
                        Label artistLbl = new Label();
                        Label albumLbl = new Label();
                        Label extensionLbl = new Label();
                        titleLbl.Content = " Song Title: ";
                        artistLbl.Content = " Artist(s): ";
                        albumLbl.Content = " Album: ";
                        extensionLbl.Content = " File Type: ";
                        titleLbl.FontWeight = FontWeights.Bold;
                        artistLbl.FontWeight = FontWeights.Bold;
                        albumLbl.FontWeight = FontWeights.Bold;
                        extensionLbl.FontWeight = FontWeights.Bold;
                        Label dspTitle = new Label();
                        Label dspArtist = new Label();
                        Label dspAlbum = new Label();
                        Label dspExtension = new Label();
                        dspTitle.Content = x.mTitle;
                        dspArtist.Content = x.mArtist;
                        dspAlbum.Content = x.mAlbum;
                        dspExtension.Content = x.mExtension;
                        sP.Children.Add(titleLbl);
                        sP.Children.Add(dspTitle);
                        sP.Children.Add(artistLbl);
                        sP.Children.Add(dspArtist);
                        sP.Children.Add(albumLbl);
                        sP.Children.Add(dspAlbum);
                        sP.Children.Add(extensionLbl);
                        sP.Children.Add(dspExtension);
                        Label statusLbl = new Label();
                        statusLbl.Content = " Status: ";
                        statusLbl.FontWeight = FontWeights.Bold;
                        Label dspStatusNeg = new Label();
                        Label dspStatusPos = new Label();
                        sP.Children.Add(statusLbl);
                        string trythis = x.mArtist + "-" + x.mTitle + x.mExtension;
                        if (AfterLogin.selectedPlaylist.mSongs.FirstOrDefault(y => (y.mArtist + "-" + y.mTitle + y.mExtension) == trythis) != null)
                        {
                            dspStatusPos.Content = "In Playlist";
                            sP.Children.Add(dspStatusPos);
                        }
                        else
                        {
                            dspStatusNeg.Content = "Not In Playlist";
                            sP.Children.Add(dspStatusNeg);
                        }
                        songObjs.Add(sP);
                        songStr.Add(" Song Title: " + x.mTitle + " Artist(s): " + x.mArtist + " Album: " + x.mAlbum + " File Type: " + x.mExtension);
                    }
                }
            }
            else if (SearchBy.SelectedIndex == 1)
            {
                foreach (Song x in LoginScreen.allSongs.mSongs)
                {
                    if (x.mArtist.ToLower().Contains(searchVal.ToLower()))
                    {
                        StackPanel sP = new StackPanel();
                        sP.Orientation = Orientation.Horizontal;
                        Label titleLbl = new Label();
                        Label artistLbl = new Label();
                        Label albumLbl = new Label();
                        Label extensionLbl = new Label();
                        titleLbl.Content = " Song Title: ";
                        artistLbl.Content = " Artist(s): ";
                        albumLbl.Content = " Album: ";
                        extensionLbl.Content = " File Type: ";
                        titleLbl.FontWeight = FontWeights.Bold;
                        artistLbl.FontWeight = FontWeights.Bold;
                        albumLbl.FontWeight = FontWeights.Bold;
                        extensionLbl.FontWeight = FontWeights.Bold;
                        Label dspTitle = new Label();
                        Label dspArtist = new Label();
                        Label dspAlbum = new Label();
                        Label dspExtension = new Label();
                        dspTitle.Content = x.mTitle;
                        dspArtist.Content = x.mArtist;
                        dspAlbum.Content = x.mAlbum;
                        dspExtension.Content = x.mExtension;
                        sP.Children.Add(titleLbl);
                        sP.Children.Add(dspTitle);
                        sP.Children.Add(artistLbl);
                        sP.Children.Add(dspArtist);
                        sP.Children.Add(albumLbl);
                        sP.Children.Add(dspAlbum);
                        sP.Children.Add(extensionLbl);
                        sP.Children.Add(dspExtension);
                        Label statusLbl = new Label();
                        statusLbl.Content = " Status: ";
                        statusLbl.FontWeight = FontWeights.Bold;
                        Label dspStatusNeg = new Label();
                        Label dspStatusPos = new Label();
                        sP.Children.Add(statusLbl);
                        string trythis = x.mArtist + "-" + x.mTitle + x.mExtension;
                        if (AfterLogin.selectedPlaylist.mSongs.FirstOrDefault(y => (y.mArtist + "-" + y.mTitle + y.mExtension) == trythis) != null)
                        {
                            dspStatusPos.Content = "In Playlist";
                            sP.Children.Add(dspStatusPos);
                        }
                        else
                        {
                            dspStatusNeg.Content = "Not In Playlist";
                            sP.Children.Add(dspStatusNeg);
                        }
                        songObjs.Add(sP);
                        songStr.Add(" Song Title: " + x.mTitle + " Artist(s): " + x.mArtist + " Album: " + x.mAlbum + " File Type: " + x.mExtension);
                    }
                }
            }
            else if (SearchBy.SelectedIndex == 2)
            {
                foreach (Song x in LoginScreen.allSongs.mSongs)
                {
                    if (x.mAlbum.ToLower().Contains(searchVal.ToLower()))
                    {
                        StackPanel sP = new StackPanel();
                        sP.Orientation = Orientation.Horizontal;
                        Label titleLbl = new Label();
                        Label artistLbl = new Label();
                        Label albumLbl = new Label();
                        Label extensionLbl = new Label();
                        titleLbl.Content = " Song Title: ";
                        artistLbl.Content = " Artist(s): ";
                        albumLbl.Content = " Album: ";
                        extensionLbl.Content = " File Type: ";
                        titleLbl.FontWeight = FontWeights.Bold;
                        artistLbl.FontWeight = FontWeights.Bold;
                        albumLbl.FontWeight = FontWeights.Bold;
                        extensionLbl.FontWeight = FontWeights.Bold;
                        Label dspTitle = new Label();
                        Label dspArtist = new Label();
                        Label dspAlbum = new Label();
                        Label dspExtension = new Label();
                        dspTitle.Content = x.mTitle;
                        dspArtist.Content = x.mArtist;
                        dspAlbum.Content = x.mAlbum;
                        dspExtension.Content = x.mExtension;
                        sP.Children.Add(titleLbl);
                        sP.Children.Add(dspTitle);
                        sP.Children.Add(artistLbl);
                        sP.Children.Add(dspArtist);
                        sP.Children.Add(albumLbl);
                        sP.Children.Add(dspAlbum);
                        sP.Children.Add(extensionLbl);
                        sP.Children.Add(dspExtension);
                        Label statusLbl = new Label();
                        statusLbl.Content = " Status: ";
                        statusLbl.FontWeight = FontWeights.Bold;
                        Label dspStatusNeg = new Label();
                        Label dspStatusPos = new Label();
                        sP.Children.Add(statusLbl);
                        string trythis = x.mArtist + "-" + x.mTitle + x.mExtension;
                        if (AfterLogin.selectedPlaylist.mSongs.FirstOrDefault(y => (y.mArtist + "-" + y.mTitle + y.mExtension) == trythis) != null)
                        {
                            dspStatusPos.Content = "In Playlist";
                            sP.Children.Add(dspStatusPos);
                        }
                        else
                        {
                            dspStatusNeg.Content = "Not In Playlist";
                            sP.Children.Add(dspStatusNeg);
                        }
                        songObjs.Add(sP);
                        songStr.Add(" Song Title: " + x.mTitle + " Artist(s): " + x.mArtist + " Album: " + x.mAlbum + " File Type: " + x.mExtension);

                    }
                }
            }
            if (songObjs.Count != 0)
            {
                Titles.ItemsSource = songObjs;
            }
            else
            {
                StackPanel sP = new StackPanel();
                Label noRes = new Label();
                noRes.Content = "No results found :(";
                songStr.Add("No results found :(");
                sP.Children.Add(noRes);
                songObjs.Add(sP);
                Titles.ItemsSource = songObjs;
                Titles.IsEnabled = false;
            }
        }

        private void ComboBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                Reset();
                string searchVal = SearchText.Text;
                if (SearchBy.SelectedIndex == 0)
                {
                    foreach (Song x in LoginScreen.allSongs.mSongs)
                    {
                        if (x.mTitle.ToLower().Contains(searchVal.ToLower()))
                        {
                            StackPanel sP = new StackPanel();
                            sP.Orientation = Orientation.Horizontal;
                            Label titleLbl = new Label();
                            Label artistLbl = new Label();
                            Label albumLbl = new Label();
                            Label extensionLbl = new Label();
                            titleLbl.Content = " Song Title: ";
                            artistLbl.Content = " Artist(s): ";
                            albumLbl.Content = " Album: ";
                            extensionLbl.Content = " File Type: ";
                            titleLbl.FontWeight = FontWeights.Bold;
                            artistLbl.FontWeight = FontWeights.Bold;
                            albumLbl.FontWeight = FontWeights.Bold;
                            extensionLbl.FontWeight = FontWeights.Bold;
                            Label dspTitle = new Label();
                            Label dspArtist = new Label();
                            Label dspAlbum = new Label();
                            Label dspExtension = new Label();
                            dspTitle.Content = x.mTitle;
                            dspArtist.Content = x.mArtist;
                            dspAlbum.Content = x.mAlbum;
                            dspExtension.Content = x.mExtension;
                            sP.Children.Add(titleLbl);
                            sP.Children.Add(dspTitle);
                            sP.Children.Add(artistLbl);
                            sP.Children.Add(dspArtist);
                            sP.Children.Add(albumLbl);
                            sP.Children.Add(dspAlbum);
                            sP.Children.Add(extensionLbl);
                            sP.Children.Add(dspExtension);
                            Label statusLbl = new Label();
                            statusLbl.Content = " Status: ";
                            statusLbl.FontWeight = FontWeights.Bold;
                            Label dspStatusNeg = new Label();
                            Label dspStatusPos = new Label();
                            sP.Children.Add(statusLbl);
                            string trythis = x.mArtist + "-" + x.mTitle + x.mExtension;
                            if (AfterLogin.selectedPlaylist.mSongs.FirstOrDefault(y => (y.mArtist + "-" + y.mTitle + y.mExtension) == trythis) != null)
                            {
                                dspStatusPos.Content = "In Playlist";
                                sP.Children.Add(dspStatusPos);
                            }
                            else
                            {
                                dspStatusNeg.Content = "Not In Playlist";
                                sP.Children.Add(dspStatusNeg);
                            }
                            songObjs.Add(sP);
                            songStr.Add(" Song Title: " + x.mTitle + " Artist(s): " + x.mArtist + " Album: " + x.mAlbum + " File Type: " + x.mExtension);

                        }
                    }
                }
                else if (SearchBy.SelectedIndex == 1)
                {
                    foreach (Song x in LoginScreen.allSongs.mSongs)
                    {
                        if (x.mArtist.ToLower().Contains(searchVal.ToLower()))
                        {
                            StackPanel sP = new StackPanel();
                            sP.Orientation = Orientation.Horizontal;
                            Label titleLbl = new Label();
                            Label artistLbl = new Label();
                            Label albumLbl = new Label();
                            Label extensionLbl = new Label();
                            titleLbl.Content = " Song Title: ";
                            artistLbl.Content = " Artist(s): ";
                            albumLbl.Content = " Album: ";
                            extensionLbl.Content = " File Type: ";
                            titleLbl.FontWeight = FontWeights.Bold;
                            artistLbl.FontWeight = FontWeights.Bold;
                            albumLbl.FontWeight = FontWeights.Bold;
                            extensionLbl.FontWeight = FontWeights.Bold;
                            Label dspTitle = new Label();
                            Label dspArtist = new Label();
                            Label dspAlbum = new Label();
                            Label dspExtension = new Label();
                            dspTitle.Content = x.mTitle;
                            dspArtist.Content = x.mArtist;
                            dspAlbum.Content = x.mAlbum;
                            dspExtension.Content = x.mExtension;
                            sP.Children.Add(titleLbl);
                            sP.Children.Add(dspTitle);
                            sP.Children.Add(artistLbl);
                            sP.Children.Add(dspArtist);
                            sP.Children.Add(albumLbl);
                            sP.Children.Add(dspAlbum);
                            sP.Children.Add(extensionLbl);
                            sP.Children.Add(dspExtension);
                            Label statusLbl = new Label();
                            statusLbl.Content = " Status: ";
                            statusLbl.FontWeight = FontWeights.Bold;
                            Label dspStatusNeg = new Label();
                            Label dspStatusPos = new Label();
                            sP.Children.Add(statusLbl);
                            string trythis = x.mArtist + "-" + x.mTitle + x.mExtension;
                            if (AfterLogin.selectedPlaylist.mSongs.FirstOrDefault(y => (y.mArtist + "-" + y.mTitle + y.mExtension) == trythis) != null)
                            {
                                dspStatusPos.Content = "In Playlist";
                                sP.Children.Add(dspStatusPos);
                            }
                            else
                            {
                                dspStatusNeg.Content = "Not In Playlist";
                                sP.Children.Add(dspStatusNeg);
                            }
                            songObjs.Add(sP);
                            songStr.Add(" Song Title: " + x.mTitle + " Artist(s): " + x.mArtist + " Album: " + x.mAlbum + " File Type: " + x.mExtension);

                        }
                    }
                }
                else if (SearchBy.SelectedIndex == 2)
                {
                    foreach (Song x in LoginScreen.allSongs.mSongs)
                    {
                        if (x.mAlbum.ToLower().Contains(searchVal.ToLower()))
                        {
                            StackPanel sP = new StackPanel();
                            sP.Orientation = Orientation.Horizontal;
                            Label titleLbl = new Label();
                            Label artistLbl = new Label();
                            Label albumLbl = new Label();
                            Label extensionLbl = new Label();
                            titleLbl.Content = " Song Title: ";
                            artistLbl.Content = " Artist(s): ";
                            albumLbl.Content = " Album: ";
                            extensionLbl.Content = " File Type: ";
                            titleLbl.FontWeight = FontWeights.Bold;
                            artistLbl.FontWeight = FontWeights.Bold;
                            albumLbl.FontWeight = FontWeights.Bold;
                            extensionLbl.FontWeight = FontWeights.Bold;
                            Label dspTitle = new Label();
                            Label dspArtist = new Label();
                            Label dspAlbum = new Label();
                            Label dspExtension = new Label();
                            dspTitle.Content = x.mTitle;
                            dspArtist.Content = x.mArtist;
                            dspAlbum.Content = x.mAlbum;
                            dspExtension.Content = x.mExtension;
                            sP.Children.Add(titleLbl);
                            sP.Children.Add(dspTitle);
                            sP.Children.Add(artistLbl);
                            sP.Children.Add(dspArtist);
                            sP.Children.Add(albumLbl);
                            sP.Children.Add(dspAlbum);
                            sP.Children.Add(extensionLbl);
                            sP.Children.Add(dspExtension);
                            Label statusLbl = new Label();
                            statusLbl.Content = " Status: ";
                            statusLbl.FontWeight = FontWeights.Bold;
                            Label dspStatusNeg = new Label();
                            Label dspStatusPos = new Label();
                            sP.Children.Add(statusLbl);
                            string trythis = x.mArtist + "-" + x.mTitle + x.mExtension;
                            if (AfterLogin.selectedPlaylist.mSongs.FirstOrDefault(y => (y.mArtist + "-" + y.mTitle + y.mExtension) == trythis) != null)
                            {
                                dspStatusPos.Content = "In Playlist";
                                sP.Children.Add(dspStatusPos);
                            }
                            else
                            {
                                dspStatusNeg.Content = "Not In Playlist";
                                sP.Children.Add(dspStatusNeg);
                            }
                            songObjs.Add(sP);
                            songStr.Add(" Song Title: " + x.mTitle + " Artist(s): " + x.mArtist + " Album: " + x.mAlbum + " File Type: " + x.mExtension);

                        }
                    }
                }
                if (songObjs.Count != 0)
                {
                    Titles.ItemsSource = songObjs;
                }
                else
                {
                    StackPanel sP = new StackPanel();
                    Label noRes = new Label();
                    noRes.Content = "No results found :(";
                    songStr.Add("No results found :(");
                    sP.Children.Add(noRes);
                    songObjs.Add(sP);
                    Titles.ItemsSource = songObjs;
                    Titles.IsEnabled = false;
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            
            
            SearchText.Text = "";
            Reset();
            this.Hide();
            //Adding.AddingWindow.Show();
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (Titles.SelectedItem != null && !songStr[Titles.SelectedIndex].Equals("No results found :("))
            {
                ListBoxItem itm = new ListBoxItem();
                Song b = LoginScreen.allSongs.mSongs.Where(x => (" Song Title: "+x.mTitle+" Artist(s): "+x.mArtist+" Album: "+x.mAlbum+" File Type: "+x.mExtension).Equals(songStr[Titles.SelectedIndex])).Single();
                string trythis = b.mArtist + "-" + b.mTitle + b.mExtension;
                
         
                if (AfterLogin.selectedPlaylist.mSongs.FirstOrDefault(x => (x.mArtist + "-" + x.mTitle + x.mExtension) == trythis) == null)
                {  
                    AfterLogin.selectedPlaylist.addSong(LoginScreen.allSongs.mSongs.Where(x => (x.mArtist + "-" + x.mTitle + x.mExtension) == trythis).Single());
                    
                    List<Song> temp = new List<Song>();
                    //bug here 2 playlists delete a playlist
                    foreach (var d in AfterLogin.selectedPlaylist.mSongs)
                        temp.Add(d);
                    AfterLogin.selectedListBox.ItemsSource = temp;
              
                    songObjs.RemoveAt(Titles.SelectedIndex);
                    songStr.RemoveAt(Titles.SelectedIndex);

                    StackPanel sP = new StackPanel();
                    sP.Orientation = Orientation.Horizontal;
                    Label titleLbl = new Label();
                    Label artistLbl = new Label();
                    Label albumLbl = new Label();
                    Label extensionLbl = new Label();
                    titleLbl.Content = " Song Title: ";
                    artistLbl.Content = " Artist(s): ";
                    albumLbl.Content = " Album: ";
                    extensionLbl.Content = " File Type: ";
                    titleLbl.FontWeight = FontWeights.Bold;
                    artistLbl.FontWeight = FontWeights.Bold;
                    albumLbl.FontWeight = FontWeights.Bold;
                    extensionLbl.FontWeight = FontWeights.Bold;
                    Label dspTitle = new Label();
                    Label dspArtist = new Label();
                    Label dspAlbum = new Label();
                    Label dspExtension = new Label();
                    dspTitle.Content = b.mTitle;
                    dspArtist.Content = b.mArtist;
                    dspAlbum.Content = b.mAlbum;
                    dspExtension.Content = b.mExtension;
                    sP.Children.Add(titleLbl);
                    sP.Children.Add(dspTitle);
                    sP.Children.Add(artistLbl);
                    sP.Children.Add(dspArtist);
                    sP.Children.Add(albumLbl);
                    sP.Children.Add(dspAlbum);
                    sP.Children.Add(extensionLbl);
                    sP.Children.Add(dspExtension);
                    Label statusLbl = new Label();
                    statusLbl.Content = " Status: ";
                    statusLbl.FontWeight = FontWeights.Bold;
                    Label dspStatusPos = new Label();
                    sP.Children.Add(statusLbl);
                    dspStatusPos.Content = "In playlist";
                    sP.Children.Add(dspStatusPos);
                    songObjs.Add(sP);
                    songStr.Add(" Song Title: " + b.mTitle + " Artist(s): " + b.mArtist + " Album: " + b.mAlbum + " File Type: " + b.mExtension);

                    Titles.ItemsSource = null;
                    Titles.ItemsSource = songObjs;

                    MessageBox.Show(b.mTitle+" has been added to "+currentPlayList.mName);
                }
                else
                    MessageBox.Show("Selected song is already added in the playlist");
            }
            else
            {
                MessageBox.Show("No songs were selected");
            }


        }

        private void Reset()
        {
            Titles.ItemsSource = null;
            songObjs.Clear();
            songStr.Clear();
            Titles.IsEnabled = true;
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

        private void Titles_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            Song b = new Song();
            if(Titles.SelectedIndex != -1 && !songStr[Titles.SelectedIndex].Equals("No results found :("))
                b= LoginScreen.allSongs.mSongs.Where(x => (" Song Title: " + x.mTitle + " Artist(s): " +
                 x.mArtist + " Album: " + x.mAlbum + " File Type: " + x.mExtension).Equals(songStr[Titles.SelectedIndex])).Single();
            
            if (currentPlayList.mSongs.FirstOrDefault(x => x.Directory == b.Directory) == null)
            {
                AddButton.IsEnabled = true;
                AddButton.Content = "Add";
            }
            else
            {
                AddButton.IsEnabled = false;
                AddButton.Content = "Added";
            }
           
        }
    }
}
