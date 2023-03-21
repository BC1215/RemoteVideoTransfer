using NAudio.Wave;
using RemoteVideoTransfer.TestClient.Audio.Interface;
using System;

namespace RemoteVideoTransfer.TestClient.Audio
{
    public delegate void NewAudioDataEventHandler(byte[] data);
    class AudioDataProvider : IDisposable
    {
        private readonly INetworkChatCodec codec;
        private readonly WaveIn waveIn;
        public event NewAudioDataEventHandler NewAudioData;

        public AudioDataProvider(INetworkChatCodec codec, int inputDeviceNumber)
        {
            this.codec = codec;
            waveIn = new WaveIn();
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

        void OnAudioCaptured(object sender, WaveInEventArgs e)
        {
            byte[] encoded = codec.Encode(e.Buffer, 0, e.BytesRecorded);
            NewAudioData?.Invoke(encoded);
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