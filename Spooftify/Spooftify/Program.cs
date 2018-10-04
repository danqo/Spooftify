using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using NAudio.Wave;
using System.Threading;
using System.Windows;
using Spooftify;
using Newtonsoft.Json;
using System.Windows.Threading;

namespace WpfApp1
{
    class SocketClientOut
    {
        public static bool buffering;
        private static PlaybackState currentState;
        public static UdpClient client ;
        //public static TcpClient tcpClient ;
        public static IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
        public static BufferedWaveProvider bufferedWaveProvider = null;
        public static WaveOut waveOut = null;
        public static IMp3FrameDecompressor decomp;
        //got helps from erszcz on stackoverflow
        

        public static void connectionEstablish()
        {
            client =  new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000); // endpoint where server is listening

            var buffer = new byte[16384 * 4];

            if (!client.Client.Connected)
            {
                client.Connect(ep);
            }
        }
        public static void logout()
        {
            buffering = false;
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Paused || waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                    client.Send(Encoding.ASCII.GetBytes("no more"), 7);

                    byte[] b = new byte[100];
                    do
                    {
                        b = client.Receive(ref ep);
                    } while (Encoding.ASCII.GetString(b) != "done");

                    //client.Receive(ref ep);
                }
            }
            SocketClientOut.client.Send(Encoding.ASCII.GetBytes("logout"), 6);
            string c = JsonConvert.SerializeObject(AccountManager.instance.Acct);
            var send = Encoding.ASCII.GetBytes(c);
            SocketClientOut.client.Send(send, send.Length);
        }
        public static void privatePort()
        {
            Random rnd = new Random();
            int port = rnd.Next(10000, 11000);
            string sPort = port.ToString();
            client.Send(Encoding.ASCII.GetBytes("connect"), 7);
            client.Send(Encoding.ASCII.GetBytes(sPort), 5);
            client.Close();
            ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            client = new UdpClient();
            int count = 0;
            while (!client.Client.Connected)
            {
                count++;
                Thread.Sleep(300);
                client.Connect(ep);
                if(count >4)
                {
                    MessageBox.Show("Failed to establish private connection");
                }
            }
            //client.Send(Encoding.ASCII.GetBytes("Hello"), 5);

        }
        public static  void sendActionRequest(byte[] requestAction)
        {
            client.Send(requestAction, requestAction.Length);
           //tcpClient.Client = client.Client;
            //tcpClient.Client.Send(requestAction);
        }
        public static void sendIdAndPassword(byte[] id, byte[] password)
        {
            client.Send(id, id.Length);
            client.Send(password, password.Length);
        }
        public static void sendSongName(byte[] SongName)
        {
            client.Send(SongName, SongName.Length);
           
        }

        public static void receivingSong()
        {
            waveOut = new WaveOut();
            decomp = null;

            int count = 0;
            var buffer = new byte[16384 * 4];
            // then receive data
            do
            {
                //if(bufferedWaveProvider != null &&
                //        bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                //      < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4)
                if(bufferedWaveProvider != null &&bufferedWaveProvider.BufferedDuration.TotalSeconds > 10)
                {
                    Thread.Sleep(1000);
                    if (buffering == false)
                        break;
                }
                byte[] receivedData = new byte[2000];
                if (!receiveData(ref receivedData))
                    break;
                  
                if (Encoding.ASCII.GetString(receivedData) == "done")
                    break;
                else
                    client.Send(Encoding.ASCII.GetBytes("more"), 4);    // Nhan change value here
            
                Mp3Frame frame;
                Stream ms = new MemoryStream();
                
                ms.Write(receivedData, 0, receivedData.Length);
                ms.Position = 0;
                frame = Mp3Frame.LoadFromStream(ms, true);
                if (decomp == null)
                {
                    WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                    frame.FrameLength, frame.BitRate);
                    decomp = new AcmMp3FrameDecompressor(waveFormat);
                    bufferedWaveProvider = new BufferedWaveProvider(decomp.OutputFormat);
                    bufferedWaveProvider.BufferDuration =
                        TimeSpan.FromSeconds(20);
                    //var volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
                    //volumeProvider.Volume = 1;
                    
                    // allow us to get well ahead of ourselves
                    //this.bufferedWaveProvider.BufferedDuration = 250;
                }
                if (bufferedWaveProvider.BufferedDuration.TotalSeconds > 5 && waveOut.PlaybackState == PlaybackState.Stopped && buffering == true)
                {
                    waveOut.Init(bufferedWaveProvider);
                    waveOut.Play();
                    //ThreadStart play = new ThreadStart(playSong);
                    //Thread playThread = new Thread(play);
                    //playThread.Start();
                    
                }
                try
                {
                    int decompressed = decomp.DecompressFrame(frame, buffer, 0);
                    bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                }
                catch
                {
                    break;
                }
              

                count++;
            } while (buffering);

            
        }
        public static bool receiveData(ref byte[] data)
        {
            
            try
            {
                data = client.Receive(ref ep);
            }

            catch(System.Net.Sockets.SocketException)
            {
                MessageBox.Show("connection was interrupted");
                if (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
                    waveOut.Stop();
                return false;
            }
            
            return true;
        }
        public static void stopSong()
        {
            buffering = false;
            
           if (waveOut.PlaybackState == PlaybackState.Paused || waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Stop();
                client.Send(Encoding.ASCII.GetBytes("no more"), 7);

                byte[] b = new byte[100];
                do
                {
                    b = client.Receive(ref ep);
                } while (Encoding.ASCII.GetString(b) != "done");
                
                //client.Receive(ref ep);
            }
        }
        public static void pauseSong()
        {
            buffering = false;
            waveOut.Pause();
        }
        public static void resumeSong()
        {
            buffering = true;
            waveOut.Resume();
          
            var buffer = new byte[16384 * 4];
            do
            {
                if (bufferedWaveProvider != null && bufferedWaveProvider.BufferedDuration.TotalSeconds > 10)
                {
                    Thread.Sleep(1000);
                }
                var receivedData = client.Receive(ref ep);
                if (Encoding.ASCII.GetString(receivedData) == "done")
                    break;
                else
                    client.Send(Encoding.ASCII.GetBytes("more"), 4);

                Mp3Frame frame;
                Stream ms = new MemoryStream();

                ms.Write(receivedData, 0, receivedData.Length);
                ms.Position = 0;
                frame = Mp3Frame.LoadFromStream(ms, true);
                int decompressed = decomp.DecompressFrame(frame, buffer, 0);
                bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
               
            } while (buffering);
        }
        public static void playSong()
        {
            //waveOut = new WaveOut();
            //waveOut.Init(bufferedWaveProvider);
            waveOut.Play();
        }
        public static byte[] receiveAccess()
        {
            try
            {
                return client.Receive(ref ep);
            }
            catch
            {
                //MessageBox.Show("can't connect to the host, please try again later");
            }
            return Encoding.ASCII.GetBytes("ServerTimeOut");
        }
            
        
        
        
    }
    }

