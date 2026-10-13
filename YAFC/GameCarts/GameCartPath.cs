using Zio;

namespace YAFC.GameCarts
{
    public static class GameCartPath
    {
        public const int MaxLength = 255;
        public const int MaxDepth = 16;

        public static UPath Parse(string? path)
        {
            if (string.IsNullOrEmpty(path) || path.Length > MaxLength)
            {
                throw new GameCartException("bad path");
            }

            if (path.IndexOfAny(new[] { '\\', '\0', ':' }) >= 0)
            {
                throw new GameCartException("bad path: forbidden character");
            }

            string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > MaxDepth)
            {
                throw new GameCartException("bad path: too deep");
            }

            foreach (string part in parts)
            {
                if (part == "." || part == "..")
                {
                    throw new GameCartException("bad path: '.' and '..' are not allowed");
                }
            }

            return new UPath("/" + string.Join('/', parts));
        }
    }
}