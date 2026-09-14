using YAFC.GameCarts;

namespace YAFC
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ConsoleRuntime console = new();
            CartridgeManager cartridge = new();

            if (args.Length == 0)
                console.Menu();
            else
                console.Run(cartridge.OpenCartridge(args[0]));
        }
    }
}
