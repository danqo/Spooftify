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


            string mediaFolder = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "MusicLibrary");
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
                    ThreadStart privateThread = new ThreadStart(privateUDP);
                    Thread childThread = new Thread(privateThread);
                    childThread.Start();

                }
              
            }

            

        }
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

                    logout(privatePort, privateEP);
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
                    string acctJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("UserJson\\{0}.json", id)));
                    var b = Encoding.ASCII.GetBytes(acctJson);
                    
                    privatePort.Send(b, b.Length, privateEP);


                    b = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(allSongs));
                    privatePort.Send(b, b.Length, privateEP);
                    
                    counter++;
                    
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
            string mediaFolder = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "MusicLibrary");
            byte[] data;
            int count1 = 0;
            var buffer1 = new byte[16384 * 4];
            var nameOfTheSong = privatePort.Receive(ref privateEP);
            var startPoint = privatePort.Receive(ref privateEP);
            Song sendingSong = null;
            try
            {
                sendingSong = allSongs.Songs.Where(x => x.ToString() == Encoding.ASCII.GetString(nameOfTheSong)).Single();
            }
            catch
            {
                var msg = Encoding.ASCII.GetBytes("Error occurred: Couldn't find this song in our library");
                privatePort.Send(msg, msg.Length, privateEP);
            }
            privatePort.Send(Encoding.ASCII.GetBytes("granted"), 7, privateEP);
            if (sendingSong != null)
            {
                int startTime = Int32.Parse(Encoding.ASCII.GetString(startPoint));
                Mp3FileReader reader = new Mp3FileReader(mediaFolder + "\\" + sendingSong.Directory);
                var b = Encoding.ASCII.GetBytes(reader.TotalTime.ToString());
                privatePort.Send(b, b.Length, privateEP);
                reader.CurrentTime = TimeSpan.FromSeconds(startTime);
                Mp3Frame mp3Frame = reader.ReadNextFrame();
                
                IMp3FrameDecompressor decomp = null;
                WaveFormat waveFormat = new Mp3WaveFormat(mp3Frame.SampleRate, mp3Frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                mp3Frame.FrameLength, mp3Frame.BitRate);
                decomp = new AcmMp3FrameDecompressor(waveFormat);
                int total = 0;
                while (mp3Frame != null)
                {
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
                privatePort.Send(Encoding.ASCII.GetBytes("done"), 4, privateEP);
                Console.WriteLine("Total packet sent: " + total);
                Console.WriteLine("------Stop Sending------");

            }
        }
        public static void logout(UdpClient privatePort, IPEndPoint privateEP)
        {

            var b = privatePort.Receive(ref privateEP);
            Account acct = JsonConvert.DeserializeObject<Account>(Encoding.ASCII.GetString(b));
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("UserJson\\{0}.json", acct.Username)), JsonConvert.SerializeObject(acct));
            Console.WriteLine("Saving account changes to json");
            privatePort.Close();
        }
    }
}
