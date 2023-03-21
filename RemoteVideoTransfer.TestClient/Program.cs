using RemoteVideoTransfer.Data;
using RemoteVideoTransfer.Data.Audio.Codec;
using RemoteVideoTransfer.TestClient.Audio;
using RemoteVideoTransfer.TestClient.Video;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace RemoteVideoTransfer.TestClient
{
    static class Program
    {
        static Stopwatch _stopwatch = new Stopwatch();
        static int _port = 46613;
        static string _host = "127.0.0.1";
        //创建终结点EndPoint
        static IPAddress _ip = IPAddress.Parse(_host);
        static IPEndPoint _ipe = new IPEndPoint(_ip, _port);   //把ip和端口转化为IPEndPoint的实例

        static VideoDataProvider _videoDataProvider = new VideoDataProvider();
        static AudioDataProvider _audioDataProvider;
        static int _sendFrameCount = 0;
        static Stopwatch _stopWatchFrame = new Stopwatch();

        ////创建Socket并连接到服务器
        //static Socket _audioSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        //{
        //    ReceiveBufferSize = 102400000,
        //    SendBufferSize = 102400000,
        //    SendTimeout = int.MaxValue,
        //    ReceiveTimeout = int.MaxValue
        //};  //创建Socket
        //static Socket _pictureSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        //{
        //    ReceiveBufferSize = 102400000,
        //    SendBufferSize = 102400000,
        //    SendTimeout = int.MaxValue,
        //    ReceiveTimeout = int.MaxValue
        //};  //创建Socket
        static TcpClient _audioSocket = new TcpClient();
        static TcpClient _pictureSocket = new TcpClient();
        static ulong totalDataSize;
        static void Main()
        {
            _pictureSocket.Connect(_ipe); //连接到服务器
            _audioSocket.Connect(_ipe); //连接到服务器
            _audioDataProvider = new AudioDataProvider(new UncompressedPcmChatCodec(), 0);
            _videoDataProvider.NewjpegData += _videoDataProvider_NewjpegData;
            _audioDataProvider.NewAudioData += _audioDataProvider_NewAudioData;

            _videoDataProvider.Start();
            _audioDataProvider.Start();
            _stopwatch.Start();
            Console.ReadLine();
            _stopwatch.Stop();

            var totalDataSizeInMB = (totalDataSize / 1024d / 1024);
            var fps = _sendFrameCount / _stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine($"{_sendFrameCount} frames, {totalDataSizeInMB.ToString("F2")} MB data has sent in {_stopwatch.Elapsed.TotalSeconds.ToString("F3")}s,{fps.ToString("F2")}fps, { (totalDataSizeInMB / (_stopwatch.Elapsed.TotalSeconds)).ToString("F2")}MB/S");
            _videoDataProvider.Stop();
            _audioDataProvider.Stop();

            Console.ReadLine();
            //_stopwatch.Start();

            //foreach (var path in Directory.GetFiles(@"C:\Users\sadly\source\repos\RemoteVideoTransfer\Output\frames"))
            //{
            //    var imageData = File.ReadAllBytes(path);

            //    //break;
            //}

            //Console.WriteLine("done");
            //Console.WriteLine(_stopwatch.Elapsed.TotalSeconds.ToString("F7"));
        }

        private static void _audioDataProvider_NewAudioData(byte[] data)
        {
            SendData(data, 1);
        }

        private static void _videoDataProvider_NewjpegData(byte[] jpegData)
        {
            SendData(jpegData, 0);
            //Console.WriteLine($"frame {++_sendFrameCount} has sent in {_stopWatchFrame.Elapsed.TotalMilliseconds} ms, length: {jpegData.Length}, {jpegData.Length / 1024d / 1024 / (_stopWatchFrame.Elapsed.TotalSeconds)} MB/S");
            //_stopWatchFrame.Restart();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">0picture,1audio</param>
        static void SendData(byte[] data, byte type)
        {
            var header = new HeaderStruct
            {
                PackageType = type,
                ExtraData = 0,
                BodyLength = (uint)Math.Abs(data.Length)
            };

            var combineData = ExtMethods.CombineHeaderAndBody(header, data);
            if (type == 0)
            {
                //_pictureSocket.Client.Send(combineData);
                _pictureSocket.Client.BeginSend(combineData, 0, combineData.Length, SocketFlags.None, null, null);
                _sendFrameCount++;
                Console.WriteLine($"picture data send, length {data.Length}");
            }
            else if (type == 1)
            {
                //_audioSocket.Client.Send(combineData);
                _audioSocket.Client.BeginSend(combineData, 0, combineData.Length, SocketFlags.None, null, null);
                Console.WriteLine($"audio data send, length {data.Length}");
            }
            totalDataSize += (ulong)data.Length;
        }
    }
}
