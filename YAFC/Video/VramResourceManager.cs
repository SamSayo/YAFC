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


        public VramResourceManager(int tileSize = 16, int screenW = 256, int screenH = 192)
        {
            _tileSize = tileSize;
            _screenW = screenW;
            _screenH = screenH;
        }

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

            Raylib.BeginScissorMode(0, y0, _screenW, y1 - y0);
            try
            {
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
            finally
            {
                Raylib.EndScissorMode();
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
