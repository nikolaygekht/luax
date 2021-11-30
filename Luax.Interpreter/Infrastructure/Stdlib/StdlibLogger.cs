using System;
using System.IO;
using Luax.Parser.Ast;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

#pragma warning disable S125                // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibLogger
    {
        private static Serilog.Core.Logger mLogger;
        internal static event EventHandler<LogEvent> Logged;

        private static void OnLogged(LogEvent e)
        {
            EventHandler<LogEvent> handler = Logged;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("loggerConfig.json", optional: true);

            mLogger = new LoggerConfiguration()
                        .ReadFrom.Configuration(builder.Build())
                        .WriteTo.Sink(new DelegatingSink(le => OnLogged(le)))
                        .CreateLogger();
        }

        //public static extern debug(message : string) : void;
        [LuaXExternMethod("logger", "debug")]
        public static object Debug(string message)
        {
            mLogger.Debug(message);
            return null;
        }

        //public static extern info(message : string) : void;
        [LuaXExternMethod("logger", "info")]
        public static object Info(string message)
        {
            mLogger.Information(message);
            return null;
        }

        //public static extern warning(message : string) : void;
        [LuaXExternMethod("logger", "warning")]
        public static object Warning(string message)
        {
            mLogger.Warning(message);
            return null;
        }

        //public static extern error(message : string) : void;
        [LuaXExternMethod("logger", "error")]
        public static object Error(string message)
        {
            mLogger.Error(message);
            return null;
        }

        //public static extern fatal(message : string) : void;
        [LuaXExternMethod("logger", "fatal")]
        public static object Fatal(string message)
        {
            mLogger.Fatal(message);
            return null;
        }
    }
    internal class DelegatingSink : ILogEventSink
    {
        private readonly Action<LogEvent> mWriter;

        public DelegatingSink(Action<LogEvent> write)
        {
            mWriter = write ?? throw new ArgumentNullException(nameof(write));
        }

        public void Emit(LogEvent logEvent)
        {
            mWriter(logEvent);
        }
    }
}
