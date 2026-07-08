using System;
using System.IO;
using Raylib_cs;
using MoonSharp.Interpreter;
using System.Numerics;

public class ConsoleRuntime
{
    private readonly VramResourceManager _vram = new(16);
    private readonly InputManager _input = new();
    private Script _luaState;

    private Closure _luaInit;
    private Closure _luaUpdate;
    private Closure _luaDraw;

    public void Run(string luaScriptPath, string spriteSheetPath)
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(800, 600, "Yet Another Fantasy Console: " + luaScriptPath);
        RenderTexture2D render = Raylib.LoadRenderTexture(256, 192);
        Raylib.SetTextureFilter(render.Texture, TextureFilter.Point);
        Raylib.SetTargetFPS(60);

        _vram.LoadSpriteSheet(spriteSheetPath);

        UserData.RegisterType<VirtualConsoleApi>();
        _luaState = new Script();

        var api = new VirtualConsoleApi(_vram, _input);
        _luaState.Globals["api"] = api;

        _luaState.DoString(@"
            spr = function(id, x, y, s, fx, fy) api.spr(id, x, y, s or 1, fx or false, fy or false) end
            cls = function(r, g, b) api.cls(r, g, b) end
            print_text = function(t, x, y, size, r, g, b) api.print(t, x, y, size, r, g, b) end
            btn = function(id) return api.btn(id) end
            btnp = function(id) return api.btnp(id) end
        ");

        try
        {
            string scriptCode = File.ReadAllText(luaScriptPath);
            _luaState.DoString(scriptCode);

            _luaInit = _luaState.Globals.Get("init").Function;
            _luaUpdate = _luaState.Globals.Get("update").Function;
            _luaDraw = _luaState.Globals.Get("draw").Function;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка компиляции Lua: {ex.Message}");
            return;
        }

        _luaInit?.Call();

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F))
            {
                Raylib.ToggleFullscreen();
            }

            _luaUpdate?.Call();

            Raylib.BeginTextureMode(render);
                _luaDraw?.Call();
            Raylib.EndTextureMode();

            Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                float scale = MathF.Min((float)Raylib.GetScreenWidth() / render.Texture.Width, (float)Raylib.GetScreenHeight() / render.Texture.Height);
                
                // Integer scale - not used
                //scale = MathF.Max(1.0f, MathF.Floor(scale));

                float renderWidth = render.Texture.Width * scale;
                float renderHeight = render.Texture.Height * scale;

                float renderX = (Raylib.GetScreenWidth() - renderWidth) / 2.0f;
                float renderY = (Raylib.GetScreenHeight() - renderHeight) / 2.0f;

                Rectangle sourceRec = new(0, 0, render.Texture.Width, -render.Texture.Height);
                Rectangle destRec = new Rectangle(renderX, renderY, renderWidth, renderHeight);

            Raylib.DrawTexturePro(render.Texture, sourceRec, destRec, new Vector2(0, 0), 0.0f, Color.White);
            Raylib.EndDrawing();
        }

        _vram.Unload();
        Raylib.CloseWindow();
    }
}