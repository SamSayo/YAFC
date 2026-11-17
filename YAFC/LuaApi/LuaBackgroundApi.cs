using MoonSharp.Interpreter;
using YAFC.Video;

namespace YAFC.LuaApi
{
    public sealed class LuaBackgroundApi : ILuaApi
    {
        private readonly Script _script;
        private readonly VramResourceManager _vram;

        public LuaBackgroundApi(LuaApiContext ctx)
        {
            _script = ctx.Script;
            _vram = ctx.Vram;
        }

        public void Register()
        {
            _script.Globals.Set("bg_set", DynValue.NewCallback(bg_set));
            _script.Globals.Set("bg_clear", DynValue.NewCallback(bg_clear));
            _script.Globals.Set("bg_scroll", DynValue.NewCallback(bg_scroll));
            _script.Globals.Set("bg_draw", DynValue.NewCallback(bg_draw));
        }


    }
}
