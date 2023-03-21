using NAudio.Wave;
using RemoteVideoTransfer.Data;
using RemoteVideoTransfer.Data.Audio.Interface;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RemoteVideoTransfer.TestClient.Audio
{
    public delegate void NewAudioDataEventHandler(byte[] data);
    class AudioDataProvider : IDisposable
    {
        private readonly INetworkChatCodec codec;
        private readonly WaveInEvent waveIn;
        public event NewAudioDataEventHandler NewAudioData;
        public AudioDataProvider(INetworkChatCodec codec, int inputDeviceNumber)
        {
            this.codec = codec;
            waveIn = new WaveInEvent();
            waveIn.BufferMilliseconds = 50;
            waveIn.DeviceNumber = inputDeviceNumber;
            waveIn.WaveFormat = codec.RecordFormat;
            waveIn.DataAvailable += OnAudioCaptured;
        }

        public void Start()
        {
            waveIn.StartRecording();
        }

        public void Stop()
        {
            waveIn.StopRecording();
        }

        bool inProcess;
        void OnAudioCaptured(object sender, WaveInEventArgs e)
        {
            if (inProcess)
            {
                return;
            }
            inProcess = true;
            //Task.Factory.StartNew(() =>
            //{
            byte[] encoded = codec.Encode(e.Buffer, 0, e.BytesRecorded);
            NewAudioData?.Invoke(encoded);
            //});
            inProcess = false;
        }

        public void Dispose()
        {
            waveIn.DataAvailable -= OnAudioCaptured;
            waveIn.StopRecording();
            waveIn.Dispose();
            waveIn?.Dispose();
        }
    }
}