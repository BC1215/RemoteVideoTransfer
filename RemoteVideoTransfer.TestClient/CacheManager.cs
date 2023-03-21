using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteVideoTransfer.TestClient
{
    public delegate void NewFrameDataReadyEventHandler(byte[] AudioData, byte[] VideoData);
    class CacheManager
    {
        public event NewFrameDataReadyEventHandler NewFrameDataReady;

        public Queue<byte[]> AudioData { get; set; }

        public Queue<byte[]> PictureData { get; set; }

        public void PushVideoData(byte[] data)
        {
            PictureData.Enqueue(data);
            TryMakeNewFrameData();
        }

        public void PushAudioData(byte[] data)
        {
            AudioData.Enqueue(data);
            //TryMakeNewFrameData();
        }

        public void TryMakeNewFrameData()
        {
            if (AudioData.Count != 0 && PictureData.Count != 0)
            {
                List<byte> aData = new List<byte>();
                aData.AddRange(AudioData.SelectMany(a => a));
                AudioData.Clear();

                List<byte> vData = new List<byte>();
                vData.AddRange(PictureData.SelectMany(v => v));
                PictureData.Clear();

                NewFrameDataReady?.Invoke(aData.ToArray(), vData.ToArray());
            }
        }
    }
}
