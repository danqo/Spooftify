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

namespace WpfApp1
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
    }
}
