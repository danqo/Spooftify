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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Create.xaml
    /// </summary>
    public partial class Create : Window
    {
        public static Window createWIndow;
        public static string selectedList = "";
        
        public Create()
        {
            InitializeComponent();
            if (AfterLogin.objectUser.mPlaylists != null)
            {
                foreach (var b in AfterLogin.objectUser.mPlaylists)
                {
                    ListBoxItem itm = new ListBoxItem();
                    itm.Content = b.mName;
                    list.Items.Add(itm);
                }
            }
        }

        private void Button_Back(object sender, RoutedEventArgs e)
        {
            selectedList = "";
            list.Items.Clear();
            this.Hide();
            AfterLogin.afterLoginWindow.Show();
       
           

        }

        private void TextBlock_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Playlist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ListBoxItem itm = new ListBoxItem();
                itm.Content = newList.Text;

                if (AfterLogin.objectUser.mPlaylists.FirstOrDefault(x => x.mName == newList.Text) == null)
                {
                    AfterLogin.objectUser.mPlaylists.Add(new Playlist(newList.Text));
                    list.Items.Add(itm);
                }
                else
                    MessageBox.Show("Please choose a different name for the playlist");
                AfterLogin.afterLoginWindow.reloadPlaylists();

            }
        }
        
        private void AddButton(object sender, RoutedEventArgs e)
        {
            ListBoxItem itm = new ListBoxItem();
            if(!String.IsNullOrWhiteSpace(newList.Text))
            {
                itm.Content = newList.Text;

                if (AfterLogin.objectUser.mPlaylists.FirstOrDefault(x => x.mName == newList.Text) == null)
                {
                    AfterLogin.objectUser.mPlaylists.Add(new Playlist(newList.Text));
                    list.Items.Add(itm);
                }
                else
                    MessageBox.Show("Please choose a different name for the playlist");
                AfterLogin.afterLoginWindow.reloadPlaylists();
                /**
                Expander exp1 = new Expander();
                var newStack = new StackPanel { Name = "NewExpanderStackPanel" };
                var newListBox = new System.Windows.Controls.ListBox();
                newListBox.Name = newList.Text;
                newListBox.Background = Brushes.Black;
                newListBox.Foreground = Brushes.White;
                var newMediaFileList = new List<string>();
                newStack.Children.Add(newListBox);
                newListBox.Name = newList.Text;
                AfterLogin.ListofListBox.Add(newListBox);
                exp1.Content = newStack;
                exp1.Header = newList.Text;
                exp1.Foreground = Brushes.LightGray;
                AfterLogin.AllPLaylist.Children.Add(exp1);
                newList.Text = "";*/
            }
        }
        private void DeleteButton(object sender, RoutedEventArgs e)
        {
           
            var msg = "No Playlist was selected to delete";
            if (list.SelectedItem != null)
            {
                string b = ((ListBoxItem)list.SelectedItem).Content.ToString();
                if (AfterLogin.afterLoginWindow.currPlaylistName == b)
                {
                    AfterLogin.ax.close();
                }
                list.Items.Remove(list.SelectedItem);
                AfterLogin.objectUser.mPlaylists.Remove(AfterLogin.objectUser.mPlaylists.Where(x => x.mName == b).Single());
                AfterLogin.afterLoginWindow.reloadPlaylists();
            }
            else
                MessageBox.Show(msg);
        }
        private void Go(object sender, RoutedEventArgs e)
        {
            if (list.SelectedItem != null)
            {
                Adding b = new Adding();
                createWIndow = this;
                this.Hide();
                b.Show();
            }
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItem != null)
                selectedList = ((ListBoxItem)list.SelectedValue).Content.ToString();
        }

        private void list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (list.SelectedValue != null)
            {
                selectedList = ((ListBoxItem)list.SelectedValue).Content.ToString();
                Adding b = new Adding();
                createWIndow = this;
                this.Hide();
                b.Show();
            }
            
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
    }
}
