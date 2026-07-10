using Raylib_cs;
using System.IO.Compression;

namespace YAFC
{
    public class CartridgeManager
    {
        private string luaCode = String.Empty;
        private Image spriteSheet;

        public (string luaCode, Image spriteSheet) OpenCartridge(string cartPath)
        {
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
        }
    }
}
