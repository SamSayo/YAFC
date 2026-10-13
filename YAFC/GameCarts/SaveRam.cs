namespace YAFC.GameCarts
{
    public sealed class SaveRam
    {
        public const int MaxSize = 64 * 1024;

        public static string DefaultDirectory => Path.Combine(AppContext.BaseDirectory, "saves");


        private readonly byte[] _data;
        private readonly string _path;
        private bool _dirty;

        public int Size => _data.Length;
        public string FilePath => _path;

        private SaveRam(string path, byte[] data)
        {
            _path = path;
            _data = data;
        }

        public static SaveRam Open(string id, int size, string? directory = null)
        {
            if (!Metadata.IsValidId(id))
            {
                throw new ArgumentException("Invalid GameCart id", nameof(id));
            }

            if (size <= 0 || size > MaxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            string path = Path.Combine(directory ?? DefaultDirectory, id + ".sav");
            var data = new byte[size];

            if (File.Exists(path))
            {
                try
                {
                    using FileStream stream = File.OpenRead(path);
                    int n = (int)Math.Min(stream.Length, size);
                    stream.ReadExactly(data, 0, n);
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                {
                    throw new GameCartException("Save file is unreadable: " + e.Message);
                }
            }

            return new SaveRam(path, data);
        }

        public bool TryRead(int offset, int length, out byte[] result)
        {
            result = Array.Empty<byte>();
            if (!InRange(offset, length)) return false;

            result = _data.AsSpan(offset, length).ToArray();

            return true;
        }

        public bool TryWrite(int offset, ReadOnlySpan<byte> bytes)
        {
            if (!InRange(offset, bytes.Length)) return false;

            Span<byte> target = _data.AsSpan(offset, bytes.Length);
            if (!target.SequenceEqual(bytes))
            {
                bytes.CopyTo(target);
                _dirty = true;
            }
            return true;
        }

        private bool InRange(int offset, int length) => offset >= 0 && length >= 0 && (long)offset + length <= _data.Length;

        public void FlushIfDirty()
        {
            if (!_dirty) return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

                string tmp = _path + ".tmp";
                File.WriteAllBytes(tmp, _data);
                File.Move(tmp, _path, overwrite: true);

                _dirty = false;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                Console.WriteLine("Save flush failed: " + e.Message);
            }
        }
    }
}
