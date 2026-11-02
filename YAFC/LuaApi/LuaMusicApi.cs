using MoonSharp.Interpreter;
using YAFC.Audio;
using YAFC.GameCarts;
using YAFC.LuaApi.Extensions;
using YAFC.LuaApi.Helpers;

namespace YAFC.LuaApi
{
    public sealed class LuaMusicApi : ILuaApi
    {
        private readonly Script _script;
        private readonly AudioManager _audio;
        private readonly CartFileSystem _files;

        public LuaMusicApi(LuaApiContext ctx)
        {
            _script = ctx.Script;
            _audio = ctx.Audio;
            _files = ctx.Files;
        }

        public void Register()
        {
            var music = new Table(_script);
            music.Set("load", DynValue.NewCallback(Load));
            music.Set("unload", DynValue.NewCallback(Unload));
            music.Set("play", DynValue.NewCallback(Play));
            music.Set("stop", DynValue.NewCallback(Stop));
            music.Set("pause", DynValue.NewCallback(Pause));
            music.Set("resume", DynValue.NewCallback(Resume));
            music.Set("volume", DynValue.NewCallback(Volume));
            music.Set("loop", DynValue.NewCallback(Loop));
            music.Set("playing", DynValue.NewCallback(Playing));
            music.Set("paused", DynValue.NewCallback(Paused));
            _script.Globals.Set("music", DynValue.NewTable(music));
        }

        private DynValue Load(ScriptExecutionContext ctx, CallbackArguments args)
        {
            string path = LuaApiExtensions.StringArg(args, 0, "music.load");

            return LuaApiExtensions.Guard(() => DynValue.NewNumber(_audio.LoadXm(_files.ReadBytes(path))));
        }

        private DynValue Unload(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "music.unload");

            return LuaApiExtensions.Guard(() => { _audio.UnloadXm(id); return DynValue.True; });
        }

        private DynValue Play(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "music.play");

            return LuaApiExtensions.Guard(() => { _audio.PlayXm(id); return DynValue.True; });
        }

        private DynValue Stop(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "music.stop");

            return LuaApiExtensions.Guard(() => { _audio.StopXm(id); return DynValue.True; });
        }

        private DynValue Pause(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "music.pause");

            return LuaApiExtensions.Guard(() => { _audio.PauseXm(id); return DynValue.True; });
        }

        private DynValue Resume(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "music.resume");

            return LuaApiExtensions.Guard(() => { _audio.ResumeXm(id); return DynValue.True; });
        }

        private DynValue Volume(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "music.volume");

            if (args.Count > 1 && !args[1].IsNil())
            {
                double v = args.AsType(1, "music.volume", DataType.Number, false).Number;

                if (double.IsNaN(v))
                {
                    throw new ScriptRuntimeException("bad argument #2 to 'music.volume' (number excepted, got nan)");
                }

                return LuaApiExtensions.Guard(() => { _audio.SetXmVolume(id, (float)v); return DynValue.True; });
            }

            return LuaApiExtensions.Guard(() => DynValue.NewNumber(_audio.GetXmVolume(id)));
        }

        private DynValue Loop(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "music.loop");

            if (args.Count > 1 && !args[1].IsNil())
            {
                bool flag = args.AsType(1, "music.loop", DataType.Boolean, false).Boolean;

                return LuaApiExtensions.Guard(() => { _audio.SetXmLooping(id, flag); return DynValue.True; });
            }

            return LuaApiExtensions.Guard(() => DynValue.NewBoolean(_audio.IsXmLooping(id)));
        }

        private DynValue Playing(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "music.playing");

            return LuaApiExtensions.Guard(() => DynValue.NewBoolean(_audio.IsXmPlaying(id)));
        }

        private DynValue Paused(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "music.paused");

            return LuaApiExtensions.Guard(() => DynValue.NewBoolean(_audio.IsXmPaused(id)));
        }
    }
}
