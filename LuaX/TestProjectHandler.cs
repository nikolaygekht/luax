using System;
using System.IO;
using Luax.Interpreter.Execution;

namespace LuaX
{
    internal sealed class TestProjectHandler : IDisposable
    {
        private readonly StreamWriter logWriter = null;
        private readonly StreamWriter errorWriter = null;
        private readonly ConsoleColor currentColor;
        private readonly LuaXTestExecutor executor;

        public TestProjectHandler(LuaXProject project)
        {
            executor = new LuaXTestExecutor(project);
            executor.OnTest += OnTest;

            if (project.Options["log"] != null)
                logWriter = new StreamWriter(project.Options["log"]);
            if (project.Options["errorlog"] != null)
                errorWriter = new StreamWriter(project.Options["errorlog"]);

            currentColor = Console.ForegroundColor;
        }

        public int Run(string[] args)
        {
            executor.Run(args);
            OnEnd();
            return executor.TotalTests != executor.SuccessfullTests ? -10 : 0;
        }

        private void OnTest(object _, LuaXTestStatusEventArgs args)
        {

            var message1 = $"{args.Class}::{args.Method}({args.Data.Replace("\n", "\\n")}) - ";
            var message2 = $"{args.Status} [{args.Message}]";
            if (Console.WindowWidth > 0)
                HandleOnConsole(args, message1, message2);
            else
                HandleOnStream(message1, message2);


            logWriter?.WriteLine("{0}", message1 + message2);

            if (args.Status == LuaXTestStatus.Exception && args.Exception != null && errorWriter != null)
                ExceptionWriter.WriteException(args.Exception, s => errorWriter.WriteLine("{0}", s));

        }

        private void HandleOnStream(string message1, string message2)
        {
            Console.WriteLine("{0} {1}", message1, message2);
        }

        private void HandleOnConsole(LuaXTestStatusEventArgs args, string message1, string message2)
        { 
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
            Console.Write(new string(' ', Console.WindowWidth - 2));
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
           
            Console.Write("{0}", message1);
            Console.ForegroundColor = args.Status == LuaXTestStatus.OK ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write("{0}", message2);
            Console.ForegroundColor = currentColor;

            if (args.Status != LuaXTestStatus.OK)
                Console.WriteLine();
        }

        private void OnEnd()
        {
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

        public void Dispose()
        {
            logWriter?.Dispose();
            errorWriter?.Dispose();
        }
    }
}


