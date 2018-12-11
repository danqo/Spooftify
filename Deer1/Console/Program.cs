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
        public static string fn = "";
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
                    else if (asen.GetString(request, 0, byteRead) == "executeMap")
                    {
                        Console.WriteLine("Executing map...");
                        // first reset mapper
                        DFS.Clear(mapper);
                        byteRead = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);
                        fn = asen.GetString(request, 0, byteRead);
                        List<string> invalidEntries = DFS.Map(mapper, fn);
                        // print contents after mapping is complete
                        Console.WriteLine("Count after mapping: " + DFS.Count(mapper));
                        DFS.Print(mapper);
                        // tell server mapping is complete with list of bad entries
                        foreach (var entry in invalidEntries)
                        {
                            stm.Write(asen.GetBytes("sending invalid entry"), 0, 21);
                            stm.Write(asen.GetBytes(entry), 0, entry.Length);
                        }
                        Console.WriteLine("Done mapping");
                        stm.Write(asen.GetBytes("done mapping"), 0, 12);
                        //var invalidJson = JsonConvert.SerializeObject(invalidEntries);
                        //stm.Write(asen.GetBytes(invalidJson), 0, invalidJson.Length);
                    }
                    else if (asen.GetString(request, 0, byteRead) == "addToMap")
                    {
                        byteRead = stm.Read(request, 0, tcpclnt.ReceiveBufferSize);
                        string newEntry = asen.GetString(request, 0, byteRead);
                        Console.WriteLine("Adding" + newEntry + "to map...");
                        DFS.AddToMap(mapper, newEntry);
                    }
                    else if (asen.GetString(request, 0, byteRead) == "executeReduce")
                    {
                        Console.WriteLine("Count after adding: " + DFS.Count(mapper));
                        Console.WriteLine("Executing reduce...");
                        DFS.Reduce(mapper);
                        // print contents after reducing is complete
                        Console.WriteLine("Count after reducing: " + DFS.Count(mapper));
                        DFS.Print(mapper);
                        DFS.Write(mapper, fn);
                        // send message done reducing
                        Console.WriteLine("Done reducing");
                        stm.Write(asen.GetBytes("done reducing"), 0, 13);
                    }
                }
            }
        }
    }
}

