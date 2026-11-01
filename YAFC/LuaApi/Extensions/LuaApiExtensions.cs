using MoonSharp.Interpreter;

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

        public static int IdArg(CallbackArguments args, int index, string func)
        {
            double d = args.AsType(index, func, DataType.Number, false).Number;

            if (d != Math.Floor(d) || d < 0 || d > int.MaxValue)
            {
                throw new ScriptRuntimeException($"bad argument #{index + 1} to '{func}' (non-negative integer excepted)");
            }

            return (int)d;
        }
    }
}
