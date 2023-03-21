using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteVideoTransfer.TestClient.Entity
{
    class VideoFrame
    {
        public byte[] PictureData { get; set; }
        public byte[] AudioData { get; set; }
    }
}
