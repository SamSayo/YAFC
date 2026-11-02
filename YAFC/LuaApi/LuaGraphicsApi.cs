using MoonSharp.Interpreter;
using Raylib_cs;
using YAFC.LuaApi.Extensions;
using YAFC.Physics;
using YAFC.Video;

namespace YAFC.LuaApi
{
    public sealed class LuaGraphicsApi : ILuaApi
    {
        private readonly Script _script;
        private readonly VramResourceManager _vram;
        private readonly PhysEngine _phys;

        public LuaGraphicsApi(LuaApiContext ctx)
        {
            _script = ctx.Script;
            _vram = ctx.Vram;
            _phys = ctx.Phys;
        }

        public void Register()
        {
            // Graphics
            _script.Globals.Set("spr", DynValue.NewCallback(spr));
            _script.Globals.Set("tile", DynValue.NewCallback(tile));
            _script.Globals.Set("cls", DynValue.NewCallback(cls));
            _script.Globals.Set("print_text", DynValue.NewCallback(printText));
        }

        private DynValue spr(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "spr");
            float x = (float)args.AsType(1, "spr", DataType.Number).Number;
            float y = (float)args.AsType(2, "spr", DataType.Number).Number;

            float scale = 1.0f;
            bool flipX = false;
            bool flipY = false;
            if (args.Count > 3 && args[3].IsNotNil())
            {
                scale = (float)args.AsType(3, "spr", DataType.Number).Number;

                if (args.Count > 4 && args[4].IsNotNil())
                {
                    flipX = args[4].CastToBool();
                    
                    if (args.Count > 5 && args[5].IsNotNil())
                    {
                        flipY = args[5].CastToBool();
                    }
                }
            }

            _vram.DrawSprite(id, x, y, scale, flipX, flipY);

            return DynValue.Nil;
        }

        private DynValue tile(ScriptExecutionContext ctx, CallbackArguments args)
        {
            int id = LuaApiExtensions.IntArg(args, 0, "spr");
            int x = LuaApiExtensions.IntArg(args, 1, "spr");
            int y = LuaApiExtensions.IntArg(args, 2, "spr");

            _vram.DrawTile(id, x, y);

            return DynValue.Nil;
        }

        private DynValue cls(ScriptExecutionContext ctx, CallbackArguments args)
        {
            byte r = LuaApiExtensions.ByteArg(args, 0, "cls");
            byte g = LuaApiExtensions.ByteArg(args, 1, "cls");
            byte b = LuaApiExtensions.ByteArg(args, 2, "cls");

            Raylib.ClearBackground(new Color(r, g, b, (byte)255));

            return DynValue.Nil;
        }

        private DynValue printText(ScriptExecutionContext ctx, CallbackArguments args)
        {
            string text = LuaApiExtensions.StringArg(args, 0, "print_text");
            int x = LuaApiExtensions.IntArg(args, 1, "print_text");
            int y = LuaApiExtensions.IntArg(args, 2, "print_text");
            int fontSize = LuaApiExtensions.IntArg(args, 3, "print_text");
            byte r = LuaApiExtensions.ByteArg(args, 4, "print_text");
            byte g = LuaApiExtensions.ByteArg(args, 5, "print_text");
            byte b = LuaApiExtensions.ByteArg(args, 6, "print_text");

            Raylib.DrawText(text, x, y, fontSize, new Color(r, g, b, (byte)255));

            return DynValue.Nil;
        }
    }
}
