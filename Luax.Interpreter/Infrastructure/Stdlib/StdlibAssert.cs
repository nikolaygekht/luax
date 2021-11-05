using System.Reflection;

#pragma warning disable S125                // Sections of code should not be commented out
#pragma warning disable IDE1006             // Naming rule violation.

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibAssert
    {
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
    }
}
