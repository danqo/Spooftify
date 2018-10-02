using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using NAudio.Wave;
using NAudioDemo.Mp3StreamingDemo;
using System.Threading;

namespace SocketClientOut
{
    class Program
    {
        private static PlaybackState currentState;
        public static UdpClient client = new UdpClient();
        //got helps from erszcz on stackoverflow
        static void Main(string[] args)
        {
            
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000); // endpoint where server is listening

            var buffer = new byte[16384 * 4];
            
            client.Connect(ep);
            client.Send(new byte[] { 1, 2, 3, 4, 5 }, 5);
            // send data


            BufferedWaveProvider bufferedWaveProvider = null;
            WaveOut waveOut = new WaveOut();
            IMp3FrameDecompressor decomp = null;
            
            int count = 0;
            // then receive data
            do
            {
                var receivedData = client.Receive(ref ep);
                Console.WriteLine("receive data from " + ep.ToString());
                client.Send(new byte[] { 1, 2, 3, 4, 5 }, 5);
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
                    waveOut.Init(bufferedWaveProvider);
                    // allow us to get well ahead of ourselves
                    //this.bufferedWaveProvider.BufferedDuration = 250;
                }
                if(bufferedWaveProvider.BufferedDuration.TotalSeconds >4)
                {
                    waveOut.Play();
                }
                int decompressed = decomp.DecompressFrame(frame, buffer, 0);
                
                bufferedWaveProvider.AddSamples(buffer, 0, decompressed);


                count++;
            } while (bufferedWaveProvider.BufferedDuration.TotalSeconds < 18);
            
           
            
            //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
            
            //PlayMp3FromUrl(receivedData);
            Console.Read();
            }
        
        public void connectionEstablish()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000); // endpoint where server is listening

            var buffer = new byte[16384 * 4];

            client.Connect(ep);
        }
        
        public void sendLoginRequest(byte[] requestLogin)
        {
            client.Send(requestLogin, 5);
        }
        public void sendIdAndPassword(byte[] id, byte[] password)
        {
            client.Send(id, id.Length);
            client.Send(password, password.Length);
        }
            

        
        public static void PlayMp3FromUrl(object state)
        {
        


            /*
            Stream ms = new MemoryStream();
            ms.Write(buffer, 0, 64000);
            ms.Position = 0;
            WaveStream blockAlignedStream = new BlockAlignReductionStream(
                WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms)));
            WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            waveOut.Init(blockAlignedStream);
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                System.Threading.Thread.Sleep(100);
            }*/
            /*
        BufferedWaveProvider bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(8000, 16, 2));
        bufferedWaveProvider.Read(buffer, 0, buffer.Length);
        WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
        waveOut.Init(bufferedWaveProvider);
        waveOut.Play();
        while (waveOut.PlaybackState == PlaybackState.Playing)
        {
            System.Threading.Thread.Sleep(100);
        }*/
            /*
            using (Stream ms = new MemoryStream())
            {
                
                using (Stream stream = WebRequest.Create(url)
                    .GetResponse().GetResponseStream())
                {
                    byte[] buffer = new byte[32768];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                }
                ms.Write(buffer, 0, 64000);
                
                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }*/
        }
    }
    }

