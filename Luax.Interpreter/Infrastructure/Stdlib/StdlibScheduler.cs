using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luax.Interpreter.Expression;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibScheduler
    {
        private static LuaXTypesLibrary mTypeLibrary;
        private static LuaXClassInstance mSchedulerClass;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("scheduler", out mSchedulerClass);
            mSchedulerClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__callback", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
            mSchedulerClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__periodInMilliseconds", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Integer });
            mSchedulerClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__needStop", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Boolean });
            mSchedulerClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__handler", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public static extern create(periodInMilliseconds : int, action: action) : scheduler
        [LuaXExternMethod("scheduler", "create")]
        public static object Create(int periodInMilliseconds, LuaXObjectInstance callback)
        {
            var @this = mSchedulerClass.New(mTypeLibrary);
            @this.Properties["__periodInMilliseconds"].Value = periodInMilliseconds;
            @this.Properties["__callback"].Value = callback;
            @this.Properties["__needStop"].Value = false;
            @this.Properties["__handler"].Value = null;
            return @this;
        }

        //public extern startWithDelay() : void
        [LuaXExternMethod("scheduler", "startImmediately")]
        public static object StartImmediately(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__handler"].Value is CancellationTokenSource tokenSource && !tokenSource.IsCancellationRequested)
                throw new ArgumentException("The scheduler is on run already", nameof(@this));
            if (@this.Properties["__periodInMilliseconds"].Value is not int)
                throw new ArgumentException("The scheduler isn't properly initialized", nameof(@this));
            if (@this.Properties["__needStop"].Value is not bool)
                throw new ArgumentException("The scheduler isn't properly initialized", nameof(@this));
            if (@this.Properties["__callback"].Value is not LuaXObjectInstance callback)
                throw new ArgumentException("The scheduler isn't properly initialized", nameof(@this));
            callback.Class.SearchMethod("invoke", null, out LuaXMethod invoke);

            @this.Properties["__needStop"].Value = false;
            LuaXMethodExecutor.Execute(invoke, mTypeLibrary, callback, Array.Empty<object>(), out var _);

            if (!(bool)@this.Properties["__needStop"].Value)
            {
                StartWithDelay(@this);
            }
            return null;
        }

        //public extern startWithDelay() : void
        [LuaXExternMethod("scheduler", "startWithDelay")]
        public static object StartWithDelay(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__handler"].Value is CancellationTokenSource tokenSource && !tokenSource.IsCancellationRequested)
                throw new ArgumentException("The scheduler is on run already", nameof(@this));
            if (@this.Properties["__periodInMilliseconds"].Value is not int periodInMilliseconds)
                throw new ArgumentException("The scheduler isn't properly initialized", nameof(@this));
            if (@this.Properties["__needStop"].Value is not bool)
                throw new ArgumentException("The scheduler isn't properly initialized", nameof(@this));
            if (@this.Properties["__callback"].Value is not LuaXObjectInstance callback)
                throw new ArgumentException("The scheduler isn't properly initialized", nameof(@this));
            callback.Class.SearchMethod("invoke", null, out LuaXMethod invoke);

            if (periodInMilliseconds >= 0)
            {
                tokenSource = new CancellationTokenSource();
                @this.Properties["__handler"].Value = tokenSource;
                Task.Run(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(periodInMilliseconds);
                        tokenSource.Token.ThrowIfCancellationRequested();
                        LuaXMethodExecutor.Execute(invoke, mTypeLibrary, callback, Array.Empty<object>(), out var _);
                        tokenSource.Token.ThrowIfCancellationRequested();
                    }
                });
            }
            else
            {
                @this.Properties["__needStop"].Value = false;
                while (!(bool)@this.Properties["__needStop"].Value)
                    LuaXMethodExecutor.Execute(invoke, mTypeLibrary, callback, Array.Empty<object>(), out var _);
            }

            return null;
        }

        //public extern stop() : void
        [LuaXExternMethod("scheduler", "stop")]
        public static object Stop(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__needStop"].Value is not bool)
                throw new ArgumentException("The scheduler isn't properly initialized", nameof(@this));

            @this.Properties["__needStop"].Value = true;
            if (@this.Properties["__handler"].Value is CancellationTokenSource tokenSource)
            {
                tokenSource.Cancel();
            }
            return null;
        }
    }
}
