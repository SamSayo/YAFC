using System.Text;
using Zio;
using Zio.FileSystems;

namespace YAFC.GameCarts
{
    public sealed class CartFileSystem
    {
        public const long MaxTotalBytes = 2_621_440;
        public const int MaxFiles = 512;

        private readonly IFileSystem _fs;

        private CartFileSystem(IFileSystem fs) => _fs = fs;

        public static CartFileSystem Snapshot(IFileSystem source)
        {
            var memory = new MemoryFileSystem();
            long total = 0;
            int files = 0;
            var buffer = new byte[16 * 1024];

            foreach (UPath path in source.EnumeratePaths("/", "*", SearchOption.AllDirectories, SearchTarget.File))
            {
                if (IsHidden(path)) continue;

                if (++files > MaxFiles)
                {
                    throw new GameCartException("GameCart has too many files!");
                }

                using Stream stream = source.OpenFile(path, FileMode.Open, FileAccess.Read);
                using var ms = new MemoryStream();

                int n;
                while ((n = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    total += n;

                    if (total > MaxTotalBytes)
                    {
                        throw new GameCartException("GameCart is too chonky!");
                    }

                    ms.Write(buffer, 0, n);
                }

                UPath dir = path.GetDirectory();
                if (!memory.DirectoryExists(dir))
                {
                    memory.WriteAllBytes(path, ms.ToArray());
                }
            }

            return new CartFileSystem(new ReadOnlyFileSystem(memory));
        }

        private static bool IsHidden(UPath path)
        {
            foreach (string segment in path.FullName.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (segment.StartsWith('.')) return true;
            }
            return false;
        }

        public bool FileExists(string path)
        {
            try
            {
                return _fs.FileExists(GameCartPath.Parse(path));
            }
            catch (GameCartException)
            {
                return false;
            }
        }

        public byte[] ReadBytes(string path) => _fs.ReadAllBytes(GameCartPath.Parse(path));


        public string ReadText(string path)
        {
            byte[] b = ReadBytes(path);
            int skip = b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF ? 3 : 0;
            return Encoding.UTF8.GetString(b, skip, b.Length - skip);
        }

        public List<string> List(string dir)
        {
            UPath d = GameCartPath.Parse(dir);
            if (!_fs.DirectoryExists(d))
            {
                throw new DirectoryNotFoundException();
            }

            var names = new List<string>();
            foreach (UPath p in _fs.EnumeratePaths(d, "*", SearchOption.TopDirectoryOnly, SearchTarget.Both))
            {
                names.Add(p.GetName() + (_fs.DirectoryExists(p) ? "/" : ""));
            }

            names.Sort(StringComparer.Ordinal);
            return names;
        }
    }
}