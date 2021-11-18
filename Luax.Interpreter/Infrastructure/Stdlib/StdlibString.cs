using System;
using System.Text.RegularExpressions;

#pragma warning disable S125 // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibString
    {
        //public static extern len(s : string) : int;
        [LuaXExternMethod("stdlib", "len")]
        public static object Extern_len(string s)
        {
            return s.Length;
        }

        //public static extern indexOf(s : string, sub:string) : int;
        [LuaXExternMethod("stdlib", "indexOf")]
        public static object Extern_indexOf(string s, string sub, bool caseInsensitive)
        {
            return s.IndexOf(sub, caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        //public static extern left(s : string, length : int) : string;
        [LuaXExternMethod("stdlib", "left")]
        public static object Extern_left(string s, int length)
        {
            return s[..length];
        }

        //public static extern trim(s : string) : string;
        [LuaXExternMethod("stdlib", "trim")]
        public static object Extern_trim(string s)
        {
            return s.Trim();
        }

        //public static extern right(s : string, length : int) : string;
        [LuaXExternMethod("stdlib", "right")]
        public static object Extern_right(string s, int length)
        {
            return s[^length..];
        }

        //public static extern substring(s : string, from : int, length : int) : string;
        [LuaXExternMethod("stdlib", "substring")]
        public static object Extern_substring(string s, int from, int length)
        {
            return s.Substring(from, length);
        }

        //public static extern match(s : string, re : string) : boolean;
        [LuaXExternMethod("stdlib", "match")]
        public static object Extern_match(string s, string re)
        {
            var m = Regex.Match(s, re);
            return m.Success;
        }

        //public static extern unicode(s : string) : int;
        [LuaXExternMethod("stdlib", "unicode")]
        public static object Extern_unicode(string s, int index)
        {
            return ((int)s[index]) & 0xffff;
        }

        //public static extern char(unicode : int) : string;
        [LuaXExternMethod("stdlib", "char")]
        public static object Extern_char(int unicode)
        {
            return new string((char)(unicode & 0xffff), 1);
        }
    }
}