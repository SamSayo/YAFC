using Raylib_cs;
using System.Numerics;

namespace YAFC
{
    public class VramResourceManager
    {
        private Texture2D _spriteSheet;
        private readonly int _tileSize;

        private Color[] gamePalette = new Color[]
        {
            new Color(0, 0, 0, 255),
            new Color(126, 126, 126, 255),
            new Color(190, 190, 190, 255),
            new Color(255, 255, 255, 255),
            new Color(126, 0, 0, 255),
            new Color(254, 0, 0, 255),
            new Color(4, 126, 0, 255),
            new Color(6, 255, 4, 255),
            new Color(126, 126, 0, 255),
            new Color(255, 255, 4, 255),
            new Color(0, 0, 126, 255),
            new Color(0, 0, 255, 255),
            new Color(126, 0, 126, 255),
            new Color(254, 0, 255, 255),
            new Color(4, 126, 126, 255),
            new Color(6, 255, 255, 255)
        };


        public VramResourceManager(int tileSize = 8)
        {
            _tileSize = tileSize;
        }

        public void LoadSpriteSheet(Image spriteSheet)
        {
            if (_spriteSheet.Id != 0)
            {
                Raylib.UnloadTexture(_spriteSheet);
            }

            ApplyFixedPalette(ref spriteSheet, gamePalette);

            _spriteSheet = Raylib.LoadTextureFromImage(spriteSheet);
        }

        public void DrawSprite(int spriteId, float x, float y, float scale = 1.0f, bool flipX = false, bool flipY = false)
        {
            if (_spriteSheet.Id == 0) return;

            // Finding col and row in spritesheet
            int spritesPerRow = _spriteSheet.Width / _tileSize;
            int row = spriteId / spritesPerRow;
            int col = spriteId % spritesPerRow;

            Rectangle srcRect = new Rectangle(col * _tileSize, row * _tileSize, _tileSize, _tileSize);
            if (flipX) srcRect.Width *= -1;
            if (flipY) srcRect.Height *= -1;

            Rectangle destRect = new Rectangle(x, y, _tileSize * scale, _tileSize * scale);
            Vector2 origin = Vector2.Zero;

            Raylib.DrawTexturePro(_spriteSheet, srcRect, destRect, origin, 0.0f, Color.White);
        }

        private static Color FindClosestColor(Color src, Color[] palette)
        {
            Color closest = palette[0];
            float minDistance = float.MaxValue;

            for (int i = 0; i < palette.Length; i++)
            {
                float dr = src.R - palette[i].R;
                float dg = src.G - palette[i].G;
                float db = src.B - palette[i].B;

                float distance = dr * dr + dg * dg + db * db;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = palette[i];
                }
            }

            return closest;
        }

        public static void ApplyFixedPalette(ref Image image, Color[] palette)
        {
            if (palette == null || palette.Length == 0) return;

            unsafe
            {
                Color* pixels = Raylib.LoadImageColors(image);
                int pixelCount = image.Width * image.Height;

                for (int i = 0; i < pixelCount; i++)
                {
                    if (pixels[i].A > 0)
                    {
                        pixels[i] = FindClosestColor(pixels[i], palette);
                    }
                }

                Raylib.UnloadImageColors(pixels);
            }
        }

        public void Unload()
        {
            if (_spriteSheet.Id != 0) Raylib.UnloadTexture(_spriteSheet);
        }
    }
}