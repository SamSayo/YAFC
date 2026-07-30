namespace YAFC
{
    public class PhysEngine
    {
        public int TileSize = 16;

        private Func<int, int, bool> _isTileSolid;

        public PhysEngine(Func<int, int, bool> isTileSolidFunc)
        {
            _isTileSolid = isTileSolidFunc;
        }

        public MoveResult MoveAndSlide(Rect box, float vx, float vy)
        {
            MoveResult result = new MoveResult { FinalX = box.X, FinalY = box.Y };

            if (vx != 0)
            {
                result.FinalX += vx;
                Rect boxX = new Rect(result.FinalX, result.FinalY, box.Width, box.Height);

                if (CollideWithMap(boxX, true, vx, out float correctedX))
                {
                    result.FinalX = correctedX;
                    if (vx > 0) result.HitWallRight = true;
                    if (vx < 0) result.HitWallLeft = true;
                }
            }

            if (vy != 0)
            {
                result.FinalY += vy;
                Rect boxY = new Rect(result.FinalX, result.FinalY, box.Width, box.Height);

                if (CollideWithMap(boxY, false, vy, out float correctedY))
                {
                    result.FinalY = correctedY;
                    if (vy > 0) result.IsGrounded = true;
                    if (vy < 0) result.HitCeiling = true;
                }
            }

            return result;
        }

        private bool CollideWithMap(Rect box, bool checkingX, float velocity, out float correctedPosition)
        {
            correctedPosition = checkingX ? box.X : box.Y;

            int minTileX = (int)Math.Floor(box.X / TileSize);
            int maxTileX = (int)Math.Floor((box.X + box.Width - 0.001f) / TileSize);
            int minTileY = (int)Math.Floor(box.Y / TileSize);
            int maxTileY = (int)Math.Floor((box.Y + box.Height - 0.001f) / TileSize);

            for (int tx = minTileX; tx <= maxTileX; tx++)
            {
                for (int ty = minTileY; ty <= maxTileY; ty++)
                {
                    if (_isTileSolid(tx, ty))
                    {
                        if (checkingX)
                        {                            
                            if (velocity > 0)
                                correctedPosition = tx * TileSize - box.Width; 
                            else if (velocity < 0)
                                correctedPosition = (tx + 1) * TileSize;
                        }
                        else
                        {
                            if (velocity > 0)
                                correctedPosition = ty * TileSize - box.Height;
                            else if (velocity < 0)
                                correctedPosition = (ty + 1) * TileSize;
                        }
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
