using System;
using System.IO;
using System.Linq;
using System.Text;
using Luax.Interpreter;
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
                    StreamWriter logWriter = null;
                    StreamWriter errorWriter = null;

                    try
                    {
                        if (project.Options["log"] != null)
                           logWriter = new StreamWriter(project.Options["log"]);
                        if (project.Options["errorlog"] != null)
                            errorWriter = new StreamWriter(project.Options["errorlog"]);

                        var currentColor = Console.ForegroundColor;

                        var executor = new LuaXTestExecutor(project);
                        executor.OnTest += (_, args) => {
                            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
                            Console.Write(new string(' ', Console.WindowWidth - 2));
                            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);

                            var message1 = $"{args.Class}::{args.Method}({args.Data}) - ";
                            Console.Write("{0}", message1);

                            var message2 = $"{args.Status} [{args.Message}]";
                            Console.ForegroundColor = args.Status == LuaXTestStatus.OK ? ConsoleColor.Green : ConsoleColor.Red;
                            Console.Write("{0}", message2);
                            Console.ForegroundColor = currentColor;

                            if (args.Status != LuaXTestStatus.OK)
                                Console.WriteLine();

                            logWriter?.WriteLine("{0}", message1 + message2);

                            if (args.Status == LuaXTestStatus.Exception && args.Exception != null && errorWriter != null)
                                WriteException(args.Exception, s => errorWriter.WriteLine("{0}", s));
                        };
                        executor.Run(args);
                        Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
                        Console.Write(new string(' ', Console.WindowWidth - 2));
                        Console.WriteLine();
                        Console.WriteLine("Total Tests: {0}", executor.TotalTests);
                        Console.ForegroundColor = executor.SuccessfullTests == executor.TotalTests ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.WriteLine("Successful Tests: {0}", executor.SuccessfullTests);

                        if (executor.TotalTests != executor.SuccessfullTests)
                            Console.WriteLine("Failed!");
                        else
                            Console.WriteLine("OK!");

                        Console.ForegroundColor = currentColor;
                    }
                    finally
                    {
                        logWriter?.Dispose();
                        errorWriter?.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Environment.ExitCode = WriteException(e, s => Console.WriteLine("{0}", s));
            }
        }

        private static int WriteException(Exception e, Action<string> writerAction)
        {
            if (e is LuaXAssertionException assertion)
            {
                writerAction($"Unexpected assertion {assertion.Message}");
                return -2;
            }
            else if (e is LuaXAstGeneratorException error)
            {
                var errors = error.Errors;
                foreach (var err in errors)
                    writerAction($"{error.SourceName}({err.Line},{err.Column}) - {err.Message}");
                return -3;
            }
            else if (e is LuaXExecutionException executionError)
            {
                writerAction($"{executionError.Locations[0].Source}({executionError.Locations[0].Line},{executionError.Locations[0].Column}) - {executionError.Message}");
                for (int i = 1; i < executionError.Locations.Count; i++)
                    writerAction($"   called from {executionError.Locations[i].Source}({executionError.Locations[i].Line},{executionError.Locations[i].Column})");
                return -4;
            }
            writerAction("Unexpected exception:");
            writerAction(e.ToString());
            return -5;
        }
    }
}
