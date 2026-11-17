namespace YAFC.Video
{
    public sealed class TileMap
    {
        public const int Cols = 32;
        public const int Rows = 16;

        public const int FlipX = 1;
        public const int FlipY = 2;

        private const ushort EmptyCell = ushort.MaxValue;

        private readonly ushort[] _ids = new ushort[Cols * Rows];
        private readonly byte[] _flags = new byte[Cols * Rows];

        public int ScrollX { get; set; }
        public int ScrollY { get; set; }

        public TileMap()
        {
            Reset();
        }

        public void Reset()
        {
            ClearCells();
            ScrollX = 0;
            ScrollY = 0;
        }

        public void ClearCells()
        {
            Array.Fill(_ids, EmptyCell);
            Array.Clear(_flags);
        }

        public void Set(int tx, int ty, int id, int flags)
        {
            int i = Index(tx, ty);

            if (id < 0 || id >= EmptyCell)
            {
                _ids[i] = EmptyCell;
                _flags[i] = 0;
                return;
            }

            _ids[i] = (ushort)id;
            _flags[i] = (byte)(flags & (FlipX | FlipY));
        }

        public bool TryGet(int tx, int ty, out int id, out int flags)
        {
            int i = Index(tx, ty);

            if (_ids[i] == EmptyCell)
            {
                id = -1;
                flags = 0;
                return false;
            }

            id = _ids[i];
            flags = _flags[i];
            return true;
        }

        private static int Index(int tx, int ty)
            => ((ty & (Rows - 1)) * Cols) + (tx & (Cols - 1));
    }
}
