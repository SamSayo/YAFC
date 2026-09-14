using Zio;
using Zio.FileSystems;

namespace YAFC.GameCarts
{
    public sealed class DirGameCart : IGameCart
    {
        public IFileSystem fs { get; }

        public UPath MainScript => "/main.lua";
        public UPath SpriteSheet => "/spritesheet.png";
        public UPath Music => "/menu.xm";
        public UPath Metadata => "/meta.xml";

        public DirGameCart(string directory)
        {
            var physical = new PhysicalFileSystem();

            fs = new SubFileSystem(physical, physical.ConvertPathFromInternal(directory));
        }

        public void Dispose()
        {
            fs.Dispose();
        }
    }
}
