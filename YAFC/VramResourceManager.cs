using Raylib_cs;
using System.Numerics;

namespace YAFC
{
    public class VramResourceManager
    {
        private Texture2D _spriteSheet;
        private readonly int _tileSize;

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

        public void Unload()
        {
            if (_spriteSheet.Id != 0) Raylib.UnloadTexture(_spriteSheet);
        }
    }
}