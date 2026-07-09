using MoonSharp.Interpreter;
using Raylib_cs;

namespace YAFC
{
    [MoonSharpUserData]
    public class VirtualConsoleApi
    {
        private readonly VramResourceManager _vram;
        private readonly InputManager _input;
        private readonly AudioSynthesizer _audio;

        public VirtualConsoleApi(VramResourceManager vram, InputManager input, AudioSynthesizer audio)
        {
            _vram = vram;
            _input = input;
            _audio = audio;
        }

        // --- Graphics ---
        public void spr(int id, float x, float y, float scale = 1.0f, bool flipX = false, bool flipY = false)
            => _vram.DrawSprite(id, x, y, scale, flipX, flipY);

        public void cls(byte r, byte g, byte b)
            => Raylib.ClearBackground(new Color(r, g, b, (byte)255));

        public void print(string text, float x, float y, int fontSize, byte r, byte g, byte b)
            => Raylib.DrawText(text, (int)x, (int)y, fontSize, new Color(r, g, b, (byte)255));

        // --- Input ---
        public bool btn(int id) => _input.IsBtnDown(id);
        public bool btnp(int id) => _input.IsBtnPressed(id);

        // --- Audio ---
        public void sound(int ch, float freq, float vol, float duty = 0.5f)
        {
            vol = Math.Clamp(vol, 0f, 1f);

            if (ch == 1)
            {
                _audio.Pulse1.Frequency = freq;
                _audio.Pulse1.Volume = vol;
                _audio.Pulse1.Duty = duty;
            }
            else if (ch == 2)
            {
                _audio.Pulse2.Frequency = freq;
                _audio.Pulse2.Volume = vol;
                _audio.Pulse2.Duty = duty;
            }
            else if (ch == 3)
            {
                _audio.Triangle.Frequency = freq;
                _audio.Triangle.Volume = vol;
                _audio.Triangle.Duty = duty;
            }
            else if (ch == 4)
            {
                _audio.Noise.Frequency = freq;
                _audio.Noise.Volume = vol;
                _audio.Noise.Duty = duty;
            }
        }

    }
}