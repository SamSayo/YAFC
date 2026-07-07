using System;
using System.IO;
using Raylib_cs;
using MoonSharp.Interpreter;

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
        // 1. Инициализация Raylib окна
        Raylib.InitWindow(800, 600, "Fantasy Console Runtime (Raylib-cs)");
        Raylib.SetTargetFPS(60);

        _vram.LoadSpriteSheet(spriteSheetPath);

        // 2. Инициализация MoonSharp
        UserData.RegisterType<VirtualConsoleApi>();
        _luaState = new Script();

        // Пробрасываем наше API под глобальным именем "api" или регистрируем методы напрямую
        var api = new VirtualConsoleApi(_vram, _input);
        _luaState.Globals["api"] = api;

        // Для удобства (чтобы писать spr() вместо api.spr()) сделаем прокси в Lua-окружении
        _luaState.DoString(@"
            spr = function(id, x, y, s, fx, fy) api.spr(id, x, y, s or 1, fx or false, fy or false) end
            cls = function(r, g, b) api.cls(r, g, b) end
            print_text = function(t, x, y, size, r, g, b) api.print(t, x, y, size, r, g, b) end
            btn = function(id) return api.btn(id) end
            btnp = function(id) return api.btnp(id) end
        ");

        // 3. Загрузка и компиляция пользовательского Lua-скрипта
        try
        {
            string scriptCode = File.ReadAllText(luaScriptPath);
            _luaState.DoString(scriptCode);

            // Вытягиваем ссылки на обязательные функции игры
            _luaInit = _luaState.Globals.Get("init").Function;
            _luaUpdate = _luaState.Globals.Get("update").Function;
            _luaDraw = _luaState.Globals.Get("draw").Function;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка компиляции Lua: {ex.Message}");
            return;
        }

        // Вызов стартовой инициализации Lua (если она есть)
        _luaInit?.Call();

        // 4. Главный игровой цикл
        while (!Raylib.WindowShouldClose())
        {
            // Обновление логики (Вызов Lua update)
            _luaUpdate?.Call();

            // Рендеринг (Вызов Lua draw)
            Raylib.BeginDrawing();
            _luaDraw?.Call();
            Raylib.EndDrawing();
        }

        // Освобождение ресурсов
        _vram.Unload();
        Raylib.CloseWindow();
    }
}