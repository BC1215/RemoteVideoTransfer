using System;
using NAudio.Wave;
using RemoteVideoTransfer.TestClient.Audio.Interface;

namespace RemoteVideoTransfer.TestClient.Audio
{
    class AudioDataPlayer : IDisposable
    {
        private readonly INetworkChatCodec codec;
        private readonly IWavePlayer waveOut;
        private readonly BufferedWaveProvider waveProvider;

        public AudioDataPlayer(INetworkChatCodec codec)
        {
            this.codec = codec;

            waveOut = new WaveOut();
            waveProvider = new BufferedWaveProvider(codec.RecordFormat);
            waveOut.Init(waveProvider);
            waveOut.Play();
        }

        public void PlayAudioData(byte[] compressed)
        {
            byte[] decoded = codec.Decode(compressed, 0, compressed.Length);
            waveProvider.AddSamples(decoded, 0, decoded.Length);
        }

        public void Dispose()
        {
            waveOut?.Dispose();
        }
    }
}