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
        public static SortedDictionary<string, List<string>> dict = new SortedDictionary<string, List<string>>();

        public static string allSongSt = File.ReadAllText(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "UserJson\\AllSongs.json"));
        public static Playlist allSongs = JsonConvert.DeserializeObject<Playlist>(allSongSt);
        public static string mediaFolder = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "MusicLibrary");
        static void Main(string[] args)
        {
            Mapper mapper = new Mapper();
            List<Song> goodSongs = new List<Song>();
            string typestr = "";
            var checkType = allSongs.Songs[0].Title[0];
            if ((int)checkType < 71 && (int)checkType > 64)
                typestr = "A-F";
            else if ((int)checkType <= 79 && (int)checkType >= 71)
                typestr = "G-O";
            if ((int)checkType <= 90 && (int)checkType >= 80)
                typestr = "P-Z";
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

                stm.Write(asen.GetBytes(typestr), 0, typestr.Length);
                //---receiving (2)
                foreach (var b in allSongs.Songs)
                {
                    Console.WriteLine("[" + b.ToString() + "]");
                }
                while (true)
                {
                    int byteRead = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);
                    Console.WriteLine(asen.GetString(request, 0, byteRead));
                    //---- sending songfile message (3)
                    if (asen.GetString(request, 0, byteRead) == "songRequest")
                    {

                        stm.Write(asen.GetBytes("SongFile"), 0, 8);
                        //----sending song(4)
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
                    else if (asen.GetString(request, 0, byteRead) == "searchTitle")
                    {
                        stm.Write(asen.GetBytes("searchTitle"), 0, "searchTitle".Length);

                        Playlist listSongFound = new Playlist("foundSongs");
                        int partLength = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);
                        string stringpart = asen.GetString(request, 0, partLength);

                        foreach (Song s in allSongs.Songs)
                        {
                            if (s.Title.ToLower().Contains(stringpart.ToLower()))
                            {
                                listSongFound.addSong(s);
                            }
                        }
                        Console.WriteLine("With keyword: " + stringpart + ", we found " + listSongFound.Songs.Count + " song(s)");

                        string stringJson = JsonConvert.SerializeObject(listSongFound);
                        stm.Write(asen.GetBytes(stringJson), 0, stringJson.Length);
                        Console.WriteLine("Send Found message and playlist object");
                        listSongFound.Songs.Clear();
                    }
                    else if (asen.GetString(request, 0, byteRead) == "check")
                    {
                        //SortedSet<string> g = new SortedSet<string>();
                        List<Song> badSongs = new List<Song>();

                        Console.WriteLine("Receiving check from server");
                        Console.WriteLine("Original list of songs under this peer:");
                        foreach (var b in allSongs.Songs)
                        {
                            var check = b.Title[0];
                            string type = "";
                            if ((int)check < 71 && (int)check > 64)
                                type = "A-F";
                            else if ((int)check <= 79 && (int)check >= 71)
                                type = "G-O";
                            if ((int)check <= 90 && (int)check >= 80)
                                type = "P-Z";
                            // Console.WriteLine("[" + b.ToString() + "]");
                            if (type == typestr)
                            {
                                goodSongs.Add(b);
                                /*
                                if (!dict.ContainsKey(b.Title))
                                {
                                    List<string> g = new List<string>();
                                    dict[b.Title] = g;
                                    g.Add(b.ToString());
                                }
                                else
                                {
                                    dict[b.Title].Add(b.ToString());
                                }*/
                            }
                            else
                            {
                                badSongs.Add(b);
                            }

                        }
                        Console.WriteLine("found " + badSongs.Count + " bad song(s)");
                        if (badSongs.Count > 0)
                        {
                            stm.Write(asen.GetBytes("bad"), 0, 3);
                            var badsongSend = JsonConvert.SerializeObject(badSongs);
                            stm.Write(asen.GetBytes(badsongSend), 0, badsongSend.Length);

                        }
                        else
                        {
                            stm.Write(asen.GetBytes("good"), 0, 4);
                        }

                        //int hello = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);
                        //Console.WriteLine(asen.GetString(request, 0, hello));
                    }
                    else if (asen.GetString(request, 0, byteRead) == "map")
                    {
                        byteRead = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);
                        mapper.clear();
                        DFS.Map(asen.GetString(request, 0, byteRead), mapper);
                        mapper.printMap();
                    }

                    else if (asen.GetString(request, 0, byteRead) == "hello")
                    {
                        Console.WriteLine("Receiving hello from server");
                    }
                    else if (asen.GetString(request, 0, byteRead) == "reduce")
                    {
                        Console.WriteLine("Receiving reduce from server");
                        mapper.emitMapReduce();

                    }

                    else if (asen.GetString(request, 0, byteRead) == "add")
                    {
                        Console.WriteLine("Adding the song to map");
                        byteRead = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);
                        var songObject = JsonConvert.DeserializeObject<Song>(asen.GetString(request));
                        goodSongs.Add(songObject);
                        //mapper.emitAdd(songObject);
                        //mapper.printMap();
                    }
                }
            }
        }
    }
}

