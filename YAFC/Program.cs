using YAFC.GameCarts;

namespace YAFC
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ConsoleRuntime yafc = new();

            if (args.Length == 0)
                yafc.Start();
            else
                yafc.Start(args[0]);
        }
    }
}
