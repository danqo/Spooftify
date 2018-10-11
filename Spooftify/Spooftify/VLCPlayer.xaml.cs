using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Spooftify
{
    /// <summary>
    /// Interaction logic for VLCPlayer.xaml
    /// </summary>
    public partial class VLCPlayer : Window
    {
        public VLCPlayer()
        {
            InitializeComponent();
            
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileAsync(
                    // Param1 = Link of file
                    new System.Uri("http://13.250.130.24/wp-content/uploads/2018/10/eminem-venom.mp4"),
                    // Param2 = Path to save
                    "C:\\Users\\nhan\\Music\\eminem\\eminem-venom.mp4"
                );
            }
            Thread.Sleep(1000);
            var uri = new Uri(@"C:\Users\nhan\Music\eminem\eminem-venom.mp4");
            var convertedURI = uri.AbsoluteUri;
            acitveXVLCPlayer.playlist.add(convertedURI);
            acitveXVLCPlayer.playlist.play();
            //acitveXVLCPlayer.playlist.playItem(0);
        }
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //progressBar.Value = e.ProgressPercentage;
        }
    }
}
