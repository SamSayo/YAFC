using ImGuiNET;
using MoonSharp.Interpreter;
using NativeFileDialogSharp;
using Raylib_cs;
using rlImGui_cs;
using System.Diagnostics;
using System.Numerics;
using YAFC.Audio;
using YAFC.GameCarts;
using YAFC.Physics;
using YAFC.Video;

namespace YAFC
{
    public class ConsoleRuntime
    {
        private readonly VramResourceManager _vram = new(16);
        private readonly InputManager _input = new();
        private readonly AudioSynthesizer _audio = new();
        private readonly PhysEngine _physEngine;
        private Script _luaState;

        private Closure _luaInit;
        private Closure _luaUpdate;
        private Closure _luaDraw;

        public ConsoleRuntime()
        {
            _physEngine = new PhysEngine(_vram.IsTileSolid);
            UserData.RegisterType<MoveResult>();
        }

        public void Menu()
        {
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            Raylib.InitWindow(850, 600, "Yet Another Fantasy Console");

            rlImGui.Setup(true);

            Shader noiseShader = Raylib.LoadShader(null, "resources\\tv.shdr");

            int timeLoc = Raylib.GetShaderLocation(noiseShader, "uTime");

            Raylib.InitAudioDevice();
            Raylib.SetTargetFPS(30);

            //Sound errorSound = Raylib.LoadSound("resources\\error.ogg");
            string cartPath = string.Empty;
            bool canBreak = false;

            string errDesc = string.Empty;

            while (!Raylib.WindowShouldClose())
            {
                float time = (float)Raylib.GetTime();

                unsafe
                {
                    Raylib.SetShaderValue(noiseShader, timeLoc, &time, ShaderUniformDataType.Float);
                }

                Raylib.BeginDrawing();
                Raylib.BeginShaderMode(noiseShader);

                Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), Color.White);

                Raylib.EndShaderMode();
                
                rlImGui.Begin();
                if (!canBreak)
                {
                    ImGui.Begin("Menu");
                    ImGui.Text("Error:" + errDesc);
                    if (ImGui.Button("Load GameCart"))
                    {
                        DialogResult result = Dialog.FileOpen("yafc,YAFC", null);

                        if (result.IsOk)
                        {
                            cartPath = result.Path;

                            FileInfo cartInfo = new FileInfo(cartPath);
                            if (cartInfo.Length > 2.5 * 1024 * 1024)
                            {
                                errDesc = "GameCart is too chonky!";
                                canBreak = false;
                            }
                            else if (cartInfo.Length <= 0)
                            {
                                errDesc = "GameCart is broken or is empty!";
                                canBreak = false;
                            }
                            else
                            {
                                canBreak = true;
                            }
                        }
                        else if (result.IsError)
                        {
                            Console.WriteLine($"Error: {result.ErrorMessage}");
                        }
                    }
                    ImGui.End();
                }
                rlImGui.End();

                Raylib.DrawText("Click on this window with left click to choose cartridge", 10, 10, 30, Color.RayWhite);
                
                Raylib.EndDrawing();

                if (canBreak) break;
            }

            rlImGui.Shutdown();
            Raylib.UnloadShader(noiseShader);
            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
            if (canBreak)
            {
                CartridgeManager cartridge = new();
                Run(cartridge.OpenCartridge(cartPath));
            }
        }

        public void Run(LoadedGameCart cartridge)
        {
            if (cartridge.luaCode == null)
                Environment.Exit(0);

            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            Raylib.InitWindow(850, 600, "Yet Another Fantasy Console");
            RenderTexture2D render = Raylib.LoadRenderTexture(256, 192);
            Raylib.SetTextureFilter(render.Texture, TextureFilter.Point);
            Raylib.SetTargetFPS(60);
            _audio.Init();

            _vram.LoadSpriteSheet(cartridge.spriteSheet);
            
            UserData.RegisterType<VirtualConsoleApi>();
            _luaState = new Script();
            
            var api = new VirtualConsoleApi(_vram, _input, _audio, _physEngine);
            _luaState.Globals["api"] = api;

            _luaState.DoString(@"
                spr = function(id, x, y, s, fx, fy) api.spr(id, x, y, s or 1, fx or false, fy or false) end
                cls = function(r, g, b) api.cls(r, g, b) end
                tile = function(id, x, y) api.tile(id, x, y) end
                print_text = function(t, x, y, size, r, g, b) api.print(t, x, y, size, r, g, b) end
                btn = function(id) return api.btn(id) end
                btnp = function(id) return api.btnp(id) end
                sound = function(ch, freq, vol, duty) api.sound(ch, freq, vol, duty or 0.5) end
                move_and_slide = function(x, y, w, h, vx, vy) return api.move_and_slide(x, y, w, h, vx, vy) end
                set_tile_solid = function(x, y, solid) return api.set_tile_solid(x, y, solid) end
                get_tile_solid = function(x, y) return api.get_tile_solid(x, y) end
            ");

            try
            {
                string scriptCode = cartridge.luaCode;
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

            Stopwatch stopwatch = Stopwatch.StartNew();
            double lastTime = 0;

            while (!Raylib.WindowShouldClose())
            {
                double currentTime = stopwatch.Elapsed.TotalSeconds;

                float deltaTime = (float)(currentTime - lastTime);
                lastTime = currentTime;

                if (deltaTime > 0.1f) deltaTime = 0.1f;

                _luaState.Globals["deltaTime"] = deltaTime;

                if (Raylib.IsKeyPressed(KeyboardKey.F))
                {
                    Raylib.ToggleFullscreen();
                }

                _luaUpdate?.Call();
                _audio.UpdateStream();

                Raylib.BeginTextureMode(render);
                _luaDraw?.Call();
                Raylib.EndTextureMode();

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                float scale = MathF.Min((float)Raylib.GetScreenWidth() / render.Texture.Width, (float)Raylib.GetScreenHeight() / render.Texture.Height);

                // Integer scale
                scale = MathF.Max(1.0f, MathF.Floor(scale));

                float renderWidth = render.Texture.Width * scale;
                float renderHeight = render.Texture.Height * scale;

                float renderX = (Raylib.GetScreenWidth() - renderWidth) / 2.0f;
                float renderY = (Raylib.GetScreenHeight() - renderHeight) / 2.0f;

                Rectangle sourceRec = new(0, 0, render.Texture.Width, -render.Texture.Height);
                Rectangle destRec = new Rectangle(renderX, renderY, renderWidth, renderHeight);

                Raylib.DrawTexturePro(render.Texture, sourceRec, destRec, new Vector2(0, 0), 0.0f, Color.White);
                Raylib.EndDrawing();
            }

            _audio.Shutdown();
            _vram.Unload();
            Raylib.CloseWindow();
        }
    }
}
