using MoonSharp.Interpreter;
using YAFC.LuaApi.Extensions;
using YAFC.LuaApi.Helpers;
using YAFC.Physics;
using YAFC.Video;

namespace YAFC.LuaApi
{
    public sealed class LuaPhysicsApi : ILuaApi
    {
        private readonly Script _script;
        private readonly VramResourceManager _vram;
        private readonly PhysEngine _phys;

        public LuaPhysicsApi(LuaApiContext ctx)
        {
            _script = ctx.Script;
            _vram = ctx.Vram;
            _phys = ctx.Phys;
        }

        public void Register()
        {
            _script.Globals.Set("move_and_slide", DynValue.FromObject(_script, (Func<float, float, float, float, float, float, MoveResult>)((x, y, w, h, vx, vy)=>_phys.MoveAndSlide(new Rect(x, y, w, h), vx, vy))));

            _script.Globals.Set("set_tile_solid", DynValue.NewCallback(set_tile_solid));
            _script.Globals.Set("get_tile_solid", DynValue.NewCallback(get_tile_solid));
            _script.Globals.Set("solid_rect", DynValue.NewCallback(solid_rect));
            _script.Globals.Set("clear_solids", DynValue.NewCallback(clear_solids));
        }

        private DynValue set_tile_solid(ScriptExecutionContext ctx, CallbackArguments args)
        {
            if (args.Count < 3)
                LuaApiResult.Fail("not enough arguments. expected tileX, tileY, isSolid");

            int tileX = LuaApiExtensions.IntArg(args, 0, "set_tile_solid");
            int tileY = LuaApiExtensions.IntArg(args, 1, "set_tile_solid");
            bool isSolid = args[2].CastToBool();

            _vram.SetTileSolid(tileX, tileY, isSolid);

            return DynValue.Nil;
        }

        private DynValue get_tile_solid(ScriptExecutionContext ctx, CallbackArguments args)
        {
            if (args.Count < 2)
                LuaApiResult.Fail("not enough arguments. expected tileX, tileY");

            int tileX = LuaApiExtensions.IntArg(args, 0, "get_tile_solid");
            int tileY = LuaApiExtensions.IntArg(args, 1, "get_tile_solid");

            return DynValue.NewBoolean(_vram.IsTileSolid(tileX, tileY));
        }

        private DynValue solid_rect(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int tileX = LuaApiExtensions.IntArg(args, 0, "solid_rect");
            int tileY = LuaApiExtensions.IntArg(args, 1, "solid_rect");
            int width = LuaApiExtensions.IntArg(args, 2, "solid_rect");
            int height = LuaApiExtensions.IntArg(args, 3, "solid_rect");
            bool isSolid = args.Count <= 4 || args[4].CastToBool();

            if ((long)width * height > 1_048_576)
                throw new ScriptRuntimeException("solid_rect: area is too large");

            _vram.FillSolid(tileX, tileY, width, height, isSolid);

            return DynValue.Nil;
        }

        private DynValue clear_solids(ScriptExecutionContext ctx, CallbackArguments args)
        {
            _vram.ClearSolids();

            return DynValue.Nil;
        }
    }
}
