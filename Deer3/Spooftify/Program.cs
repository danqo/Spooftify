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
    /// <summary>
    /// Handles any udp requests made to the server
    /// </summary>
    public class SocketClientOut
    {
        public static bool buffering;
        public static UdpClient client;

        public static IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
        public static BufferedWaveProvider bufferedWaveProvider = null;
        public static WaveOut waveOut = null;
        public static Thread current = null;
        public static IMp3FrameDecompressor decomp;
        public static int currentLocation = 0;

        public static void connectionEstablish()
        {
            client = new UdpClient();
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
            int port = rnd.Next(10000, 13000);
            string sPort = port.ToString();
            client.Send(Encoding.ASCII.GetBytes("connectDeerServer"), "connectDeerServer".Length);
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
                if (count > 4)
                {
                    MessageBox.Show("Failed to establish private connection");
                }
            }

        }
        public static void sendActionRequest(byte[] requestAction)
        {
            client.Send(requestAction, requestAction.Length);
            
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

        public static void sendStartTime(byte[] startTime)
        {
            client.Send(startTime, startTime.Length);
        }

        public static void receivingSong()
        {
            waveOut = new WaveOut();
            decomp = null;


            int count = 0;
            var buffer = new byte[16384 * 4];
            do
            {
                current = Thread.CurrentThread;
                if (bufferedWaveProvider != null && bufferedWaveProvider.BufferedDuration.TotalSeconds > 5)
                {
                    Thread.Sleep(200);
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
                    try
                    {
                        WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                    frame.FrameLength, frame.BitRate);
                        decomp = new AcmMp3FrameDecompressor(waveFormat);
                    }
                    catch
                    {
                        break;
                    }
                    bufferedWaveProvider = new BufferedWaveProvider(decomp.OutputFormat);
                    bufferedWaveProvider.BufferDuration =
                        TimeSpan.FromSeconds(20);
                }
                if (bufferedWaveProvider.BufferedDuration.TotalSeconds > 5 && waveOut.PlaybackState == PlaybackState.Stopped && buffering == true)
                {
                    try
                    {
                        waveOut.Init(bufferedWaveProvider);
                        waveOut.Play();
                    }
                    catch
                    {
                        break;
                    }
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

            catch (System.Net.Sockets.SocketException)
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
            if (current != null)
            {
                current.Abort();
                current.Abort();

            }

        }
        public static void resumeSong()
        {
            buffering = true;
            waveOut.Resume();

            var buffer = new byte[16384 * 4];
            do
            {
                current = Thread.CurrentThread;
                if (bufferedWaveProvider != null && bufferedWaveProvider.BufferedDuration.TotalSeconds > 10)
                {
                    Thread.Sleep(200);
                    if (buffering == false)
                        break;
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
                try
                {
                    int decompressed = decomp.DecompressFrame(frame, buffer, 0);
                    bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                }
                catch
                {
                    break;
                }


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

