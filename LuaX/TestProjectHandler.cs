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
                ExceptionWriter.WriteException(args.Exception, s => errorWriter.WriteLine("{0}", s));
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


