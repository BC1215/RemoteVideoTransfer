﻿using NAudio.Wave;

namespace RemoteVideoTransfer.TestClient.Audio.Codec
{
    class MicrosoftAdpcmChatCodec : AcmChatCodec
    {
        public MicrosoftAdpcmChatCodec()
            : base(new WaveFormat(8000, 16, 1), new AdpcmWaveFormat(8000, 1))
        {
        }

        public override string Name => "Microsoft ADPCM";
    }
}
