using System;
using System.Linq;
using Luax.Interpreter.Execution;
using Luax.Interpreter.Infrastructure.Stdlib;
using Luax.Parser.Ast;

namespace LuaX
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: LuaX project_or_source arg1, arg2, arg3");
                Environment.ExitCode = -1;
                return;
            }

            try
            {
                var project = LuaXProjectReader.Read(args[0]);
                if (project.ProjectType == LuaXProjectType.ConsoleApp)
                {
                    var executor = new LuaXConsoleExecutor(project);
                    Environment.ExitCode = executor.Run(args);
                }
                else
                {
                    var executor = new LuaXTestExecutor(project);
                    executor.OnTest += (_, args) => Console.WriteLine($"{args.Class},{args.Method}({args.Data}) - {args.Status} {args.Message}");
                    Environment.ExitCode = executor.Run(args);
                }
            }
            catch (LuaXAssertionException assertion)
            {
                Console.WriteLine($"Unexpected assertion {assertion.Message}");
                Environment.ExitCode = -2;
            }
            catch (LuaXAstGeneratorException error)
            {
                var errors = error.Errors;
                foreach (var err in errors)
                    Console.WriteLine($"{error.Source}({err.Line},{err.Column}) - {err.Message}");
                Environment.ExitCode = -3;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Unexpected exception:");
                Console.WriteLine(exception.ToString());
                Environment.ExitCode = -4;
            }
        }
    }
}
