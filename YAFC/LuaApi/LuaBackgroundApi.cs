using MoonSharp.Interpreter;
using YAFC.LuaApi.Extensions;
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

        private DynValue bg_set(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int tx = LuaApiExtensions.WholeNumber(args, 0, "bg_set");
            int ty = LuaApiExtensions.WholeNumber(args, 1, "bg_set");

            int id = -1;
            if (args.Count > 2 && args[2].IsNotNil())
                id = LuaApiExtensions.WholeNumber(args, 2, "bg_set");

            int flags = 0;
            if (args.Count > 3 && args[3].IsNotNil())
                flags = LuaApiExtensions.WholeNumber(args, 3, "bg_set");

            _vram.Background.Set(tx, ty, id, flags);

            return DynValue.Nil;
        }

        private DynValue bg_clear(ScriptExecutionContext ctx, CallbackArguments args)
        {
            _vram.Background.ClearCells();

            return DynValue.Nil;
        }

        private DynValue bg_scroll(ScriptExecutionContext ctx, CallbackArguments args)
        {
            _vram.Background.ScrollX = LuaApiExtensions.FlooredNumber(args, 0, "bg_scroll");
            _vram.Background.ScrollY = LuaApiExtensions.FlooredNumber(args, 1, "bg_scroll");

            return DynValue.Nil;
        }

        private DynValue bg_draw(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int y0 = 0;
            int y1 = _vram.ScreenHeight;

            if (args.Count > 0 && args[0].IsNotNil())
                y0 = LuaApiExtensions.FlooredNumber(args, 0, "bg_draw");

            if (args.Count > 1 && args[1].IsNotNil())
                y0 = LuaApiExtensions.FlooredNumber(args, 1, "bg_draw");

            _vram.DrawBackground(y0, y1);

            return DynValue.Nil;
        }
    }
}
