using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Luax.Interpreter.Execution;
using Luax.Interpreter.Execution.Coverage;

namespace LuaX
{
    public static class Program
    {
        private static void Help()
        {
            Console.WriteLine("Usage: LuaX options project_or_source arg1, arg2, arg3");
            Console.WriteLine(" Where options are:");
            Console.WriteLine("  -c or --coverage   - generate coverage report");
            Environment.ExitCode = -1;
        }

        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (args.Length < 1)
            {
                Help();
                return;
            }

            string name = null;
            var appArgs = new List<string>();
            bool enableCoverage = false;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.IsNullOrEmpty(name))
                {
                    if (arg.StartsWith("-"))
                    {
                        switch (arg)
                        {
                            case "-c":
                            case "--coverage":
                                enableCoverage = true;
                                break;
                            default:
                                Console.WriteLine($"Unknown argument {arg}");
                                Help();
                                return;
                        }
                    }
                    else
                        name = arg;
                }
                else
                    appArgs.Add(arg);
            }

            if (string.IsNullOrEmpty(name))
            {
                Help();
                return;
            }

            Execute(name, appArgs.ToArray(), enableCoverage);
        }

        private static void Execute(string name, string[] args, bool enableCoverage)
        {
            try
            {
                CoverageReport coverageReport;
                var project = LuaXProjectReader.Read(name);
                if (project.ProjectType == LuaXProjectType.ConsoleApp)
                {
                    var executor = new LuaXConsoleExecutor(project) { EnableCoverage = enableCoverage };
                    Environment.ExitCode = executor.Run(args);
                    coverageReport = executor.CoverageReport;
                }
                else
                {
                    using var executor = new TestProjectHandler(project) { EnableCoverage = enableCoverage };
                    Environment.ExitCode = executor.Run(args);
                    coverageReport = executor.CoverageReport;
                }

                if (coverageReport != null)
                    SaveReport(coverageReport);
            }
            catch (Exception e)
            {
                Environment.ExitCode = ExceptionWriter.WriteException(e, s => Console.WriteLine("{0}", s));
            }
        }

        public static void SaveReport(CoverageReport report)
        {
            using var fs = new FileStream("coverage.xml", FileMode.Create, FileAccess.Write);
            report.SaveTo(fs, Encoding.UTF8);
        }
    }
}


