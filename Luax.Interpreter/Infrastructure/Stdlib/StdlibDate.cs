using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Luax.Parser.Ast;

#pragma warning disable S125 // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibDate
    {
        //public static extern mkdate(year : int, month : int, day : int) : datetime;
        [LuaXExternMethod("stdlib", "mkdate")]
        public static object Extern_mkdate(int year, int month, int day)
        {
            return new DateTime(year, month, day);
        }

        //public static extern mkdatetime(year : int, month : int, day : int, int hour, int minute, int second, int milliseconds) : datetime
        [LuaXExternMethod("stdlib", "mkdatetime")]
        public static object Extern_mkdatetime(int year, int month, int day, int hour, int minute, int section, int milliseconds)
        {
            return new DateTime(year, month, day, hour, minute, section, milliseconds);
        }
        //public static extern toJdn(x : datetime) : real;
        [LuaXExternMethod("stdlib", "toJdn")]
        public static object Extern_toJdn(DateTime x)
        {
            return DateToJDN(x);
        }
        //public static extern fromJdn(x : real) : datetime;
        [LuaXExternMethod("stdlib", "fromJdn")]
        public static object Extern_fromJdn(double x)
        {
            return JdnToDate(x);
        }
        //public static extern day(x : datetime) : int;
        [LuaXExternMethod("stdlib", "day")]
        public static object Extern_day(DateTime x)
        {
            return x.Day;
        }
        //public static extern dayOfWeek(x : datetime) : int;
        [LuaXExternMethod("stdlib", "dayOfWeek")]
        public static object Extern_dayOfWeek(DateTime x)
        {
            return x.DayOfWeek;
        }

        //public static extern month(x : datetime) : int;
        [LuaXExternMethod("stdlib", "month")]
        public static object Extern_month(DateTime x)
        {
            return x.Month;
        }

        //public static extern year(x : datetime) : int;
        [LuaXExternMethod("stdlib", "year")]
        public static object Extern_year(DateTime x)
        {
            return x.Year;
        }

        //public static extern leapYear(x : datetime) : boolean;
        [LuaXExternMethod("stdlib", "leapYear")]
        public static object Extern_leapYear(DateTime x)
        {
            var year = x.Year;
            if (year % 4 != 0)
                return false;
            if (year % 400 == 0)
                return true;
            return year % 100 != 0;
        }

        //public static extern hour(x : datetime) : int;
        [LuaXExternMethod("stdlib", "hour")]
        public static object Extern_hour(DateTime x)
        {
            return x.Hour;
        }

        //public static extern minute(x : datetime) : int;
        [LuaXExternMethod("stdlib", "minute")]
        public static object Extern_minute(DateTime x)
        {
            return x.Minute;
        }

        //public static extern second(x : datetime) : int;
        [LuaXExternMethod("stdlib", "second")]
        public static object Extern_second(DateTime x)
        {
            return x.Second;
        }

        //public static extern seconds(x : datetime) : real;
        [LuaXExternMethod("stdlib", "seconds")]

        public static object Extern_seconds(DateTime x)
        {
            return x.Second + x.Millisecond / 1000.0;
        }

        //public static extern nowlocal() : datetime;
        [LuaXExternMethod("stdlib", "nowlocal")]

        public static object Extern_local()
        {
            return DateTime.Now;
        }

        //public static extern nowutc() : datetime;
        [LuaXExternMethod("stdlib", "nowutc")]

        public static object Extern_utc()
        {
            return DateTime.UtcNow;
        }

        internal static int ToJDN(DateTime dateTime)
            => (1461 * ((dateTime.Year) + 4800 + (dateTime.Month - 14) / 12)) / 4 + (367 * (dateTime.Month - 2 - 12 * ((dateTime.Month - 14) / 12))) / 12 - (3 * (((dateTime.Year) + 4900 + (dateTime.Month - 14) / 12) / 100)) / 4 + dateTime.Day - 32075;

        internal static double ToJT(DateTime dateTime)
            => (dateTime.Hour * 3600.0 + dateTime.Minute * 60.0 + dateTime.Second + dateTime.Millisecond / 1000.0) / 86400.0;

        public static double DateToJDN(DateTime date) => ToJDN(date) + ToJT(date);

        public static DateTime JdnToDate(double jdn)
        {
            FromJDN((int)jdn, out var year, out var month, out var day);
            FromJT(jdn, out var hour, out var minute, out var second, out var millisecond);
            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        internal static void FromJDN(int jdn, out int year, out int month, out int day)
        {
            const int y = 4716, v = 3, j = 1401, u = 5, m = 2, s = 153, n = 12, w = 2, r = 4, B = 274277, p = 1461, C = -38;

            var f = jdn + j + (((4 * jdn + B) / 146097) * 3) / 4 + C;
            var e = r * f + v;
            var g = (e % p) / r;
            var h = u * g + w;
            day = (h % s) / u + 1;
            month = (h / s + m) % n + 1;
            year = (e / p) - y + (n + m - month) / n;
        }

        internal static void FromJT(double jt, out int hour, out int minute, out int second, out int milliseconds)
        {
            jt = (jt - (int)jt) * 86400.0;
            milliseconds = (int)((jt - (int)jt) * 1000);
            var jti = (int)jt;
            second = jti % 60;
            jti /= 60;
            minute = jti % 60;
            jti /= 60;
            hour = jti;
        }
    }
}
