using System;

#pragma warning disable S125 // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibMath
    {
        //public static extern iabs(x : int) : int;
        [LuaXExternMethod("stdlib", "iabs")]
        public static object Extern_iabs(int x)
        {
            return x < 0 ? -x : x;
        }
        //public static extern isgn(x : int) : int;
        [LuaXExternMethod("stdlib", "isgn")]
        public static object Extern_isgn(int x)
        {
#pragma warning disable S3358 // Ternary operators should not be nested
            return x > 0 ? 1 : (x < 0 ? -1 : 0);
#pragma warning restore S3358 // Ternary operators should not be nested

        }
        //public static extern abs(x : real) : real;
        [LuaXExternMethod("stdlib", "abs")]
        public static object Extern_abs(double x)
        {
            return Math.Abs(x);
        }
        //public static extern sgn(x : real) : int;
        [LuaXExternMethod("stdlib", "sgn")]
        public static object Extern_sgn(double x)
        {
#pragma warning disable S3358 // Ternary operators should not be nested
            return x > 0 ? 1 : (x < 0 ? -1 : 0);
#pragma warning restore S3358 // Ternary operators should not be nested
        }
        //public static extern sin(x : real) : real;
        [LuaXExternMethod("stdlib", "sin")]
        public static object Extern_sin(double x)
        {
            return Math.Sin(x);
        }
        //public static extern cos(x : real) : real;
        [LuaXExternMethod("stdlib", "cos")]
        public static object Extern_cos(double x)
        {
            return Math.Cos(x);
        }
        //public static extern tan(x : real) : real;
        [LuaXExternMethod("stdlib", "tan")]
        public static object Extern_tan(double x)
        {
            return Math.Tan(x);
        }
        //public static extern asin(x : real) : real;
        [LuaXExternMethod("stdlib", "asin")]
        public static object Extern_asin(double x)
        {
            return Math.Asin(x);
        }
        //public static extern acos(x : real) : real;
        [LuaXExternMethod("stdlib", "acos")]
        public static object Extern_acos(double x)
        {
            return Math.Acos(x);
        }
        //public static extern atan(x : real) : real;
        [LuaXExternMethod("stdlib", "atan")]
        public static object Extern_atan(double x)
        {
            return Math.Atan(x);
        }
        //public static extern atan2(y : real, x : real) : real;
        [LuaXExternMethod("stdlib", "atan2")]
        public static object Extern_atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }
        //public static extern sqrt(x : real) : real;
        [LuaXExternMethod("stdlib", "sqrt")]
        public static object Extern_sqrt(double x)
        {
            return Math.Sqrt(x);
        }
        //public static extern ceil(x : real) : real;
        [LuaXExternMethod("stdlib", "ceil")]
        public static object Extern_ceil(double x)
        {
            return Math.Ceiling(x);
        }
        //public static extern floor(x : real) : real;
        [LuaXExternMethod("stdlib", "floor")]
        public static object Extern_floor(double x)
        {
            return Math.Floor(x);
        }
        //public static extern round(x : real, digits : int) : real;
        [LuaXExternMethod("stdlib", "round")]
        public static object Extern_round(double x, int digits)
        {
            var dMult = 1.0;

            for (var i = 0; i < digits; i++)
            {
                dMult *= 10.0;
            }

            var val = x * dMult;
            return val < 0 ? Extern_ceil(val - 0.5) : Extern_floor(val + 0.5);
        }
        //public static extern log(x : real) : real;
        [LuaXExternMethod("stdlib", "log")]
        public static object Extern_log(double x)
        {
            return Math.Log(x);
        }
        //public static extern log10(x : real) : real;
        [LuaXExternMethod("stdlib", "log10")]
        public static object Extern_log10(double x)
        {
            return Math.Log10(x);
        }
    }
}