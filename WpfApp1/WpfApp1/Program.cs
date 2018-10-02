using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using NAudio.Wave;
using Newtonsoft.Json;
using System.Threading;
namespace WpfApp1
{
    class SocketServerOut
    {
        public static Boolean buffering;
        public static UserCollection everyUser;
        public static UdpClient udpServer;
        public static IPEndPoint remoteEP;
        public static byte[] portAddress;
        public static string allSongSt = File.ReadAllText(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\AllSongs.json"));
        public static Playlist allSongs = JsonConvert.DeserializeObject<Playlist>(allSongSt);
        static void Main(string[] args)
        {
            udpServer = new UdpClient(11000);
            remoteEP = new IPEndPoint(IPAddress.Any, 11000);

            //tcpServer.Client = udpServer.Client;

            //EndPoint endPoint = new EndPoint(IPAddress.Parse("127.0.0.1"));
            //udpServer.Client.Bind(remoteEP);
            //udpServer.Client.Listen(200);
            //udpServer.Client.Accept();
            string mediaFolder = System.IO.Path.Combine("C: \\Users\\nhan\\Desktop\\Spring2018\\CECS491A\\CECS327\\WpfApp1\\WpfApp1\\", "MusicLibrary");
            byte[] data;

            string st = File.ReadAllText(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\Users.json"));
            everyUser = JsonConvert.DeserializeObject<UserCollection>(st);
            while (true)
            {
                var request = udpServer.Receive(ref remoteEP);
                
                Console.WriteLine("Receiving a request: " + Encoding.ASCII.GetString(request));
                if (Encoding.ASCII.GetString(request) == "connect")
                {
                    portAddress = udpServer.Receive(ref remoteEP);
                    Console.WriteLine("Connection Established");
                    //privateUDP();
                    ThreadStart privateThread = new ThreadStart(privateUDP);
                    Thread childThread = new Thread(privateThread);
                    childThread.Start();

                }
                if (Encoding.ASCII.GetString(request) == "login")
                {

                    data = udpServer.Receive(ref remoteEP);
                    string id = Encoding.ASCII.GetString(data);
                    data = udpServer.Receive(ref remoteEP);
                    string password = Encoding.ASCII.GetString(data);
                    Console.WriteLine(" id: " + id + " password: " + password);

                    var msg = "Incorrect Username or Password!";
                    int counter = 0;
                    foreach (var x in everyUser.Users)
                    {

                        if (id == x.mUsername && password == x.mPassword)
                        {
                            Console.WriteLine("Correct username and password. Access granted");
                            udpServer.Send(Encoding.ASCII.GetBytes("granted"), 7, remoteEP);
                            var b = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(x));
                            udpServer.Send(b, b.Length, remoteEP);


                            b = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(allSongs));
                            udpServer.Send(b, b.Length, remoteEP);
                            //AfterLogin b = new AfterLogin(x);
                            counter++;
                            //b.Show();
                            break;
                        }

                    }
                    if (counter == 0)
                    {
                        Console.WriteLine("incorrect username or pasesword. Access denied");
                        udpServer.Send(Encoding.ASCII.GetBytes("denied"), 6, remoteEP);
                        //InvalidLogin.Visibility = Visibility.Visible;
                        //MessageBox.Show(msg);
                    }
                }
                if (Encoding.ASCII.GetString(request) == "playMusic")
                {
                    int count1 = 0;
                    var buffer1 = new byte[16384 * 4];
                    var nameOfTheSong = udpServer.Receive(ref remoteEP);
                    Song sendingSong = null;
                    try
                    {
                        sendingSong = allSongs.mSongs.Where(x => x.ToString() == Encoding.ASCII.GetString(nameOfTheSong)).Single();
                    }
                    catch
                    {
                        var msg = Encoding.ASCII.GetBytes("Error occurred: Couldn't find this song in our library");
                        udpServer.Send(msg, msg.Length, remoteEP);
                    }
                    udpServer.Send(Encoding.ASCII.GetBytes("granted"), 7, remoteEP);
                    if (sendingSong != null)
                    {

                        Mp3FileReader reader = new Mp3FileReader(mediaFolder + "\\" + sendingSong.Directory);

                        Mp3Frame mp3Frame = reader.ReadNextFrame();
                        IMp3FrameDecompressor decomp = null;
                        WaveFormat waveFormat = new Mp3WaveFormat(mp3Frame.SampleRate, mp3Frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                        mp3Frame.FrameLength, mp3Frame.BitRate);
                        decomp = new AcmMp3FrameDecompressor(waveFormat);
                        int total = 0;
                        while (mp3Frame != null)
                        {

                            //if (count > 50000000) //retrieve a sample of 500 frames
                            //   return;
                            //   int decompressed = decomp.DecompressFrame(mp3Frame, buffer1, 0);
                            // total += decompressed;

                            //_fs.Write(mp3Frame.RawData, 0, mp3Frame.RawData.Length);
                            data = mp3Frame.RawData;
                            udpServer.Send(data, data.Length, remoteEP);
                            if (Encoding.ASCII.GetString(udpServer.Receive(ref remoteEP)) != "more")
                                break;

                            total += data.Length;
                            if (count1 % 500 == 0)
                            {
                                Console.WriteLine(" Sending Song: " + sendingSong.ToString());
                                Console.WriteLine("Total packet sent: " + total);
                            }


                            count1 = count1 + 1;
                            mp3Frame = reader.ReadNextFrame();

                        }
                        udpServer.Send(Encoding.ASCII.GetBytes("done"), 4, remoteEP);
                        Console.WriteLine("Total packet sent: " + total);
                    }
                }
            }

            /* 
            while (true)
            {
                string strMP3Folder = "C:\\Users\\nhan\\Music\\";
                string strMP3SourceFilename = "KAAN-Kaancepts.mp3";
                string strMP3OutputFilename = "KAAN-KaanceptsSplit.mp3";
                byte[] buffer;
                var remoteEP = new IPEndPoint(IPAddress.Any, 11000);



                var buffer1 = new byte[16384 * 4];
                using (Mp3FileReader reader = new Mp3FileReader(strMP3Folder + strMP3SourceFilename))
                {
                    int count = 1;
                    Mp3Frame mp3Frame = reader.ReadNextFrame();
                    System.IO.FileStream _fs = new System.IO.FileStream(strMP3Folder + strMP3OutputFilename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                   // IMp3FrameDecompressor decomp = null;
                    //WaveFormat waveFormat = new Mp3WaveFormat(mp3Frame.SampleRate, mp3Frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                   //mp3Frame.FrameLength, mp3Frame.BitRate);
                   // decomp = new AcmMp3FrameDecompressor(waveFormat);
                    //int total = 0;
                    while (mp3Frame != null)
                    {
                         var data = udpServer.Receive(ref remoteEP); // listen on port 11000
                        Console.WriteLine("receive data from " + remoteEP.ToString());
                        if (count > 10000) //retrieve a sample of 500 frames
                            return;
                     //   int decompressed = decomp.DecompressFrame(mp3Frame, buffer1, 0);
                       // total += decompressed;

                        //_fs.Write(mp3Frame.RawData, 0, mp3Frame.RawData.Length);
                        buffer = mp3Frame.RawData;
                        udpServer.Send(buffer, buffer.Length, remoteEP);
                        Console.WriteLine("Sending package" + buffer.Length);
                        
                        count = count + 1;
                        mp3Frame = reader.ReadNextFrame();
                        
                    }
                    
                    _fs.Close();
                }

                
                

            }
            */

        }
        //public void user(Socket client)
        public static void privateUDP()
        {

            int i = 0;
            Int32.TryParse(Encoding.ASCII.GetString(portAddress), out i);
            UdpClient privatePort = new UdpClient(i);
            var privateEP = new IPEndPoint(IPAddress.Any, i);
            while (true)
            {
                
                var request = privatePort.Receive(ref privateEP);
                Console.WriteLine("Receiving a request: " + Encoding.ASCII.GetString(request));
                if (Encoding.ASCII.GetString(request) == "login")
                    login(privatePort, privateEP);
                if (Encoding.ASCII.GetString(request) == "playMusic")
                    playMusic(privatePort, privateEP);
                if(Encoding.ASCII.GetString(request) == "logout")
                {
                    privatePort.Close();
                    Console.WriteLine("Disconnecting user");
                    break;
                }

            }
        }
        public static void login(UdpClient privatePort, IPEndPoint privateEP)
        {


            var data = privatePort.Receive(ref privateEP);
            string id = Encoding.ASCII.GetString(data);
            data = privatePort.Receive(ref privateEP);
            string password = Encoding.ASCII.GetString(data);
            Console.WriteLine(" id: " + id + " password: " + password);

            var msg = "Incorrect Username or Password!";
            int counter = 0;
            foreach (var x in everyUser.Users)
            {

                if (id == x.mUsername && password == x.mPassword)
                {
                    Console.WriteLine("Correct username and password. Access granted");
                    privatePort.Send(Encoding.ASCII.GetBytes("granted"), 7, privateEP);
                    var b = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(x));
                    privatePort.Send(b, b.Length, privateEP);


                    b = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(allSongs));
                    privatePort.Send(b, b.Length, privateEP);
                    //AfterLogin b = new AfterLogin(x);
                    counter++;
                    //b.Show();
                    break;
                }
               
            }
            if (counter == 0)
            {
                Console.WriteLine("Incorrect username or password. Access denied");
                privatePort.Send(Encoding.ASCII.GetBytes("denied"), 6, privateEP);

            }
        }
        public static void playMusic(UdpClient privatePort, IPEndPoint privateEP)
        {
            string mediaFolder = System.IO.Path.Combine("C: \\Users\\nhan\\Desktop\\Spring2018\\CECS491A\\CECS327\\WpfApp1\\WpfApp1\\", "MusicLibrary");
            byte[] data;
            int count1 = 0;
            var buffer1 = new byte[16384 * 4];
            var nameOfTheSong = privatePort.Receive(ref privateEP);
            Song sendingSong = null;
            try
            {
                sendingSong = allSongs.mSongs.Where(x => x.ToString() == Encoding.ASCII.GetString(nameOfTheSong)).Single();
            }
            catch
            {
                var msg = Encoding.ASCII.GetBytes("Error occurred: Couldn't find this song in our library");
                privatePort.Send(msg, msg.Length, privateEP);
            }
            privatePort.Send(Encoding.ASCII.GetBytes("granted"), 7, privateEP);
            if (sendingSong != null)
            {

                Mp3FileReader reader = new Mp3FileReader(mediaFolder + "\\" + sendingSong.Directory);

                Mp3Frame mp3Frame = reader.ReadNextFrame();
                IMp3FrameDecompressor decomp = null;
                WaveFormat waveFormat = new Mp3WaveFormat(mp3Frame.SampleRate, mp3Frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                mp3Frame.FrameLength, mp3Frame.BitRate);
                decomp = new AcmMp3FrameDecompressor(waveFormat);
                int total = 0;
                while (mp3Frame != null)
                {

                    //if (count > 50000000) //retrieve a sample of 500 frames
                    //   return;
                    //   int decompressed = decomp.DecompressFrame(mp3Frame, buffer1, 0);
                    // total += decompressed;

                    //_fs.Write(mp3Frame.RawData, 0, mp3Frame.RawData.Length);
                    data = mp3Frame.RawData;
                    privatePort.Send(data, data.Length, privateEP);
                    if (Encoding.ASCII.GetString(privatePort.Receive(ref privateEP)) != "more")
                        break;

                    total += data.Length;
                    if (count1 % 500 == 0)
                    {
                        Console.WriteLine(" Sending Song: " + sendingSong.ToString());
                        Console.WriteLine("Total packet sent: " + total);
                    }


                    count1 = count1 + 1;
                    mp3Frame = reader.ReadNextFrame();

                }
                udpServer.Send(Encoding.ASCII.GetBytes("done"), 4, privateEP);
                Console.WriteLine("Total packet sent: " + total);

            }
        }
    }
}
