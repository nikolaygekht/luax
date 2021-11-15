using System;
using System.Linq;
using System.Text;
using Luax.Interpreter.Execution;

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
                    using var executor = new TestProjectHandler(project);
                    Environment.ExitCode = executor.Run(args);
                }
            }
            catch (Exception e)
            {
                Environment.ExitCode = ExceptionWriter.WriteException(e, s => Console.WriteLine("{0}", s));
            }
        }
    }
}


