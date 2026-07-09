using Raylib_cs;
using System;

namespace YAFC
{
    public class AudioSynthesizer
    {
        private const int SampleRate = 44100;
        private AudioStream _stream;

        public ChannelConfig Pulse1 = new() { Type = WaveType.Pulse, Duty = 0.5f };
        public ChannelConfig Pulse2 = new() { Type = WaveType.Pulse, Duty = 0.25f };
        public ChannelConfig Triangle = new() { Type = WaveType.Triangle };
        public ChannelConfig Noise = new() { Type = WaveType.Noise };

        public enum WaveType { Pulse, Triangle, Noise}

        public class ChannelConfig
        {
            public WaveType Type;
            public float Frequency = 0f;
            public float Volume = 0f;
            public float Duty = 0.5f;
            public float Phase = 0f;

            public Random rand = new();
            public float LastNoiseVal = 0f;
        }

        public void Init()
        {
            Raylib.InitAudioDevice();
            _stream = Raylib.LoadAudioStream(SampleRate, 16, 1);
            Raylib.PlayAudioStream(_stream);
        }

        public void UpdateStream()
        {
            if (!Raylib.IsAudioStreamProcessed(_stream)) return;

            int samplesToWrite = 1024;
            short[] buffer = new short[samplesToWrite];

            for (int i = 0; i < samplesToWrite; i++)
            {
                float mixedSample = 0f;

                mixedSample += GenerateSample(Pulse1);
                mixedSample += GenerateSample(Pulse2);
                mixedSample += GenerateSample(Triangle);
                mixedSample += GenerateSample(Noise);

                mixedSample = Math.Clamp(mixedSample, -1.0f, 1.0f);

                buffer[i] = (short)(mixedSample * 32767f);
            }

            unsafe
            {
                fixed (short* pBuffer = buffer)
                {
                    Raylib.UpdateAudioStream(_stream, pBuffer, samplesToWrite);
                }
            }
        }

        private float GenerateSample(ChannelConfig ch)
        {
            if (ch.Frequency <= 0 || ch.Volume <= 0) return 0;

            float phaseStep = ch.Frequency / SampleRate;
            ch.Phase = (ch.Phase + phaseStep) % 1.0f;

            switch (ch.Type)
            {
                case WaveType.Pulse:
                    float pulseVal = (ch.Phase < ch.Duty) ? 1.0f : -1.0f;
                    return pulseVal * ch.Volume * 0.25f;

                case WaveType.Triangle:
                    float triVal = 0f;
                    if (ch.Phase < 0.5f)
                        triVal = -1.0f + (ch.Phase * 4.0f);
                    else
                        triVal = 3.0f - (ch.Phase * 4.0f);
                    return triVal * ch.Volume * 0.3f;

                case WaveType.Noise:
                    if (ch.Phase < phaseStep * 2)
                    {
                        ch.LastNoiseVal = (float)(ch.rand.NextDouble() * 2.0 - 1.0);
                    }
                    return ch.LastNoiseVal * ch.Volume * 0.2f;

                default:
                    return 0f;
            }

        }

        public void Shutdown()
        {
            Raylib.UnloadAudioStream(_stream);
            Raylib.CloseAudioDevice();
        }
    }
}
