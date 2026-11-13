using YAFC.GameCarts;

namespace YAFC
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ConsoleRuntime yafc = new();
            yafc.Start();
        }
    }
}
