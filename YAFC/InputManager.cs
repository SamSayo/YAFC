using Raylib_cs;
using System.Collections.Generic;

public class InputManager
{
    // Маппинг физических клавиш ПК на виртуальные ID кнопок (0-5)
    private readonly Dictionary<int, KeyboardKey> _buttonMap = new()
    {
        { 0, KeyboardKey.Left },
        { 1, KeyboardKey.Right },
        { 2, KeyboardKey.Up },
        { 3, KeyboardKey.Down },
        { 4, KeyboardKey.Z }, // Кнопка "A" в виртуальной консоли
        { 5, KeyboardKey.X }  // Кнопка "B" в виртуальной консоли
    };

    // Метод для Lua: удерживается ли кнопка
    public bool IsBtnDown(int buttonId)
    {
        if (_buttonMap.TryGetValue(buttonId, out var key))
        {
            return Raylib.IsKeyDown(key);
        }
        return false;
    }

    // Метод для Lua: была ли кнопка только что нажата
    public bool IsBtnPressed(int buttonId)
    {
        if (_buttonMap.TryGetValue(buttonId, out var key))
        {
            return Raylib.IsKeyPressed(key);
        }
        return false;
    }
}