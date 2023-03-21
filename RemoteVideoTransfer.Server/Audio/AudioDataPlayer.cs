using System;
using NAudio.Wave;
using RemoteVideoTransfer.Data.Audio.Interface;

namespace RemoteVideoTransfer.Server.Audio
{
    class AudioDataPlayer : IDisposable
    {
        private readonly INetworkChatCodec codec;
        private readonly IWavePlayer waveOut;
        public readonly BufferedWaveProvider waveProvider;

        public AudioDataPlayer(INetworkChatCodec codec)
        {
            this.codec = codec;

            waveOut = new WaveOutEvent();
            waveProvider = new BufferedWaveProvider(codec.RecordFormat);
            waveProvider.DiscardOnBufferOverflow = true;
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