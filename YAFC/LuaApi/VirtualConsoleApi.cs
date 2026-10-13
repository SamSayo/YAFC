using MoonSharp.Interpreter;
using Raylib_cs;
using YAFC.Audio;
using YAFC.Physics;
using YAFC.Video;

namespace YAFC.LuaApi
{
    [MoonSharpUserData]
    public class VirtualConsoleApi
    {
        private readonly VramResourceManager _vram;
        private readonly InputManager _input;
        private readonly AudioManager _audio;
        private readonly PhysEngine _physEngine;

        public VirtualConsoleApi(VramResourceManager vram, InputManager input, AudioManager audio, PhysEngine physEngine)
        {
            _vram = vram;
            _input = input;
            _audio = audio;
            _physEngine = physEngine;
        }

        // --- Graphics ---
        public void spr(int id, float x, float y, float scale = 1.0f, bool flipX = false, bool flipY = false)
            => _vram.DrawSprite(id, x, y, scale, flipX, flipY);
        public void cls(byte r, byte g, byte b)
            => Raylib.ClearBackground(new Color(r, g, b, (byte)255));
        public void print(string text, float x, float y, int fontSize, byte r, byte g, byte b)
            => Raylib.DrawText(text, (int)x, (int)y, fontSize, new Color(r, g, b, (byte)255));
        public void tile(int id, int tileX, int tileY)
            => _vram.DrawTile(id, tileX, tileY);

        // --- Input ---
        public bool btn(int id) => _input.IsBtnDown(id);
        public bool btnp(int id) => _input.IsBtnPressed(id);

        // --- Audio ---
        public void sound(int ch, float freq, float vol, float duty = 0.5f)
        {
            vol = Math.Clamp(vol, 0f, 1f);

            if (ch == 1)
            {
                _audio.Synth.Pulse1.Frequency = freq;
                _audio.Synth.Pulse1.Volume = vol;
                _audio.Synth.Pulse1.Duty = duty;
            }
            else if (ch == 2)
            {
                _audio.Synth.Pulse2.Frequency = freq;
                _audio.Synth.Pulse2.Volume = vol;
                _audio.Synth.Pulse2.Duty = duty;
            }
            else if (ch == 3)
            {
                _audio.Synth.Triangle.Frequency = freq;
                _audio.Synth.Triangle.Volume = vol;
                _audio.Synth.Triangle.Duty = duty;
            }
            else if (ch == 4)
            {
                _audio.Synth.Noise.Frequency = freq;
                _audio.Synth.Noise.Volume = vol;
                _audio.Synth.Noise.Duty = duty;
            }
        }

        // --- Physics ---
        public MoveResult move_and_slide(float x, float y, float w, float h, float vx, float vy) => _physEngine.MoveAndSlide(new Rect(x, y, w, h), vx, vy);

        public void set_tile_solid(int tileX, int tileY, bool solid)
        => _vram.SetTileSolid(tileX, tileY, solid);

        public bool get_tile_solid(int tileX, int tileY)
            => _vram.IsTileSolid(tileX, tileY);
    }
}