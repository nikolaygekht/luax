using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Luax.Parser;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// The library of types
    /// </summary>
    public class LuaXTypesLibrary
    {
        private readonly LuaXApplication mApplication;
        private readonly Dictionary<string, LuaXClassInstance> mTypes = new Dictionary<string, LuaXClassInstance>();

        public LuaXTypesLibrary(LuaXApplication application)
        {
            mApplication = application;
            foreach (var @class in application.Classes)
                mTypes.Add(@class.Name, new LuaXClassInstance(@class));
        }

        public bool SearchClass(string className, out LuaXClassInstance @class)
            => mTypes.TryGetValue(className, out @class);

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

        internal bool CastTo(LuaXTypeDefinition returnType, ref object argument)
        {
            switch (returnType.TypeId)
            {
                case LuaXType.Integer:
                    {
                        if (argument is int)
                            return true;
                        if (argument is double r)
                        {
                            argument = (int)r;
                            return true;
                        }
                        if (argument is bool b)
                        {
                            argument = b ? 1 : 0;
                            return true;
                        }
                        if (argument is DateTime d)
                        {
                            argument = ToJDN(d);
                            return true;
                        }
                        if (argument is string s1)
                        {
                            if (Int32.TryParse(s1, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i1))
                            {
                                argument = i1;
                                return true;
                            }
                            return false;
                        }
                    }
                    return false;
                case LuaXType.Real:
                    {
                        if (argument is int i)
                        {
                            argument = (double)i;
                            return true;
                        }
                        if (argument is double)
                            return true;
                        if (argument is bool b)
                        {
                            argument = b ? 1.0 : 0.0;
                            return true;
                        }
                        if (argument is DateTime d)
                        {
                            argument = DateToJDN(d);
                            return true;
                        }
                        if (argument is string s2)
                        {
                            if (Double.TryParse(s2, NumberStyles.Float, CultureInfo.InvariantCulture, out double i1))
                            {
                                argument = i1;
                                return true;
                            }
                            return false;
                        }
                    }
                    return false;
                case LuaXType.Datetime:
                    {
                        if (argument is int i)
                        {
                            argument = JdnToDate(i);
                            return true;
                        }
                        if (argument is double r)
                        {
                            argument = JdnToDate(r);
                            return true;
                        }
                        if (argument is DateTime )
                        {
                            return true;
                        }
                        if (argument is string s3)
                        {
                            if (DateTime.TryParseExact(s3, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1))
                            {
                                argument = d1;
                                return true;
                            }
                            if (DateTime.TryParseExact(s3, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2))
                            {
                                argument = d2;
                                return true;
                            }
                            if (DateTime.TryParseExact(s3, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d3))
                            {
                                argument = d3;
                                return true;
                            }
                            if (DateTime.TryParseExact(s3, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d4))
                            {
                                argument = d4;
                                return true;
                            }
                            return false;
                        }
                    }
                    return false;
                case LuaXType.String:
                    {
                        if (argument == null)
                            return true;
                        if (argument is int i)
                        {
                            argument = i.ToString(CultureInfo.InvariantCulture);
                            return true;
                        }
                        if (argument is double r)
                        {
                            argument = r.ToString(CultureInfo.InvariantCulture);
                            return true;
                        }
                        if (argument is bool b)
                        {
                            argument = b ? "true" : "false";
                            return true;
                        }
                        if (argument is DateTime d)
                        {
                            if (d.Hour == 0 && d.Minute == 0 && d.Second == 0 && d.Millisecond == 0)
                                argument = d.ToString("yyyy-MM-dd");
                            else if (d.Millisecond == 0)
                                argument = d.ToString("yyyy-MM-dd HH:mm:ss");
                            else
                                argument = d.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            return true;
                        }
                        if (argument is string)
                            return true;
                    }
                    return false;
                case LuaXType.Boolean:
                    {
                        if (argument is bool)
                            return true;
                        if (argument is string s)
                        {
                            if (s == "true")
                            {
                                argument = true;
                                return true;
                            }
                            if (s == "false")
                            {
                                argument = false;
                                return true;
                            }
                            return false;
                        }
                    }
                    return false;
                case LuaXType.Object:
                    if (argument == null)
                        return true;
                    if (argument is LuaXObjectInstance instance)
                    {
                        return mApplication.Classes.Search(returnType.Class, out var target) &&
                            mApplication.Classes.IsKindOf(instance.Class.LuaType, target);
                    }
                    return false;
                default:
                    return false;
            }
        }
    }
}
