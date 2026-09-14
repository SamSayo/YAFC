using Raylib_cs;
using System.Text.Json;

namespace YAFC.GameCarts
{
    public class CartridgeManager
    {
        public LoadedGameCart OpenCartridge(string cartPath)
        {
            using IGameCart cart = GameCartFactory.Create(cartPath);

            LoadedGameCart gameCart = new();

            if (cart.fs.FileExists(cart.MainScript))
            {
                using (Stream stream = cart.fs.OpenFile(cart.MainScript, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream))
                {
                    gameCart.luaCode = reader.ReadToEnd();
                }
            }

            if (cart.fs.FileExists(cart.SpriteSheet))
            {
                using (Stream stream = cart.fs.OpenFile(cart.SpriteSheet, FileMode.Open, FileAccess.Read))
                using (MemoryStream ms = new())
                {
                    stream.CopyTo(ms);
                    byte[] rawImageData = ms.ToArray();

                    gameCart.spriteSheet = Raylib.LoadImageFromMemory(".png", rawImageData);
                }
            }
            else
            {
                int candidatesNum = 0;
                byte[] rawImageData;
                foreach (var spriteCandidate in cart.fs.EnumeratePaths("/", "*.png", SearchOption.TopDirectoryOnly, Zio.SearchTarget.File))
                {
                    candidatesNum++;
                    using (Stream stream = cart.fs.OpenFile(spriteCandidate, FileMode.Open, FileAccess.Read))
                    using (MemoryStream ms = new())
                    {
                        stream.CopyTo(ms);
                        rawImageData = ms.ToArray();

                        gameCart.spriteSheet = Raylib.LoadImageFromMemory(".png", rawImageData);
                    }
                }
            }

            if (cart.fs.FileExists(cart.Metadata))
            {
                using (Stream stream = cart.fs.OpenFile(cart.Metadata, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new(stream))
                {
                    gameCart.meta = JsonSerializer.Deserialize<Metadata>(reader.ReadToEnd());
                }
            }
            else
            {
                string fallbackMeta = "{" +
                    "\"Id\":\"no.game.id\"," +
                    "\"Name\":\"YAFC\"," +
                    "\"Description\":\"Please, make meta.json\"," +
                    "\"Version\": \"0.0.0\"" +
                    "}";

                gameCart.meta = JsonSerializer.Deserialize<Metadata>(fallbackMeta);
            }

            return gameCart;

            /*
            using (ZipArchive cart = ZipFile.OpenRead(cartPath))
            {
                foreach (ZipArchiveEntry file in cart.Entries)
                {
                    Console.WriteLine($"Cart file: {file.FullName}, Size: {file.Length / 1000} KB");

                    if (file.Name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream stream = file.Open())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            luaCode = reader.ReadToEnd();
                        }
                    }
                    else if (file.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        using (Stream stream = file.Open())
                        using (MemoryStream ms = new())
                        {
                            stream.CopyTo(ms);
                            byte[] rawImageData = ms.ToArray();

                            spriteSheet = Raylib.LoadImageFromMemory(".png", rawImageData);
                        }
                    }
                }
            }

            return (luaCode, spriteSheet);
            */
        }
    }
}
