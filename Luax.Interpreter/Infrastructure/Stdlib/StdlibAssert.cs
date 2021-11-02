using Luax.Parser.Ast;

#pragma warning disable S125                // Sections of code should not be commented out
#pragma warning disable IDE1006             // Naming rule violation.
#pragma warning disable RCS1163, IDE0060    // Unused parameters.

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibAssert
    {
        //public static extern isTrue(condition : boolean, message : string) : void;
        [LuaXExternMethod("assert", "isTrue")]
        public static object isTrue(LuaXElementLocation location, LuaXObjectInstance _, object[] args)
        {
            if (!(bool)args[0])
                throw new LuaXAssertionException($"Expected the condition to be true but it is false because {(string)args[1]}");
            return null;
        }

        //public static extern isTrue(condition : boolean, message : string) : void;
        [LuaXExternMethod("assert", "isFalse")]
        public static object isFalse(LuaXElementLocation location, LuaXObjectInstance _, object[] args)
        {
            if ((bool)args[0])
                throw new LuaXAssertionException($"Expected the condition to be false but it is false because {(string)args[1]}");
            return null;
        }
    }
}
