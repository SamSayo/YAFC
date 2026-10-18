using MoonSharp.Interpreter;
using YAFC.Audio;
using YAFC.GameCarts;

namespace YAFC.LuaApi
{
    public sealed class LuaMusicApi
    {
        private readonly Script _script;
        private readonly AudioManager _audio;
        private readonly CartFileSystem _files;

        private LuaMusicApi(Script script, AudioManager audio, CartFileSystem files)
        {
            _script = script;
            _audio = audio;
            _files = files;
        }

        public static void Install(Script script, AudioManager audio, CartFileSystem files)
            => new LuaMusicApi(script, audio, files).Register();

        private void Register()
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

        // Helpers (they are repeating)
        private static DynValue Fail(string message)
            => DynValue.NewTuple(DynValue.Nil, DynValue.NewString(message));

        private static string StringArg(CallbackArguments args, int index, string func)
            => args.AsType(index, func, DataType.String, false).String;

        private static int IdArg(CallbackArguments args, int index, string func)
        {
            double d = args.AsType(index, func, DataType.Number, false).Number;

            if (d != Math.Floor(d) || d < 0 || d > int.MaxValue)
            {
                throw new ScriptRuntimeException($"bad argument #{index + 1} to '{func}' (non-negative integer excepted)");
            }

            return (int)d;
        }

        private static DynValue Guard(Func<DynValue> action)
        {
            try
            {
                return action();
            }
            catch (KeyNotFoundException e) { return Fail(e.Message); }
            catch (InvalidOperationException e) { return Fail(e.Message); }
            catch (ArgumentException e) { return Fail(e.Message); }
            catch (GameCartException e) { return Fail(e.Message); }
            catch (FileNotFoundException) { return Fail("file not found"); }
            catch (DirectoryNotFoundException) { return Fail("file not found"); }
            catch (Exception) { return Fail("file not found"); }
        }

        private DynValue Load(ScriptExecutionContext ctx, CallbackArguments args)
        {
            string path = StringArg(args, 0, "music.load");

            return Guard(() => DynValue.NewNumber(_audio.LoadXm(_files.ReadBytes(path))));
        }

        private DynValue Unload(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = IdArg(args, 0, "music.unload");

            return Guard(() => { _audio.UnloadXm(id); return DynValue.True; });
        }

        private DynValue Play(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = IdArg(args, 0, "music.play");

            return Guard(() => { _audio.PlayXm(id); return DynValue.True; });
        }

        private DynValue Stop(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = IdArg(args, 0, "music.stop");

            return Guard(() => { _audio.StopXm(id); return DynValue.True; });
        }

        private DynValue Pause(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = IdArg(args, 0, "music.pause");

            return Guard(() => { _audio.PauseXm(id); return DynValue.True; });
        }

        private DynValue Resume(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = IdArg(args, 0, "music.resume");

            return Guard(() => { _audio.ResumeXm(id); return DynValue.True; });
        }

        private DynValue Volume(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = IdArg(args, 0, "music.volume");

            if (args.Count > 1 && !args[1].IsNil())
            {
                double v = args.AsType(1, "music.volume", DataType.Number, false).Number;

                if (double.IsNaN(v))
                {
                    throw new ScriptRuntimeException("bad argument #2 to 'music.volume' (number excepted, got nan)");
                }

                return Guard(() => { _audio.SetXmVolume(id, (float)v); return DynValue.True; });
            }

            return Guard(() => DynValue.NewNumber(_audio.GetXmVolume(id)));
        }

        private DynValue Loop(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = IdArg(args, 0, "music.loop");

            if (args.Count > 1 && !args[1].IsNil())
            {
                bool flag = args.AsType(1, "music.loop", DataType.Boolean, false).Boolean;

                return Guard(() => { _audio.SetXmLooping(id, flag); return DynValue.True; });
            }

            return Guard(() => DynValue.NewBoolean(_audio.IsXmLooping(id)));
        }

        private DynValue Playing(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = IdArg(args, 0, "music.playing");

            return Guard(() => DynValue.NewBoolean(_audio.IsXmPlaying(id)));
        }

        private DynValue Paused(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = IdArg(args, 0, "music.paused");

            return Guard(() => DynValue.NewBoolean(_audio.IsXmPaused(id)));
        }
    }
}
