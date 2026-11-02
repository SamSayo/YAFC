using MoonSharp.Interpreter;
using YAFC.Audio;
using YAFC.GameCarts;
using YAFC.Input;
using YAFC.Physics;
using YAFC.Video;

namespace YAFC.LuaApi
{
    public sealed class LuaApiContext
    {
        public required Script Script { get; init; }
        public required CartFileSystem Files { get; init; }
        public required AudioManager Audio { get; init; }
        public required InputManager Input { get; init; }
        public required PhysEngine Phys { get; init; }
        public required VramResourceManager Vram { get; init; }
        public SaveRam? Save { get; init; }
    }
}
