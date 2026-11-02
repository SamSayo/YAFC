using MoonSharp.Interpreter;
using YAFC.Audio;
using YAFC.LuaApi.Extensions;
using YAFC.LuaApi.Helpers;

namespace YAFC.LuaApi
{
    public sealed class LuaSynthApi : ILuaApi
    {
        private readonly Script _script;
        private readonly AudioManager _audio;

        private const float MaxSoundFreq = 22050f;

        public LuaSynthApi(LuaApiContext ctx)
        {
            _script = ctx.Script;
            _audio = ctx.Audio;
        }

        public void Register()
        {
            _script.Globals.Set("sound", DynValue.NewCallback(sound));
        }

        private DynValue sound(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int ch = LuaApiExtensions.IntArg(args, 0, "sound");
            float freq = (float)args.AsType(1, "sound", DataType.Number).Number;
            float vol = (float)args.AsType(2, "sound", DataType.Number).Number;

            float duty = 0.5f;
            if (args.Count > 3 && args[3].IsNotNil())
            {
                duty = (float)args.AsType(3, "sound", DataType.Number).Number;
            }

            freq = float.IsFinite(freq) ? Math.Clamp(freq, 0f, MaxSoundFreq) : 0f;
            vol = float.IsFinite(vol) ? Math.Clamp(vol, 0f, 1f) : 0f;
            duty = float.IsFinite(duty) ? Math.Clamp(duty, 0f, 1f) : 0.5f;

            AudioSynthesizer.ChannelConfig? channel = ch switch
            {
                1 => _audio.Synth.Pulse1,
                2 => _audio.Synth.Pulse2,
                3 => _audio.Synth.Triangle,
                4 => _audio.Synth.Noise,
                _ => null
            };

            if (channel == null) return LuaApiResult.Fail("no channel was selected");

            channel.Frequency = freq;
            channel.Volume = vol;
            channel.Duty = duty;

            return DynValue.Nil;
        }
    }
}
