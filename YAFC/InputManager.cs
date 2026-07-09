using Raylib_cs;

namespace YAFC
{
    public class InputManager
    {
        private readonly Dictionary<int, KeyboardKey> _buttonMap = new()
    {
        { 0, KeyboardKey.Left },
        { 1, KeyboardKey.Right },
        { 2, KeyboardKey.Up },
        { 3, KeyboardKey.Down },
        { 4, KeyboardKey.Z },
        { 5, KeyboardKey.X }
    };

        public bool IsBtnDown(int buttonId)
        {
            if (_buttonMap.TryGetValue(buttonId, out var key))
            {
                return Raylib.IsKeyDown(key);
            }
            return false;
        }

        public bool IsBtnPressed(int buttonId)
        {
            if (_buttonMap.TryGetValue(buttonId, out var key))
            {
                return Raylib.IsKeyPressed(key);
            }
            return false;
        }
    }
}