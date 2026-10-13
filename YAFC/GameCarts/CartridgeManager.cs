using Raylib_cs;

namespace YAFC.GameCarts
{
    public class CartridgeManager
    {
        public LoadedGameCart OpenCartridge(string cartPath)
        {
            using IGameCart cart = GameCartFactory.Create(cartPath);
            CartFileSystem files = CartFileSystem.Snapshot(cart.fs);

            LoadedGameCart gameCart = new() { files = files};

            // Copying main.lua to memory
            string mainScriptPath = cart.MainScript.FullName;
            if (!files.FileExists(mainScriptPath))
            {
                string seen = string.Join(", ", files.List("/"));
                string raw = cart.fs.FileExists(cart.MainScript) ? "yes" : "no";
                throw new GameCartException(
                    $"GameCart has no main.lua (snapshot root: [{seen}]; source sees it: {raw})");
            }
            gameCart.luaCode = files.ReadText(mainScriptPath);

            // Copying spritesheet.png to memory
            string spritePath = cart.SpriteSheet.FullName;
            if (!files.FileExists(spritePath))
            {
                throw new GameCartException("GameCart has no spritesheet.png");
            }

            // Is there a sprite inside this spritesheet
            Image image = Raylib.LoadImageFromMemory(".png", files.ReadBytes(spritePath));
            if (image.Width < 16 || image.Height < 16)
            {
                throw new GameCartException("spritesheet.png is not a valid PNG");
            }
            gameCart.spriteSheet = image;

            // Reading manifest from gamecart
            string metaPath = cart.Metadata.FullName;
            gameCart.meta = files.FileExists(metaPath)
                ? Metadata.Parse(files.ReadText(metaPath))
                : Metadata.Fallback;

            if (gameCart.meta.CanSave)
            {
                gameCart.save = SaveRam.Open(gameCart.meta.Id, gameCart.meta.SaveSize);
            }

            return gameCart;
        }
    }
}
