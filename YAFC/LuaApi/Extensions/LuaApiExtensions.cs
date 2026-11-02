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
