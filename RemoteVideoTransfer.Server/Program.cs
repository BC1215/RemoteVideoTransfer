using RemoteVideoTransfer.Data;
using RemoteVideoTransfer.Data.Audio.Codec;
using RemoteVideoTransfer.Server.Audio;
using RemoteVideoTransfer.Server.Socket;
using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteVideoTransfer.Server
{
    static class Program
    {
        static FrmShow _frm = new FrmShow();
        static AudioDataPlayer _audioDataPlayer;
        static int picReceiveCount = 0;
        static void Main()
        {
            _audioDataPlayer = new AudioDataPlayer(new UncompressedPcmChatCodec());
            _frm.BackgroundImageLayout = ImageLayout.Zoom;
            Task.Factory.StartNew(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(_frm);
            });

            var transferServer = new TransferServerStarter();
            var servers = transferServer.Start(1);
            Console.WriteLine("启动结果: {0}！", servers != null);
            if (servers != null)
            {
                foreach (SocketServer socketServer in servers)
                {
                    Console.WriteLine(socketServer.Name);
                    Console.WriteLine(socketServer.Config.Ip);
                    Console.WriteLine(socketServer.Config.Port);
                    socketServer.OnCommandArrived += socketServer_OnCommandArrived;
                    socketServer.NewSessionConnected += socketServer_NewSessionConnected;
                    socketServer.SessionClosed += socketServer_SessionClosed;
                }
            }


            Console.ReadLine();

            transferServer.Stop();
        }

        private static void socketServer_SessionClosed(SocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            //throw new NotImplementedException();
        }

        private static void socketServer_NewSessionConnected(SocketSession session)
        {
            //throw new NotImplementedException();
        }


        private static void socketServer_OnCommandArrived(SocketSession clientSession, byte[] data, int type)
        {
            if (type == 0)
            {
                picReceiveCount++;
                _frm.Invoke(new Action(() =>
                {
                    var img = Bytes2Bitmap(data);
                    _frm.BackgroundImage = img;
                    _frm.Refresh();
                }));
                Console.WriteLine($@"picture data received, length: {data.Length}");
            }
            else if (type == 1)
            {
                _audioDataPlayer.PlayAudioData(data);
                Console.WriteLine($@"audio data received, length: {data.Length}");
            }
            Console.Title = $"已接收视频帧：{picReceiveCount}，音频已缓冲时长：{_audioDataPlayer.waveProvider.BufferedDuration}";
        }

        static Bitmap Bytes2Bitmap(byte[] data)
        {
            var ms = new MemoryStream(data);
            var img = new Bitmap(ms);
            return img;
        }
    }
}
