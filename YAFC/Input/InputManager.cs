using Raylib_cs;

namespace YAFC.Input
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
            { 5, KeyboardKey.X },
            { 6, KeyboardKey.Enter },
            { 7, KeyboardKey.RightShift }
        };

        private readonly HashSet<int> _pending = new();
        private readonly HashSet<int> _stepPressed = new();

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
            return _stepPressed.Contains(buttonId);
        }

        public void Poll()
        {
            foreach (var (id, key) in _buttonMap)
            {
                if (Raylib.IsKeyPressed(key))
                    _pending.Add(id);
            }
        }

        public void BeginStep()
        {
            _stepPressed.Clear();

            foreach (int id in _pending)
                _stepPressed.Add(id);

            _pending.Clear();
        }

        public void Reset()
        {
            _pending.Clear();
            _stepPressed.Clear();
        }
    }
}