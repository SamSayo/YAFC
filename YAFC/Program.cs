namespace YAFC
{
    class Program
    {
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
