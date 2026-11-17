namespace YAFC.Physics
{
    public sealed class SolidMap
    {
        private const int ChunkShift = 4;
        private const int ChunkSize = 1 << ChunkShift;
        private const int ChunkMask = ChunkSize - 1;

        private const int MaxChunks = 4096;

        private readonly Dictionary<(int, int), bool[]> _chunks = new();

        private (int, int) _lastKey;
        private bool[]? _lastChunk;

        public bool Get(int tx, int ty)
        {
            bool[]? chunk = FindChunk(tx >> ChunkShift, ty >> ChunkShift, create: false);
            return chunk != null && chunk[Slot(tx, ty)];
        }

        public void Set(int tx, int ty, bool solid)
        {
            bool[]? chunk = FindChunk(tx >> ChunkShift, ty >> ChunkShift, create: solid);
            if (chunk != null)
                chunk[Slot(tx, ty)] = solid;
        }

        public void Fill(int tx, int ty, int width, int height, bool solid)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    Set(tx + x, ty + y, solid);
        }

        public void Clear()
        {
            _chunks.Clear();
            _lastChunk = null;
        }


        private static int Slot(int tx, int ty)
            => ((ty & ChunkMask) << ChunkShift) | (tx & ChunkMask);

        private bool[]? FindChunk(int cx, int cy, bool create)
        {
            if (_lastChunk != null && _lastKey == (cx, cy))
                return _lastChunk;

            if (!_chunks.TryGetValue((cx, cy), out bool[]? chunk))
            {
                if (!create)
                    return null;

                if (_chunks.Count >= MaxChunks)
                    throw new InvalidOperationException("Collision map is full");

                chunk = new bool[ChunkSize * ChunkSize];
                _chunks[(cx, cy)] = chunk;
            }

            _lastKey = (cx, cy);
            _lastChunk = chunk;
            return chunk;
        }
    }
}
