using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;

namespace RemoteVideoTransfer.TestClient.Video
{
    public delegate void NewjpegDataEventHandler(byte[] jpegData);
    class VideoDataProvider
    {
        static Capture _capture;
        static bool inProcess = false;
        public event NewjpegDataEventHandler NewjpegData;
        private double TargetFps = 30;
        private double frameSendInterval = 0;

        /// <summary>
        /// 默认帧宽
        /// </summary>
        public static int FrameWidth = 800;
        /// <summary>
        /// 默认帧高
        /// </summary>
        public static int FrameHeight = 600;
        /// <summary>
        /// 默认帧高
        /// </summary>
        public static int Fps = 30;

        public VideoDataProvider()
        {
            _capture = new Capture();
            _capture.SetCaptureProperty(CapProp.FrameWidth, FrameWidth);
            _capture.SetCaptureProperty(CapProp.FrameHeight, FrameHeight);
            _capture.ImageGrabbed += ProcessFrame;
            frameSendInterval = 1000 / TargetFps;
        }

        public void Start()
        {
            _capture.Start();
        }

        public void Stop()
        {
            _capture.Stop();
        }
        DateTime lastFrameTime = DateTime.MinValue;
        private void ProcessFrame(object sender, EventArgs e)
        {
            if (inProcess)
            {
                Console.WriteLine("skip a frame inprocess");
                return;
            }
            if (lastFrameTime != DateTime.MinValue
                &&
                (DateTime.Now - lastFrameTime).TotalMilliseconds < frameSendInterval)
            {
                return;
            }

            inProcess = true;
            var mat = _capture.QueryFrame();
            Image<Bgr, byte> image = mat.ToImage<Bgr, byte>();
            //var imageScaled = image.Resize(0.5, Inter.Area);
            //image.Dispose();
            //var dataSend = imageScaled.ToJpegData(quality: 6);
            //imageScaled.Dispose();
            var dataSend = image.ToJpegData(quality: 6);
            image.Dispose();
            mat.Dispose();
            NewjpegData?.Invoke(dataSend);
            lastFrameTime = DateTime.Now;
            inProcess = false;
        }
    }
}
