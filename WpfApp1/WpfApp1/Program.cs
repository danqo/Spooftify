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
        //---merging///
        public static TcpListener tcpDeers;
        public static Socket currentSocket;
        public static Stream ms;
        public static string currSongInMs = "";
        public static ASCIIEncoding asen;
        public static Dictionary<string, List<object>> dict = new Dictionary<string, List<object>>();
        
        //public static bool found = false;
        public static string found = "";
        public static byte[] peertoclient = new byte[3000];
        //----merging///
        public static Boolean buffering;
        public static UserCollection everyUser;
        public static UdpClient udpServer;
        public static IPEndPoint remoteEP;
        public static byte[] portAddress;
        public static string allSongSt = File.ReadAllText(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\AllSongs.json"));
        public static Playlist allSongs = JsonConvert.DeserializeObject<Playlist>(allSongSt);
        public static Playlist deerSongs = new Playlist("deer");

        public static List<Song> peerSongList = new List<Song>();
        public static List<string> redisInvIndexList = new List<string>();

        public static int peerCounter = 0;
        public static int mapCounter = 0;
        public static int reduceCounter = 0;
        public static int completedCounter = 0;
        public static List<string> distfs = new List<string> { "Titles.txt", "Artists.txt", "Albums.txt" };

        static void Main(string[] args)
        {
            
            /*
            Console.WriteLine("--Trying out Dictionary");
            string str = "Name of the song";
            List<object> b = new List<object>();
            b.Add(buffering);
            dict[str] = b;
            b.Add(buffering);
            */
            Console.WindowWidth = 120;
            //Console.SetWindowPosition(5, 0);
            udpServer = new UdpClient(11000);
            remoteEP = new IPEndPoint(IPAddress.Any, 11000);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            Console.WriteLine("Starting TCP listener for Peers and UDP listener for clients");

            tcpDeers = new TcpListener(ipAddress, 500);
            tcpDeers.Start();
            ThreadStart TCPThread = new ThreadStart(privateTCP);
            Thread childTCPThread = new Thread(TCPThread);
            childTCPThread.Start();

            ThreadStart commandThread = new ThreadStart(commandServer);
            Thread childCommandThread = new Thread(commandServer);
            childCommandThread.Start();
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
        public static void commandServer()
        {
            Socket s;
            Console.WriteLine("Enter command: (talk, stop, mapreduce)");
            string b = Console.ReadLine();
            while (b != "stop")
            {
                if (b == "talk")
                {

                    Console.WriteLine("Enter Which category you wanna talk to: (A-F), all");
                    b = Console.ReadLine();
                    if (b == "all")
                    {
                        Console.WriteLine("Enter command: (check, send)");
                        string c = Console.ReadLine();
                        if (c == "send")
                        {
                            foreach (var song in peerSongList)
                            {
                                var checkType = song.Title[0];
                                string typestr = "";
                                if ((int)checkType < 71 && (int)checkType > 64)
                                    typestr = "A-F";
                                else if ((int)checkType <= 79 && (int)checkType >= 71)
                                    typestr = "G-O";
                                if ((int)checkType <= 90 && (int)checkType >= 80)
                                    typestr = "P-Z";
                                Console.WriteLine("Sending song: " + song.Title + " to deer [" + typestr + "]");

                                var sock = dict[typestr][0] as Socket;
                                sock.Send(asen.GetBytes("add"));
                                sock.Send(asen.GetBytes(JsonConvert.SerializeObject(song)));
                            }
                        }
                        else if(c == "map")
                        {
                           
                            Console.WriteLine("Sort by: (title, album, artist");
                            c = Console.ReadLine();
                            foreach (var d in dict)
                            {
                                foreach (var sock in dict[d.Key])
                                {
                                    s = sock as Socket;
                                    byte[] data = new byte[1024];
                                    s.Send(asen.GetBytes("map"));
                                    s.Send(asen.GetBytes(c));

                                }
                            }
                           
                        }
                        else
                        {
                            foreach (var d in dict)
                            {
                                foreach (var sock in dict[d.Key])
                                {
                                    //Socket d = new Socket(SocketType.Stream, ProtocolType.Tcp);

                                    //privatePort.Send(Encoding.ASCII.GetBytes("granted"), 7, privateEP);
                                    s = sock as Socket;
                                    byte[] data = new byte[1024];
                                    s.Send(asen.GetBytes(c));
                                    //s.Send(asen.GetBytes(JsonConvert.SerializeObject(s))); 

                                    //receiving message(1) user name and json files
                                    //int k = s.Receive(data);
                                    //Console.WriteLine("Receiving from " + d.Key + ": " + asen.GetString(data));
                                    // if(asen.GetString)
                                }
                            }
                        }
                    }
                    else
                    {


                        byte[] receivedData = new byte[2048];
                        while (true)
                        {
                            int d = -1;
                            try
                            {
                                d = dict[b].Count;
                            }
                            catch
                            {
                                Console.WriteLine("This category isn't available currently, please choose a different one: ");
                                b = Console.ReadLine();
                            }
                            if (d != -1)
                                break;
                        }

                        asen = new ASCIIEncoding();
                        foreach (var sock in dict[b])
                        {
                            //Socket d = new Socket(SocketType.Stream, ProtocolType.Tcp);

                            //privatePort.Send(Encoding.ASCII.GetBytes("granted"), 7, privateEP);
                            Console.WriteLine("Enter command: (check, hello)");
                            string c = Console.ReadLine();
                            s = sock as Socket;
                            byte[] data = new byte[1024];
                            s.Send(asen.GetBytes(c));
                            //s.Send(asen.GetBytes(JsonConvert.SerializeObject(s))); 
                        }
                    }
                }
                if (b == "mapreduce")
                {
                    foreach (string type in distfs)
                    {
                        ResetMapReduce();
                        Console.WriteLine("Telling peers to execute map...");
                        // tell all peers to execute map
                        foreach (var d in dict)
                        {
                            foreach (var sock in dict[d.Key])
                            {
                                s = sock as Socket;
                                s.Send(asen.GetBytes("executeMap"));
                                s.Send(asen.GetBytes(type));
                            }
                        }
                        Console.WriteLine("Waiting for peers' response to redistribute incorrect index values...");
                        // wait for peer responses
                        while (mapCounter != peerCounter)
                        {
                            WaitTwoSec().Wait();
                            Console.WriteLine("Waiting for " + (peerCounter - mapCounter).ToString() + " more peer(s).");
                        }
                        Console.WriteLine("Redistributing index values to peers...");
                        // redistribute to peers
                        foreach (string index in redisInvIndexList)
                        {
                            var checkType = index[0];
                            string typestr = "";
                            if ((int)checkType < 71 && (int)checkType > 64)
                                typestr = "A-F";
                            else if ((int)checkType <= 79 && (int)checkType >= 71)
                                typestr = "G-O";
                            if ((int)checkType <= 90 && (int)checkType >= 80)
                                typestr = "P-Z";
                            Console.WriteLine("Sending index: " + index + " to deer [" + typestr + "]");
                            var sock = dict[typestr][0] as Socket;
                            sock.Send(asen.GetBytes("addToMap"));
                            sock.Send(asen.GetBytes(index));
                        }
                        WaitTwoSec().Wait();
                        Console.WriteLine("Telling peers to execute reduce...");
                        // tell all peers to execute reduce
                        foreach (var d in dict)
                        {
                            foreach (var sock in dict[d.Key])
                            {
                                s = sock as Socket;
                                s.Send(asen.GetBytes("executeReduce"));
                            }
                        }
                        while (reduceCounter != peerCounter)
                        {
                            WaitTwoSec().Wait();
                            Console.WriteLine("Waiting for " + (peerCounter - reduceCounter).ToString() + " more peer(s).");
                        }
                        Console.WriteLine("Map Reduce for " + type + " complete!");
                    }
                }
                Console.WriteLine("Enter command: (talk, stop, mapreduce)");
                b = Console.ReadLine();
            }
            
        }
        public static void privateTCP()
        {
            while (true)
            {
                Socket s = tcpDeers.AcceptSocket();
                Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);
                Thread childThread = new Thread(new ParameterizedThreadStart(privateTCPSocket));
                peerCounter += 1;
                Console.WriteLine("Total number of peers connected: " + peerCounter);
                childThread.Start(s);
            }
        }
        public static void privateTCPSocket(object socket)
        {
            asen = new ASCIIEncoding();
            var s = socket as Socket;
            currentSocket = s;
            byte[] b = new byte[2048];
            byte[] receivedData = new byte[2000];
            //receiving message(1) user name and json files
            int k = s.Receive(b);

            Console.WriteLine("Recieved deer name: " + asen.GetString(b, 0, k) + " With end point as: " + s.RemoteEndPoint);
            k = s.Receive(b);
            string deerType = asen.GetString(b, 0, k);
            Console.WriteLine("The deer hold the songs under catagory: " +deerType);
            if (dict.ContainsKey(deerType))
                dict[deerType].Add(s);
            else
            {
                List<object> d = new List<object>(); 
                dict[deerType] = d;
                d.Add(s);
            }
            Console.WriteLine("Available catagory and numbers of peers in charge of it: ");
            foreach(var e in dict)
            {
                Console.WriteLine(e.Key + " : " + e.Value.Count);
            }
            //sending st string(2)
            //string st = "The string was recieved by the server.";
            //s.Send(asen.GetBytes(st));
            ms = new MemoryStream();
            while (true)
            {

                //--receiving request
                try
                {
                    k = s.Receive(receivedData);
                }
                catch
                {
                    break;
                }
                Console.WriteLine("Receiving a request from peer: " + Encoding.ASCII.GetString(receivedData, 0, k));

                if (asen.GetString(receivedData, 0, k) == "SongFile")
                {
                    while (true)
                    {
                        //receive mp3 raw data  
                        k = s.Receive(receivedData);
                        if (asen.GetString(receivedData, 0, k) != "done")
                        {
                            ms.Write(receivedData, 0, k);
                            s.Send(asen.GetBytes("ok"));
                        }
                        else
                        {
                            Console.WriteLine("received " + ms.Length + "bytes");
                            ms.Position = 0;
                            break;
                        }
                    }
                }
                else if (asen.GetString(receivedData, 0, k) == "searchTitle")
                {
                    Console.WriteLine("Peer to server responded for the request serach title");
 
                    k = s.Receive(receivedData);
                    var c = asen.GetString(receivedData, 0, k);
                    peertoclient = asen.GetBytes(c);// maybe we have to reset this one everytime
                }
                else if(asen.GetString(receivedData, 0, k) == "good")
                {
                    Console.WriteLine("The peer under categorey [" + deerType + "] is in the good shape");
                }
                else if(asen.GetString(receivedData, 0, k) == "bad")
                {
                    Console.WriteLine("The peer under categorey [" + deerType + "] is in the bad shape");
                    k = s.Receive(receivedData);
                    var sendingSongs = JsonConvert.DeserializeObject<List<Song>>(asen.GetString(receivedData));
                    foreach(var song in sendingSongs)
                    {
                        peerSongList.Add(song);
                    }
                   
                    
                }
                else if(asen.GetString(receivedData, 0, k) == "sending invalid entry")
                {
                    k = s.Receive(receivedData);
                    redisInvIndexList.Add(asen.GetString(receivedData, 0, k));
                }
                else if(asen.GetString(receivedData, 0, k) == "done mapping")
                {
                    mapCounter += 1;
                }
                else if (asen.GetString(receivedData, 0, k) == "done reducing")
                {
                    reduceCounter += 1;
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
                Console.WriteLine("Receiving a request from client: " + Encoding.ASCII.GetString(request));
                if (Encoding.ASCII.GetString(request) == "login")
                    login(privatePort, privateEP);
                if (Encoding.ASCII.GetString(request) == "playMusic")
                    playMusic(privatePort, privateEP);
                if (Encoding.ASCII.GetString(request) == "searchTitle")
                    searchTitle(privatePort, privateEP);
                if (Encoding.ASCII.GetString(request) == "logout")
                {

                    logout(privatePort, privateEP);
                    Console.WriteLine("Disconnecting user");
                    break;
                }
                

            }
        }
        public static void searchTitle(UdpClient privatePort, IPEndPoint privateEP)
        {

            byte[] data = new byte[3000];
            asen = new ASCIIEncoding();
            //receiving title of the song from client
            var title = privatePort.Receive(ref privateEP);
            var stringTitle = asen.GetString(title);
            //check for which type of the song based on the first letter of the title;
            var checkType = stringTitle.ToUpper()[0];
            string typestr = "";
            if ((int)checkType < 71 && (int)checkType > 64)
                typestr = "A-F";
            else if ((int)checkType <= 79 && (int)checkType >= 71)
                typestr = "G-O";
            if ((int)checkType <= 90 && (int)checkType >= 80)
                typestr = "P-Z";
            Console.WriteLine("Search song with title: " +stringTitle + " Which start with " + stringTitle.ToUpper()[0] + "under category " + typestr);
            Console.WriteLine("Ask connected peers under [" + typestr + "] to show client de way");
            List<object> removeLater = new List<object>();
            if (dict.ContainsKey(typestr))
            {
                foreach (var socket in dict[typestr])
                {
                    int count = 0;
                    var b = socket as Socket;
                    if(b.Connected)
                    {
                        Console.WriteLine("This peer is still active");
                        Console.WriteLine("sending serachtitle message and part of string song title");
                        b.Send(asen.GetBytes("searchTitle"));
                        b.Send(asen.GetBytes(stringTitle));
                        //int k = b.Receive(data);
                        Thread.Sleep(1000);
                        break;
 
                    }
                    else
                    {
                        
                        Console.WriteLine("The peer at index: " + count + " is disconnected");
                        Console.WriteLine("Look for the next peer");
                        removeLater.Add(socket);
                        //dict[typestr].Remove(socket);
                    }
                }
            }
            foreach(var b in removeLater)
            {
                Console.WriteLine("Remove"+ removeLater.Count()+ " disconnected peer from the list");
                dict[typestr].Remove(b);
                Console.WriteLine("current connected peer for " + typestr+ ": " +dict[typestr].Count);
            }

            removeLater.Clear();
            var songjsonToClient = JsonConvert.DeserializeObject<Playlist>(asen.GetString(peertoclient)); // testing purpose
            Console.WriteLine("Server to Client: json song object");
            privatePort.Send(peertoclient, peertoclient.Length, privateEP);         
            


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


                    b = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(deerSongs));
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
            Console.WriteLine("The name of the requested song is: "+ asen.GetString(nameOfTheSong));
            Console.WriteLine("starting point of the song is" + asen.GetString(startPoint));
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
            string typestr = "";
            var checkType = sendingSong.Title[0];
            if ((int)checkType < 71 && (int)checkType > 64)
                typestr = "A-F";
            else if ((int)checkType <= 79 && (int)checkType >= 71)
                typestr = "G-O";
            if ((int)checkType <= 90 && (int)checkType >= 80)
                typestr = "P-Z";

            if (dict.ContainsKey(typestr) && !Encoding.ASCII.GetString(nameOfTheSong).Equals(currSongInMs))
            //if (dict.ContainsKey(typestr))
            {
                ms = new MemoryStream();
                var b = dict[typestr] ;
                foreach(var d  in b)
                {
                    var c = d as Socket;
                    if (c.Connected)
                    {
                        c.Send(asen.GetBytes("songRequest"));
                        c.Send(nameOfTheSong);
                        Thread.Sleep(2000);
                        break;
                    }
                }
                currSongInMs = Encoding.ASCII.GetString(nameOfTheSong);
            }

            privatePort.Send(Encoding.ASCII.GetBytes("granted"), 7, privateEP);
            if (sendingSong != null)
            {
                int startTime = Int32.Parse(Encoding.ASCII.GetString(startPoint));

                //Mp3FileReader reader = new Mp3FileReader(mediaFolder + "\\" + sendingSong.Directory);
                Mp3FileReader reader = new Mp3FileReader(ms);
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
                //ms = new MemoryStream();
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
        public static void nomore(UdpClient privatePort, IPEndPoint privateEP)
        {

            privatePort.Send(Encoding.ASCII.GetBytes("done"), 4, privateEP);
        }
        public static async Task WaitTwoSec()
        {
            await Task.Delay(1000);
        }
        public static void ResetMapReduce()
        {
            redisInvIndexList.Clear();
            mapCounter = 0;
            reduceCounter = 0;
            completedCounter = 0;
        }
    }
}
