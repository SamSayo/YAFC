using MoonSharp.Interpreter;
using YAFC.GameCarts;
using YAFC.LuaApi.Helpers;

namespace YAFC.LuaApi.Extensions
{
    public static class LuaApiExtensions
    {
        public static string StringArg(CallbackArguments args, int index, string func)
            => args.AsType(index, func, DataType.String, false).String;

        public static int IntArg(CallbackArguments args, int index, string func)
        {
            double d = args.AsType(index, func, DataType.Number, false).Number;
            if (d != Math.Floor(d) || d < 0 || d > int.MaxValue)
            {
                throw new ScriptRuntimeException($"bad argument #{index + 1} to '{func}' (non-negative integer expected)");
            }
            return (int)d;
        }

        public static byte ByteArg(CallbackArguments args, int index, string func)
        {
            double d = args.AsType(index, func, DataType.Number, false).Number;
            if (d != Math.Floor(d) || d < 0 || d > byte.MaxValue)
            {
                throw new ScriptRuntimeException($"bad argument #{index + 1} to '{func}' (non-negative byte expected)");
            }
            return (byte)d;
        }

        public static int WholeNumber(CallbackArguments args, int index, string func)
        {
            double d = args.AsType(index, func, DataType.Number, false).Number;

            if (double.IsNaN(d) || d != Math.Floor(d) || d < int.MinValue || d > int.MaxValue)
                throw new ScriptRuntimeException($"bad argument #{index + 1} to '{func}' (integer excepted)");

            return (int)d;
        }

        public static int FlooredNumber(CallbackArguments args, int index, string func)
        {
            double d = args.AsType(index, func, DataType.Number, false).Number;

            if (double.IsNaN(d))
                throw new ScriptRuntimeException($"bad argument #{index + 1} to '{func}' (number excepted)");

            return (int)Math.Clamp(Math.Floor(d), -1_000_000_000d, 1_000_000_000d);
        }

        public static DynValue Guard(Func<DynValue> action)
        {
            try
            {
                return action();
            }
            catch (KeyNotFoundException e) { return LuaApiResult.Fail(e.Message); }
            catch (InvalidOperationException e) { return LuaApiResult.Fail(e.Message); }
            catch (ArgumentException e) { return LuaApiResult.Fail(e.Message); }
            catch (GameCartException e) { return LuaApiResult.Fail(e.Message); }
            catch (FileNotFoundException) { return LuaApiResult.Fail("file not found"); }
            catch (DirectoryNotFoundException) { return LuaApiResult.Fail("file not found"); }
            catch (Exception) { return LuaApiResult.Fail("file not found"); }
        }
    }
}
