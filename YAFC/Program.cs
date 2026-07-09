using MoonSharp.Interpreter;
using Raylib_cs;
using System.IO;
using System.Numerics;

namespace YAFC
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleRuntime console = new();

            if (args.Length == 0)
                console.Menu();
            else
                console.Run(args[0], args[1]);
        }
    }
}
