using MoonSharp.Interpreter;
using System.Text.Json;
using YAFC.GameCarts;

namespace YAFC.LuaApi.Helpers
{
    public static class LuaApiResult
    {
        public static DynValue Fail(string message)
            => DynValue.NewTuple(DynValue.Nil, DynValue.NewString(message));

        public static DynValue Fail(Exception e) => Fail(e switch
        {
            GameCartException => e.Message,
            FileNotFoundException => "file not found",
            DirectoryNotFoundException => "directory not found",
            JsonException => "json: " + e.Message,
            _ => "io error"
        });
    }
}
