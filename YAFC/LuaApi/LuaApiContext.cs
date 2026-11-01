using MoonSharp.Interpreter;
using YAFC.Audio;
using YAFC.GameCarts;

namespace YAFC.LuaApi
{
    public sealed class LuaApiContext
    {
        public required Script Script { get; init; }
        public required CartFileSystem Files { get; init; }
        public required AudioManager Audio { get; init; }
        public SaveRam? Save { get; init; }
    }
}
