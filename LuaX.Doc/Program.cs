using System;
using System.IO;
using System.Linq;
using System.Text;
using Luax.Interpreter.Execution;
using Luax.Parser;
using Luax.Parser.Ast;

namespace LuaX.Doc
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: LuaX.Doc luaxFile dsFile group");
                Environment.ExitCode = -1;
                return;
            }

            try
            {
                var parser = new LuaXAstGenerator();
                var body = parser.Compile(args[0], File.ReadAllText(args[0]));
                using var output = new DocumentationWriter(
                    body.Classes,
                    args[2],
                    new StreamWriter(
                        new FileStream(args[1], FileMode.Create, FileAccess.Write, FileShare.None), Encoding.UTF8));
                output.Write();
            }
            catch (Exception e)
            {
                Environment.ExitCode = ExceptionWriter.WriteException(e, s => Console.WriteLine("{0}", s));
            }
        }
    }
}


