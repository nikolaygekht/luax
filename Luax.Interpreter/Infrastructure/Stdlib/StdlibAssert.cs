using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Luax.Interpreter.Expression;

#pragma warning disable S125                // Sections of code should not be commented out
#pragma warning disable IDE1006             // Naming rule violation.

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibAssert
    {
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
        }

        //public static extern isTrue(condition : boolean, message : string) : void;
        [LuaXExternMethod("assert", "isTrue")]
        public static object isTrue(bool condition, string message)
        {
            if (!condition)
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException("Expected the condition to be true but it is false" + (message ?? ""));
            }
            return null;
        }

        //public static extern isTrue(condition : boolean, message : string) : void;
        [LuaXExternMethod("assert", "isFalse")]
        public static object isFalse(bool condition, string message)
        {
            if (condition)
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException("Expected the condition to be false but it is true" + (message ?? ""));
            }
            return null;
        }

        private static bool throws(LuaXObjectInstance action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (!action.Class.SearchMethod("invoke", null, out var method) || method.Arguments.Count != 0 || method.Static)
                throw new ArgumentException("The object passed does not have invoke method", nameof(action));

            bool thrown = false;
            try
            {
                LuaXMethodExecutor.Execute(method, mTypeLibrary, action, Array.Empty<object>(), out var _);
            }
            catch (LuaXExecutionException)
            {
                thrown = true;
            }
            return thrown;
        }

        //public static extern throws(action : assertAction, message : string) : void
        [LuaXExternMethod("assert", "throws")]
        public static object throws(LuaXObjectInstance action, string message)
        {
            if (!throws(action))
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException("Expected the action to throw the exception but it does not" + (message ?? ""));
            }

            return null;
        }

        //public static extern throws(action : assertAction, message : string) : void
        [LuaXExternMethod("assert", "doesNotThrow")]
        public static object doesNotThrow(LuaXObjectInstance action, string message)
        {
            if (throws(action))
            {
                if (!string.IsNullOrEmpty(message))
                    message = " because " + message;
                throw new LuaXAssertionException("Expected the action to not throw the exception but it does" + (message ?? ""));
            }

            return null;
        }
    }
}
