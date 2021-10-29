using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luax.Interpreter.Test
{
    public static class TestValue
    {
        public static object Translate(Type valueType, object value)
        {
            valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            if (valueType == typeof(object))
                return value;

            if (value == null)
                return value;

            if (value.GetType() == valueType)
                return value;

            if (valueType == typeof(DateTime))
            {
                if (value is string s)
                {
                    DateTimeStyles styles = DateTimeStyles.None;
                    if (s.EndsWith("Z"))
                    {
                        styles |= DateTimeStyles.AssumeUniversal;
                        s = s[0..^1];
                    }
                    else if (s.EndsWith("L"))
                    {
                        styles |= DateTimeStyles.AssumeLocal;
                        s = s[0..^1];
                    }

                    if (DateTime.TryParseExact(s, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, styles, out var d1) ||
                        DateTime.TryParseExact(s, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, styles, out d1) ||
                        DateTime.TryParseExact(s, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, styles, out d1) ||
                        DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, styles, out d1))
                    {
                        if (((styles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal) && d1.Kind == DateTimeKind.Local)
                            d1 = d1.ToUniversalTime();
                        return d1;
                    }
                    throw new ArgumentException($"Value {s} has unexpected data format. Try use yyyy-MM-dd format", nameof(value));
                }
                else if (value is int i)
                    return new DateTime((long)i);
                else if (value is long l)
                    return new DateTime(l);
            }
            else if (valueType == typeof(TimeSpan))
            {
                if (value is string s)
                {
                    if (TimeSpan.TryParseExact(s, @"hh\:mm", CultureInfo.InvariantCulture, out var ts))
                        return ts;
                    if (TimeSpan.TryParseExact(s, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out ts))
                        return ts;

                    throw new ArgumentException($"Value {s} has unexpected time format. Try use hh:mm:ss format", nameof(value));
                }
                else if (value is int i)
                    return TimeSpan.FromMilliseconds((long)i);
                else if (value is long l)
                    return TimeSpan.FromMilliseconds(l);
            }
            if (valueType == typeof(decimal))
            {
                if (value is string s)
                {
                    if (decimal.TryParse(s, out decimal r))
                        return r;
                }
                else if (value is int i)
                    return (decimal)i;
                else if (value is double r)
                    return (decimal)r;
            }
            else if (valueType == typeof(byte[]) && value is string sb)
                return Convert.FromHexString(sb);
            else if (valueType == typeof(Guid) && value is string sc)
            {
                if (Guid.TryParse(sc, out var g))
                    return g;
                throw new ArgumentException($"Value {sc} has unexpected guid format. ", nameof(value));
            }
            else if (valueType == typeof(Delegate))
                return value;

            return Convert.ChangeType(value, valueType);
        }
    }
}
