using Raylib_cs;
using System.Numerics;

namespace YAFC.Video
{
    public class VramResourceManager
    {
        private Texture2D _spriteSheet;
        private readonly int _tileSize;
        private Dictionary<(int, int), bool> _tileSolidity = new();

        private readonly Color[] gamePalette = new Color[]
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


        public VramResourceManager(int tileSize = 16)
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

        public void DrawTile(int tileId, int tileX, int tileY)
        {
            float x = tileX * _tileSize;
            float y = tileY * _tileSize;

            DrawSprite(tileId, x, y);
        }

        private static int DistanceSq(Color a, Color b)
        {
            int dr = a.R - b.R;
            int dg = a.G - b.G;
            int db = a.B - b.B;

            return dr * dr + dg * dg + db * db;
        }

        private static Color FindClosestColor(Color src, Color[] palette)
        {
            Color closest = palette[0];
            float minDistance = float.MaxValue;

            for (int i = 0; i < palette.Length; i++)
            {
                int distance = DistanceSq(src, palette[i]);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = palette[i];
                    if (distance == 0) break;
                }
            }

            return closest;
        }

        public static unsafe void ApplyFixedPalette(ref Image image, Color[] palette)
        {
            if (palette == null || palette.Length == 0) return;

            if (image.Format != PixelFormat.UncompressedR8G8B8A8)
                Raylib.ImageFormat(ref image, PixelFormat.UncompressedR8G8B8A8);

            Color* pixels = (Color*)image.Data;
            int pixelCount = image.Width * image.Height;

            var cache = new Dictionary<uint, Color>();

            for (int i = 0; i < pixelCount; i++)
            {
                Color src = pixels[i];
                if (src.A == 0) continue;

                uint key = ((uint)src.R << 16) | ((uint)src.G << 8) | src.B;

                if (!cache.TryGetValue(key, out Color mapped))
                {
                    mapped = FindClosestColor(src, palette);
                    cache[key] = mapped;
                }

                pixels[i] = new Color(mapped.R, mapped.G, mapped.B, src.A);
            }
        }

        public void Unload()
        {
            if (_spriteSheet.Id != 0) Raylib.UnloadTexture(_spriteSheet);
        }

        public void SetTileSolid(int tileX, int tileY, bool isSolid)
        {
            _tileSolidity[(tileX, tileY)] = isSolid;
        }

        public bool IsTileSolid(int tileX, int tileY)
        {
            return _tileSolidity.TryGetValue((tileX, tileY), out var solid) && solid;
        }
    }
}
