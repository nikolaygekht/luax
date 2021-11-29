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
            LuaXObjectInstance instance = @this;
            object retval = null;

            if (instance == null)
            {
                instance = new LuaXObjectInstance(mConfiguratorClass);
                retval = instance;
            }

            instance.Properties["__configurator"].Value = new LoggerConfiguration();
            instance.Properties["__rollingInterval"].Value = RollingInterval.Infinite;
            instance.Properties["__retainedFileCountLimit"].Value = 31;
            return retval;
        }

        //public extern setMinimumLevel(level : int) : baseLoggerConfigurator;
        [LuaXExternMethod("baseLoggerConfigurator", "setMinimumLevel")]
        public static object SetMinimumLevel(LuaXObjectInstance @this, int level)
        {
            if (@this.Properties["__configurator"].Value is not LoggerConfiguration configurator)
                throw new ArgumentException("The configurator isn't properly initialized", nameof(@this));

            configurator = configurator.MinimumLevel.ControlledBy(new Serilog.Core.LoggingLevelSwitch(GetLogEventLevel(level)));
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

            Serilog.Events.LogEventLevel eventLevel = GetLogEventLevel(level);
            configurator = configurator.WriteTo.Console(eventLevel);
            @this.Properties["__configurator"].Value = configurator;
            return @this;
        }

        //public extern setRollingInterval(rollingInterval : int) : baseLoggerConfigurator;
        [LuaXExternMethod("baseLoggerConfigurator", "setRollingInterval")]
        public static object SetRollingInterval(LuaXObjectInstance @this, int rollingInterval)
        {
            RollingInterval ri = RollingInterval.Infinite;
            if (rollingInterval >= 0 && rollingInterval <= 5)
                ri = (RollingInterval)rollingInterval;

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

        public static Serilog.Events.LogEventLevel GetLogEventLevel(int level)
        {
            Serilog.Events.LogEventLevel eventLevel = Serilog.Events.LogEventLevel.Verbose;
            if (level >= 0 && level <= 5)
                eventLevel = (Serilog.Events.LogEventLevel)level;
            return eventLevel;
        }
    }
}
