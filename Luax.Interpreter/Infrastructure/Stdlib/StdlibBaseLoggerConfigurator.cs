using System;
using Luax.Parser.Ast;
using Serilog;

#pragma warning disable S125                // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibBaseLoggerConfigurator
    {
        private static LuaXClassInstance mConfiguratorClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("baseLoggerConfigurator", out mConfiguratorClass);
            mConfiguratorClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__configurator", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
            mConfiguratorClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__rollingInterval", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
            mConfiguratorClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__retainedFileCountLimit", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern baseLoggerConfigurator() : void;
        [LuaXExternMethod("baseLoggerConfigurator", "baseLoggerConfigurator")]
        public static object Constructor(LuaXObjectInstance @this)
        {
            if (@this != null)
            {
                @this.Properties["__configurator"].Value = new LoggerConfiguration();
                @this.Properties["__rollingInterval"].Value = RollingInterval.Infinite;
                @this.Properties["__retainedFileCountLimit"].Value = 31;
                return null;
            }

            var configuratorInstance = new LuaXObjectInstance(mConfiguratorClass);
            configuratorInstance.Properties["__configurator"].Value = new LoggerConfiguration();
            configuratorInstance.Properties["__rollingInterval"].Value = RollingInterval.Infinite;
            configuratorInstance.Properties["__retainedFileCountLimit"].Value = 31;
            return configuratorInstance;
        }

        //public extern setMinimumLevel(level : int) : baseLoggerConfigurator;
        [LuaXExternMethod("baseLoggerConfigurator", "setMinimumLevel")]
        public static object SetMinimumLevel(LuaXObjectInstance @this, int level)
        {
            if (@this.Properties["__configurator"].Value is not LoggerConfiguration configurator)
                throw new ArgumentException("The configurator isn't properly initialized", nameof(@this));

            switch(level)
            {
                case 0:
                    configurator = configurator.MinimumLevel.Verbose();
                    break;
                case 1:
                    configurator = configurator.MinimumLevel.Debug();
                    break;
                case 2:
                    configurator = configurator.MinimumLevel.Information();
                    break;
                case 3:
                    configurator = configurator.MinimumLevel.Warning();
                    break;
                case 4:
                    configurator = configurator.MinimumLevel.Error();
                    break;
                case 5:
                    configurator = configurator.MinimumLevel.Fatal();
                    break;
            }
            @this.Properties["__configurator"].Value = configurator;
            return @this;
        }

        //public extern setWriteToPath(path : string) : baseLoggerConfigurator;
        [LuaXExternMethod("baseLoggerConfigurator", "setWriteToPath")]
        public static object SetWriteToPath(LuaXObjectInstance @this, string path)
        {
            if (@this.Properties["__configurator"].Value is not LoggerConfiguration configurator)
                throw new ArgumentException("The configurator isn't properly initialized", nameof(@this));
            if (@this.Properties["__rollingInterval"].Value is not RollingInterval rollingInterval)
                throw new ArgumentException("The configurator isn't properly initialized", nameof(@this));
            if (@this.Properties["__retainedFileCountLimit"].Value is not int retainedFileCountLimit)
                throw new ArgumentException("The configurator isn't properly initialized", nameof(@this));

            configurator = configurator.WriteTo.File(path, rollingInterval: rollingInterval,
                retainedFileCountLimit: retainedFileCountLimit);
            @this.Properties["__configurator"].Value = configurator;
            return @this;
        }

        //public extern setWriteToConsole() : baseLoggerConfigurator;
        [LuaXExternMethod("baseLoggerConfigurator", "setWriteToConsole")]
        public static object SetWriteToConsole(LuaXObjectInstance @this)
        {
            if (@this.Properties["__configurator"].Value is not LoggerConfiguration configurator)
                throw new ArgumentException("The configurator isn't properly initialized", nameof(@this));

            configurator = configurator.WriteTo.Console();
            @this.Properties["__configurator"].Value = configurator;
            return @this;
        }

        //public extern setRestrictedWriteToConsole(level : int) : baseLoggerConfigurator;
        [LuaXExternMethod("baseLoggerConfigurator", "setRestrictedWriteToConsole")]
        public static object SetRestrictedWriteToConsole(LuaXObjectInstance @this, int level)
        {
            if (@this.Properties["__configurator"].Value is not LoggerConfiguration configurator)
                throw new ArgumentException("The configurator isn't properly initialized", nameof(@this));

            Serilog.Events.LogEventLevel eventLevel = Serilog.Events.LogEventLevel.Verbose;
            switch (level)
            {
                case 1:
                    eventLevel = Serilog.Events.LogEventLevel.Debug;
                    break;
                case 2:
                    eventLevel = Serilog.Events.LogEventLevel.Information;
                    break;
                case 3:
                    eventLevel = Serilog.Events.LogEventLevel.Warning;
                    break;
                case 4:
                    eventLevel = Serilog.Events.LogEventLevel.Error;
                    break;
                case 5:
                    eventLevel = Serilog.Events.LogEventLevel.Fatal;
                    break;
            }
            configurator = configurator.WriteTo.Console(eventLevel);
            @this.Properties["__configurator"].Value = configurator;
            return @this;
        }

        //public extern setRollingInterval(rollingInterval : int) : baseLoggerConfigurator;
        [LuaXExternMethod("baseLoggerConfigurator", "setRollingInterval")]
        public static object SetRollingInterval(LuaXObjectInstance @this, int rollingInterval)
        {
            RollingInterval ri = RollingInterval.Infinite;
            switch (rollingInterval)
            {
                case 0:
                    ri = RollingInterval.Minute;
                    break;
                case 1:
                    ri = RollingInterval.Hour;
                    break;
                case 2:
                    ri = RollingInterval.Day;
                    break;
                case 3:
                    ri = RollingInterval.Month;
                    break;
                case 4:
                    ri = RollingInterval.Day;
                    break;
            }
            @this.Properties["__rollingInterval"].Value = ri;
            return @this;
        }

        //public extern setRetainedFileCountLimit(limit : int) : baseLoggerConfigurator;
        [LuaXExternMethod("baseLoggerConfigurator", "setRetainedFileCountLimit")]
        public static object SetRetainedFileCountLimit(LuaXObjectInstance @this, int limit)
        {
            @this.Properties["__retainedFileCountLimit"].Value = limit;
            return @this;
        }
    }
}
