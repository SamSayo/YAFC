using Raylib_cs;
using System.Numerics;
using YAFC.Physics;

namespace YAFC.Video
{
    public class VramResourceManager
    {
        private Texture2D _spriteSheet;
        private readonly int _tileSize;
        private readonly int _screenW;
        private readonly int _screenH;
        private readonly SolidMap _solids = new();

        public TileMap Background { get; } = new TileMap();

        public int ScreenWidth => _screenW;
        public int ScreenHeight => _screenH;

        private static readonly Color[] gamePalette = new[]
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
            new Color(6, 255, 255, 255),

            new Color(36, 36, 48, 255),
            new Color(68, 68, 82, 255),
            new Color(85, 85, 85, 255),
            new Color(154, 160, 174, 255),
            new Color(224, 224, 232, 255),
            new Color(68, 52, 60, 255),
            new Color(126, 112, 104, 255),
            new Color(238, 228, 210, 255),

            new Color(60, 28, 16, 255),
            new Color(96, 48, 24, 255),
            new Color(138, 72, 34, 255),
            new Color(176, 102, 48, 255),
            new Color(212, 146, 82, 255),
            new Color(102, 76, 44, 255),
            new Color(152, 124, 78, 255),
            new Color(218, 194, 138, 255),

            new Color(255, 204, 0, 255),
            new Color(255, 255, 154, 255),
            new Color(16, 68, 24, 255),
            new Color(34, 154, 48, 255),
            new Color(136, 212, 34, 255),
            new Color(170, 255, 154, 255),
            new Color(0, 102, 78, 255),
            new Color(48, 190, 136, 255),

            new Color(16, 24, 78, 255),
            new Color(24, 68, 164, 255),
            new Color(56, 126, 238, 255),
            new Color(85, 170, 255, 255),
            new Color(170, 224, 255, 255),
            new Color(0, 170, 190, 255),
            new Color(118, 238, 238, 255),
            new Color(68, 96, 144, 255),

            new Color(56, 16, 82, 255),
            new Color(102, 42, 156, 255),
            new Color(156, 85, 238, 255),
            new Color(204, 164, 255, 255),
            new Color(170, 34, 126, 255),
            new Color(255, 85, 170, 255),
            new Color(255, 170, 224, 255),
            new Color(136, 82, 118, 255)
        };

        public VramResourceManager(int tileSize = 16, int screenW = 256, int screenH = 192)
        {
            _tileSize = tileSize;
            _screenW = screenW;
            _screenH = screenH;
        }

        public VramResourceManager WithTileSize(int newTileSize) => new(newTileSize, _screenW, _screenH);

        public void LoadSpriteSheet(Image spriteSheet)
        {
            Reset();

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

            x = MathF.Floor(x);
            y = MathF.Floor(y);

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

        public void DrawBackground(int y0, int y1)
        {
            if (_spriteSheet.Id == 0) return;

            y0 = Math.Clamp(y0, 0, _screenH);
            y1 = Math.Clamp(y1, 0, _screenH);
            if (y1 <= y0) return;

            int ts = _tileSize;
            int spriteCount = (_spriteSheet.Width / ts) * (_spriteSheet.Height / ts);

            int sx = Wrap(Background.ScrollX, TileMap.Cols * ts);
            int sy = Wrap(Background.ScrollY, TileMap.Rows * ts);

            int firstCol = sx / ts;
            int lastCol = (sx + _screenW - 1) / ts;
            int firstRow = (sy + y0) / ts;
            int lastRow = (sy + y1 - 1) / ts;

            for (int row = firstRow; row <= lastRow; row++)
            {
                int screenY = row * ts - sy;
                int mapRow = row % TileMap.Rows;

                for (int col = firstCol; col <= lastCol; col++)
                {
                    if (!Background.TryGet(col % TileMap.Cols, mapRow, out int id, out int flags))
                        continue;

                    if (id >= spriteCount)
                        continue;

                    DrawSprite(id, col * ts - sx, screenY, 1.0f, (flags & TileMap.FlipX) != 0, (flags & TileMap.FlipY) != 0);
                }
            }
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

        private static int Wrap(int value, int size)
        {
            int m = value % size;
            return m < 0 ? m + size : m;
        }

        public void Reset()
        {
            Background.Reset();
            _solids.Clear();
        }

        public void Unload()
        {
            if (_spriteSheet.Id != 0) Raylib.UnloadTexture(_spriteSheet);

            _spriteSheet = default;

            Reset();
        }

        public void SetTileSolid(int tileX, int tileY, bool isSolid)
        {
            _solids.Set(tileX, tileY, isSolid);
        }

        public bool IsTileSolid(int tileX, int tileY)
        {
            return _solids.Get(tileX, tileY);
        }

        public void FillSolid(int tileX, int tileY, int width, int height, bool isSolid)
        {
            _solids.Fill(tileX, tileY, width, height, isSolid);
        }

        public void ClearSolids()
        {
            _solids.Clear();
        }
    }
}
