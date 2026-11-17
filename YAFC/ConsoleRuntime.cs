using ImGuiNET;
using MoonSharp.Interpreter;
using NativeFileDialogSharp;
using Raylib_cs;
using rlImGui_cs;
using System.Numerics;

using YAFC.Audio;
using YAFC.GameCarts;
using YAFC.Physics;
using YAFC.Video;
using YAFC.LuaApi;
using YAFC.Input;

namespace YAFC
{
    enum ConsoleScreen
    {
        Menu = 0,
        Logo,
        Run
    }

    public class ConsoleRuntime
    {
        // Constants
        private const int WindowW = 850;
        private const int WindowH = 600;

        private const int ScreenW = 256;
        private const int ScreenH = 192;

        private const long MaxCartSize = 2_621_440;

        private const float LogoDuration = 2.5f;
        private const float FlushInterval = 5f;

        private const int MaxTransitionsPerFrame = 8;

        private const float StepTime = 1f / 60f;
        private const int MaxStepsPerFrame = 5;

        // Subsystems
        private readonly VramResourceManager _vram = new(16, ScreenW, ScreenH);
        private readonly InputManager _input = new();
        private readonly AudioManager _audio = new();
        private readonly PhysEngine _physEngine;

        // State Machine
        private ConsoleScreen _currentScreen = ConsoleScreen.Menu;
        private ConsoleScreen? _pendingScreen;
        private bool _quit;

        // Common state
        private string _errDesc = string.Empty;
        private string? _cartPath;
        private LoadedGameCart _gameCart;

        // Graphics resources
        private Shader _noiseShader;
        private int _timeLoc;
        private RenderTexture2D _renderTarget;

        // Logo state
        private float _logoTimer;


        // GameCart runtime state
        private Script _luaState;
        private Closure _luaUpdate;
        private Closure _luaDraw;
        private float _flushTimer;
        private float _stepAccumulator;

        // Init flags
        private bool _windowInitialized;
        private bool _imguiInitializationAttempted;
        private bool _audioInitializationAttempted;
        private bool _shaderLoaded;
        private bool _renderTargetLoaded;
        private bool _vramLoadAttempted;

        public ConsoleRuntime()
        {
            _physEngine = new PhysEngine(_vram.IsTileSolid);
            UserData.RegisterType<MoveResult>();
        }

        public void Start(string? cartPath = null)
        {
            try
            {
                InitializeSystem();

                if (cartPath != null)
                {
                    _cartPath = cartPath;
                    _currentScreen = ConsoleScreen.Logo;
                }


                EnterState(_currentScreen);
                ApplyPendingTransitions();

                while (!_quit && !Raylib.WindowShouldClose())
                {
                    float deltaTime = MathF.Min(Raylib.GetFrameTime(), 0.1f);

                    ApplyPendingTransitions();

                    UpdateCurrentState(deltaTime);
                    ApplyPendingTransitions();

                    if (_quit)
                        break;

                    Raylib.BeginDrawing();
                    try
                    {
                        DrawCurrentState();
                    }
                    finally
                    {
                        Raylib.EndDrawing();
                    }
                }
            }
            finally
            {
                try
                {
                    ExitState(_currentScreen);
                }
                finally
                {
                    ShutdownSystem();
                }
            }
        }

        private void ChangeState(ConsoleScreen next)
        {
            _pendingScreen = next;
        }

        private void ApplyPendingTransitions()
        {
            int transitions = 0;

            while (_pendingScreen.HasValue)
            {
                if (++transitions > MaxTransitionsPerFrame)
                {
                    throw new InvalidOperationException("Too many state transitions in one frame.");
                }

                ConsoleScreen next = _pendingScreen.Value;
                _pendingScreen = null;

                if (next == _currentScreen)
                    continue;

                ExitState(_currentScreen);
                _currentScreen = next;
                EnterState(_currentScreen);
            }
        }

        private void EnterState(ConsoleScreen screen)
        {
            switch (screen)
            {
                case ConsoleScreen.Menu:
                    EnterMenu();
                    break;

                case ConsoleScreen.Logo:
                    EnterLogo();
                    break;

                case ConsoleScreen.Run:
                    EnterRun();
                    break;
            }
        }

        private void ExitState(ConsoleScreen screen)
        {
            switch (screen)
            {
                case ConsoleScreen.Run:
                    ExitRun();
                    break;
            }
        }

        private void UpdateCurrentState(float dt)
        {
            switch (_currentScreen)
            {
                case ConsoleScreen.Menu:
                    UpdateMenu();
                    break;

                case ConsoleScreen.Logo:
                    UpdateLogo(dt);
                    break;

                case ConsoleScreen.Run:
                    UpdateRun(dt);
                    break;
            }
        }

        private void DrawCurrentState()
        {
            switch (_currentScreen)
            {
                case ConsoleScreen.Menu:
                    DrawMenu();
                    break;

                case ConsoleScreen.Logo:
                    DrawLogo();
                    break;

                case ConsoleScreen.Run:
                    DrawRun();
                    break;
            }
        }

        private void InitializeSystem()
        {
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            Raylib.InitWindow(WindowW, WindowH, "Yet Another Fantasy Console");
            _windowInitialized = true;

            Raylib.SetExitKey(KeyboardKey.Null);
            Raylib.SetTargetFPS(60);

            _imguiInitializationAttempted = true;
            rlImGui.Setup(true);

            string shaderPath = Path.Combine(AppContext.BaseDirectory, "resources", "tv.shdr");

            _noiseShader = Raylib.LoadShader(null, shaderPath);
            _shaderLoaded = _noiseShader.Id != 0;
            _timeLoc = Raylib.GetShaderLocation(_noiseShader, "uTime");

            _renderTarget = Raylib.LoadRenderTexture(ScreenW, ScreenH);
            _renderTargetLoaded = _renderTarget.Id != 0;

            if (_renderTargetLoaded)
                Raylib.SetTextureFilter(_renderTarget.Texture, TextureFilter.Point);

            _audioInitializationAttempted = true;
            _audio.Initialize();
        }

        private void ShutdownSystem()
        {
            if (_imguiInitializationAttempted)
            {
                try
                {
                    rlImGui.Shutdown();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ImGui shutdown error: {ex.Message}");
                }

                _imguiInitializationAttempted = false;
            }

            if (_shaderLoaded)
            {
                try
                {
                    Raylib.UnloadShader(_noiseShader);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Shader unload error: {ex.Message}");
                }

                _shaderLoaded = false;
                _noiseShader = default;
            }

            if (_renderTargetLoaded)
            {
                try
                {
                    Raylib.UnloadRenderTexture(_renderTarget);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RenderTexture unload error: {ex.Message}");
                }

                _renderTargetLoaded = false;
                _renderTarget = default;
            }

            if (_audioInitializationAttempted)
            {
                try
                {
                    _audio.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Audio shutdown error: {ex.Message}");
                }

                _audioInitializationAttempted = false;
            }

            if (_windowInitialized)
            {
                try
                {
                    Raylib.CloseWindow();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Window shutdown error: {ex.Message}");
                }

                _windowInitialized = false;
            }
        }

        #region Menu
        private void EnterMenu()
        {
            _gameCart = null;
            _cartPath = null;

            Raylib.SetWindowTitle("YAFC - You are in Menu!");
        }

        private void UpdateMenu()
        {
            float time = (float)Raylib.GetTime();

            unsafe
            {
                Raylib.SetShaderValue(_noiseShader, _timeLoc, &time, ShaderUniformDataType.Float);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                _quit = true;
        }

        private void DrawMenu()
        {
            Raylib.BeginShaderMode(_noiseShader);
                Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), Color.White);
            Raylib.EndShaderMode();

            rlImGui.Begin();
            ImGui.Begin("Menu");

            if (!string.IsNullOrEmpty(_errDesc))
                ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), "Error: " + _errDesc);

            if (ImGui.Button("Load GameCart"))
                TrySelectCartridge();

            ImGui.End();
            rlImGui.End();

            Raylib.DrawText("Click 'Load GameCart to choose a cartridge", 10, 10, 24, Color.Black);
        }

        private void TrySelectCartridge()
        {
            DialogResult result = Dialog.FileOpen("yafc,YAFC", null);

            if (result.IsError)
            {
                _errDesc = result.ErrorMessage;
                Console.WriteLine($"File dialog error: {result.ErrorMessage}");
                return;
            }

            if (!result.IsOk)
                return;

            try
            {
                FileInfo info = new(result.Path);

                if (info.Length > MaxCartSize)
                {
                    _errDesc = "GameCart is too chonky!";
                    return;
                }

                if (info.Length <= 0)
                {
                    _errDesc = "GameCart is broken or is empty!";
                    return;
                }

                _errDesc = string.Empty;
                _cartPath = result.Path;
                ChangeState(ConsoleScreen.Logo);
            }
            catch (Exception ex)
            {
                _errDesc = $"Can't access GameCart: {ex.Message}";
                Console.WriteLine(_errDesc);
            }
        }
        #endregion

        #region Logo
        private void EnterLogo()
        {
            _logoTimer = 0f;
            Raylib.SetWindowTitle("YAFC - We are Starting up!");

            try
            {
                if (string.IsNullOrEmpty(_cartPath))
                    throw new InvalidOperationException("No GameCart path was specified.");

                CartridgeManager manager = new();
                _gameCart = manager.OpenCartridge(_cartPath);

                if (_gameCart?.luaCode == null)
                    throw new InvalidDataException("GameCart contains no Lua code!");
            }
            catch (Exception ex)
            {
                _errDesc = $"Can't open GameCart: {ex.Message}";
                Console.WriteLine(_errDesc);
                ChangeState(ConsoleScreen.Menu);
            }
        }

        private void UpdateLogo(float dt)
        {
            _logoTimer += dt;

            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                ChangeState(ConsoleScreen.Menu);
                return;
            }

            bool skip = Raylib.IsKeyPressed(KeyboardKey.Space) ||
                Raylib.IsKeyPressed(KeyboardKey.Enter) ||
                Raylib.IsMouseButtonPressed(MouseButton.Left);

            if (skip || _logoTimer >= LogoDuration)
                ChangeState(ConsoleScreen.Run);
        }

        private void DrawLogo()
        {
            float t = _logoTimer / LogoDuration;

            float alpha = t < 0.3f
                ? t / 0.3f 
                : t > 0.7f
                    ? (1f - t) / 0.3f
                    : 1f;

            alpha = Math.Clamp(alpha, 0f, 1f);

            const string title = "YAFC";
            const int fontSize = 80;

            int width = Raylib.MeasureText(title, fontSize);

            Raylib.ClearBackground(Color.Black);
            Raylib.DrawText(
                title,
                (Raylib.GetScreenWidth() - width) / 2,
                Raylib.GetScreenHeight() / 2 - fontSize / 2,
                fontSize,
                Raylib.Fade(Color.RayWhite, alpha)
                );
        }
        #endregion

        #region Run
        private void EnterRun()
        {
            try
            {
                if (_gameCart?.luaCode == null)
                    throw new InvalidDataException("GameCart contains no Lua code!");

                _vramLoadAttempted = true;
                _vram.LoadSpriteSheet(_gameCart.spriteSheet);

                _luaState = new Script(CoreModules.Preset_SoftSandbox);
                InstallApis(_gameCart);

                _luaState.Globals["deltaTime"] = StepTime;
                _luaState.DoString(_gameCart.luaCode);

                Closure? init = _luaState.Globals.Get("init").Function;
                _luaUpdate = _luaState.Globals.Get("update").Function;
                _luaDraw = _luaState.Globals.Get("draw").Function;

                init?.Call();

                _flushTimer = 0f;
                _stepAccumulator = 0f;
                _input.Reset();
                Raylib.SetWindowTitle($"{_gameCart.meta.Id} | {_gameCart.meta.Name} | {_gameCart.meta.Version} | {_gameCart.meta.CanSave}");
            }
            catch (InterpreterException ex)
            {
                FailRun(ex.DecoratedMessage ?? ex.Message);
            }
            catch (Exception ex)
            {
                FailRun($"Failed to initialize cartridge: {ex.Message}");
            }
        }

        private void UpdateRun(float dt)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                ChangeState(ConsoleScreen.Menu);
                return;
            }

            if (Raylib.IsKeyPressed(KeyboardKey.F))
                Raylib.ToggleFullscreen();

            if (_gameCart == null || _luaState == null)
            {
                FailRun("Cartridge runtime is not initialized");
                return;
            }

            _flushTimer += dt;
            if (_flushTimer >= FlushInterval)
            {
                try
                {
                    _gameCart.save?.FlushIfDirty();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Save flush error: {ex.Message}");
                    _errDesc = $"Save error: {ex.Message}";
                }

                _flushTimer = 0f;
            }

            try
            {
                _input.Poll();

                _stepAccumulator += dt;

                int steps = 0;
                while (_stepAccumulator >= StepTime && steps < MaxStepsPerFrame)
                {
                    _stepAccumulator -= StepTime;
                    steps++;

                    _input.BeginStep();
                    _luaState.Globals["deltaTime"] = StepTime;
                    _luaUpdate?.Call();
                }

                if (_stepAccumulator >= StepTime)
                    _stepAccumulator = 0f;

                _audio.Update();
            }
            catch (InterpreterException ex)
            {
                FailRun($"Runtime Lua Error: {ex.DecoratedMessage ?? ex.Message}");
            }
            catch (Exception ex)
            {
                FailRun($"Runtime Error: {ex.Message}");
            }
        }

        private void DrawRun()
        {
            if (_luaState == null)
                return;

            try
            {
                Raylib.BeginTextureMode(_renderTarget);
                try
                {
                    _luaDraw?.Call();
                }
                finally
                {
                    Raylib.EndTextureMode();
                }

                DrawRenderToScreen();
            }
            catch (InterpreterException ex)
            {
                FailRun($"Draw Lua Error: {ex.DecoratedMessage ?? ex.Message}");
            }
            catch (Exception ex)
            {
                FailRun($"Rendering Error: {ex.Message}");
            }
        }

        private void DrawRenderToScreen()
        {
            Raylib.ClearBackground(Color.Black);

            int sw = Raylib.GetScreenWidth();
            int sh = Raylib.GetScreenHeight();

            float scale = MathF.Min((float)sw / ScreenW, (float)sh / ScreenH);

            scale = MathF.Max(1f, MathF.Floor(scale));

            float renderWidth = ScreenW * scale;
            float renderHeight = ScreenH * scale;

            Rectangle source = new(0, 0, ScreenW, -ScreenH);

            Rectangle destination = new((sw - renderWidth) / 2f, (sh - renderHeight) / 2f, renderWidth, renderHeight);

            Raylib.DrawTexturePro(_renderTarget.Texture, source, destination, Vector2.Zero, 0f, Color.White);
        }

        private void ExitRun()
        {
            try
            {
                _gameCart?.save?.FlushIfDirty();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save flush error during cartridge exit: {ex.Message}");
                _errDesc = $"Save error: {ex.Message}";
            }
            finally
            {
                try
                {
                    if (_vramLoadAttempted)
                        _vram.Unload();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"VRAM unload error: {ex.Message}");
                }
                finally
                {
                    _vramLoadAttempted = false;

                    _luaUpdate = null;
                    _luaDraw = null;
                    _luaState = null;

                    _flushTimer = 0f;
                }
            }
        }

        private void FailRun(string message)
        {
            Console.WriteLine(message);
            _errDesc = message;
            ChangeState(ConsoleScreen.Menu);
        }
        #endregion

        // Helpers
        private void InstallApis(LoadedGameCart cart)
        {
            var ctx = new LuaApiContext
            {
                Script  = _luaState,
                Files   = cart.files,
                Audio   = _audio,
                Input   = _input,
                Phys    = _physEngine,
                Vram    = _vram,
                Save    = cart.save
            };

            ILuaApi[] apis =
                [
                    new LuaCartApi(ctx),
                    new LuaMusicApi(ctx),
                    new LuaPhysicsApi(ctx),
                    new LuaInputApi(ctx),
                    new LuaSynthApi(ctx),
                    new LuaGraphicsApi(ctx),
                    new LuaBackgroundApi(ctx)
                ];

            foreach (var api in apis)
                api.Register();
        }
    }
}
