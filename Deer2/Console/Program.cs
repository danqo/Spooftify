using NAudio.Wave;
using Newtonsoft.Json;
using Spooftify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WpfApp1;

namespace ConsoleDeer1

{
    class Program
    {
        public static string allSongSt = File.ReadAllText(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\AllSongs.json"));
        public static Playlist allSongs = JsonConvert.DeserializeObject<Playlist>(allSongSt);
        public static string mediaFolder = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "MusicLibrary");
        static void Main(string[] args)
        {
            Console.WriteLine("Enter to start the connection from peer to server: ");
            Console.ReadLine();
            TcpClient tcpclnt = new TcpClient();
            Console.WriteLine("Connecting.....");
            tcpclnt.Client.Connect("127.0.0.1", 500);
            Console.WriteLine("Connected");
            while (true)
            {

                Console.Write("Username: ");
                var username = Console.ReadLine();
                Stream stm = tcpclnt.GetStream();
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(username);
                byte[] request = new byte[tcpclnt.ReceiveBufferSize];
                //---sending--- (1) all songs json

                stm.Write(ba, 0, ba.Length);
                Console.WriteLine("Transmitting songs json");
                stm.Write(asen.GetBytes(allSongSt), 0, allSongSt.Length);
                //---receiving (2)
                while (true)
                {
                    int byteRead = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);
                    Console.WriteLine(asen.GetString(request, 0, byteRead));
                    //---- sending song (3)
                    if (asen.GetString(request, 0, byteRead) == "songRequest")
                    {
                        int songLength = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);

                        var sendingSong = allSongs.Songs.FirstOrDefault(x => x.ToString() == asen.GetString(request, 0, songLength));
                        Mp3FileReader reader = new Mp3FileReader(mediaFolder + "\\" + sendingSong.Directory);
                        Mp3Frame mp3Frame = reader.ReadNextFrame();

                        int total = 0;
                        int count1 = 0;
                        while (mp3Frame != null)
                        {
                            var data = mp3Frame.RawData;
                            stm.Write(data, 0, data.Length);

                            total += data.Length;
                            if (count1 % 500 == 0)
                            {
                                Console.WriteLine(" Sending Song: " + sendingSong.ToString());
                                Console.WriteLine("Total packet sent: " + total);
                            }
                            count1 = count1 + 1;
                            mp3Frame = reader.ReadNextFrame();
                            //receive an ok message 
                            int sendNextFrame = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);
                        }

                        stm.Write(asen.GetBytes("done"), 0, 4);
                        Console.WriteLine("Total packet sent: " + total);
                        Console.WriteLine("------Stop Sending------");

                    }
                }
            }
        }
    }
}

