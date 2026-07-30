namespace YAFC
{
    public struct Rect
    {
        public float X, Y, Width, Height;

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Overlaps(Rect other)
        {
            return X < other.X + other.Width &&
                    X + Width > other.X &&
                    Y < other.Y + other.Height &&
                    Y + Height > other.Y;
        }
    }

    public struct MoveResult
    {
        public float FinalX;
        public float FinalY;
        public bool HitWallLeft;
        public bool HitWallRight;
        public bool IsGrounded;
        public bool HitCeiling;
    }
}
