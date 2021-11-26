using System;
using Luax.Parser.Ast;
using Serilog;

#pragma warning disable S125                // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibBaseLogger
    {
        private static LuaXClassInstance mLoggerClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("baseLogger", out mLoggerClass);
            mLoggerClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__logger", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern baseLogger() : void;
        [LuaXExternMethod("baseLogger", "baseLogger")]
        public static object Constructor(LuaXObjectInstance _)
        {
            var logger = new LuaXObjectInstance(mLoggerClass);
            logger.Properties["__logger"].Value = null;
            return logger;
        }

        //public extern bindConfigurator(configurator : baseLoggerConfigurator) : void;
        [LuaXExternMethod("baseLogger", "bindConfigurator")]
        public static object BindConfigurator(LuaXObjectInstance @this, LuaXObjectInstance configuratorInstance)
        {
            //if (@this.Properties["__logger"].Value is not Serilog.Core.Logger logger)
            //    throw new ArgumentException("The configurator isn't properly initialized", nameof(@this));
            if (configuratorInstance.Properties["__configurator"].Value is not LoggerConfiguration configurator)
                throw new ArgumentException("The configurator isn't properly initialized", nameof(configuratorInstance));

            @this.Properties["__logger"].Value = configurator.CreateLogger();
            return null;
        }

        //public extern log(level : int, message : string) : void;
        [LuaXExternMethod("baseLogger", "log")]
        public static object Log(LuaXObjectInstance @this, int level, string message)
        {
            if (@this.Properties["__logger"].Value is not Serilog.Core.Logger logger)
                throw new ArgumentException("The logger isn't properly initialized", nameof(@this));

            switch (level)
            {
                case 0:
                    logger.Verbose(message);
                    break;
                case 1:
                    logger.Debug(message);
                    break;
                case 2:
                    logger.Information(message);
                    break;
                case 3:
                    logger.Warning(message);
                    break;
                case 4:
                    logger.Error(message);
                    break;
                case 5:
                    logger.Fatal(message);
                    break;
            }
            return null;
        }
    }
}
