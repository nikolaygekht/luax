#pragma warning disable S125 // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibBitwise
    {
        //public static extern _and(a : int, b : int) : int;
        [LuaXExternMethod("bitwise", "_and")]
        public static object And(int a, int b)
        {
            return a & b;
        }
        //public static extern _or(a : int, b : int) : int;
        [LuaXExternMethod("bitwise", "_or")]
        public static object Or(int a, int b)
        {
            return a | b;
        }
        //public static extern _xor(a : int, b : int) : int;
        [LuaXExternMethod("bitwise", "_xor")]
        public static object Xor(int a, int b)
        {
            return a ^ b;
        }
        //public static extern _not(a : int) : int;
        [LuaXExternMethod("bitwise", "_not")]
        public static object Not(int a)
        {
            return (int)(a ^ 0xffff_ffff);
        }
        //public static extern _shl(a : int, n : int) : int;
        [LuaXExternMethod("bitwise", "_shl")]
        public static object Shl(int a, int n)
        {
            return a << n;
        }
        //public static extern _shr(a : int, n : int) : int;
        [LuaXExternMethod("bitwise", "_shr")]
        public static object Shr(int a, int n)
        {
            return (int)((uint)a >> n);
        }
    }
}
