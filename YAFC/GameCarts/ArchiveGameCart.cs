using Zio;
using Zio.FileSystems;

namespace YAFC.GameCarts
{
    public sealed class ArchiveGameCart : IGameCart
    {
        public IFileSystem fs { get; }

        public UPath MainScript => "/main.lua";
        public UPath SpriteSheet => "/spritesheet.png";
        public UPath Music => "/menu.xm";
        public UPath Metadata => "/meta.xml";

        public ArchiveGameCart(string filename)
        {
            fs = new ZipArchiveFileSystem(filename);
        }
        
        public void Dispose()
        {
            fs.Dispose();
        }
    }
}
