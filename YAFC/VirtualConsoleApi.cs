using MoonSharp.Interpreter;
using Raylib_cs;

[MoonSharpUserData]
public class VirtualConsoleApi
{
    private readonly VramResourceManager _vram;
    private readonly InputManager _input;

    public VirtualConsoleApi(VramResourceManager vram, InputManager input)
    {
        _vram = vram;
        _input = input;
    }

    // --- Графика ---
    public void spr(int id, float x, float y, float scale = 1.0f, bool flipX = false, bool flipY = false)
        => _vram.DrawSprite(id, x, y, scale, flipX, flipY);

    public void cls(byte r, byte g, byte b)
        => Raylib.ClearBackground(new Color(r, g, b, (byte)255));

    public void print(string text, float x, float y, int fontSize, byte r, byte g, byte b)
        => Raylib.DrawText(text, (int)x, (int)y, fontSize, new Color(r, g, b, (byte)255));

    // --- Ввод ---
    public bool btn(int id) => _input.IsBtnDown(id);
    public bool btnp(int id) => _input.IsBtnPressed(id);
}