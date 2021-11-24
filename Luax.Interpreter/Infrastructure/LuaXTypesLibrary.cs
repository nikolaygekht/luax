using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Luax.Interpreter.Infrastructure.Stdlib;
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
        public LuaXExternMethodsLibrary ExternMethods { get; } = new LuaXExternMethodsLibrary();

        public LuaXTypesLibrary(LuaXApplication application)
        {
            mApplication = application;

            mTypes.Add("object", new LuaXClassInstance(LuaXClass.Object));

            foreach (var @class in application.Classes)
                mTypes.Add(@class.Name, new LuaXClassInstance(@class));

            ExternMethods.Add(this, typeof(StdlibString));
            ExternMethods.Add(this, typeof(StdlibMath));
            ExternMethods.Add(this, typeof(StdlibDate));
            ExternMethods.Add(this, typeof(StdlibMix));
            ExternMethods.Add(this, typeof(StdlibAssert));
            ExternMethods.Add(this, typeof(StdlibList));
            ExternMethods.Add(this, typeof(StdlibQueue));
            ExternMethods.Add(this, typeof(StdlibStack));
            ExternMethods.Add(this, typeof(StdlibBuffer));
            ExternMethods.Add(this, typeof(StdlibIO));
            ExternMethods.Add(this, typeof(StdlibVariant));
            ExternMethods.Add(this, typeof(StdlibBitwise));
            ExternMethods.Add(this, typeof(StdlibCsvParser));
            ExternMethods.Add(this, typeof(StdlibXml));
            ExternMethods.Add(this, typeof(StdlibSortedList));
            ExternMethods.Add(this, typeof(StdlibIntMap));
        }

        public string[] GetClassNames() => mTypes.Keys.ToArray();

        public bool SearchClass(string className, out LuaXClassInstance @class)
            => mTypes.TryGetValue(className, out @class);

        public bool IsKindOf(string sourceClassName, string targetClassName) =>
            mApplication.Classes.IsKindOf(sourceClassName, targetClassName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CastInteger(ref object argument)
        {
            switch (argument)
            {
                case int:
                    return true;
                case double r:
                    argument = (int)r;
                    return true;
                case bool b:
                    argument = b ? 1 : 0;
                    return true;
                case DateTime d:
                    argument = StdlibDate.ToJDN(d);
                    return true;
                case string s1 when int.TryParse(s1, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i1):
                    argument = i1;
                    return true;
                case string :
                    return false;
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CastReal(ref object argument)
        {
            switch (argument)
            {
                case int i:
                    argument = (double)i;
                    return true;
                case double:
                    return true;
                case bool b:
                    argument = b ? 1.0 : 0.0;
                    return true;
                case DateTime d:
                    argument = StdlibDate.DateToJDN(d);
                    return true;
                case string s2 when double.TryParse(s2, NumberStyles.Float, CultureInfo.InvariantCulture, out double i1):
                    argument = i1;
                    return true;
                case string :
                    return false;
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CastDateTime(ref object argument)
        {
            switch (argument)
            {
                case int i:
                    argument = StdlibDate.JdnToDate(i);
                    return true;
                case double r:
                    argument = StdlibDate.JdnToDate(r);
                    return true;
                case DateTime:
                    return true;
                case string s3 when DateTime.TryParseExact(s3, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var d1):
                    argument = d1;
                    return true;
                case string s3 when DateTime.TryParseExact(s3, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var d2):
                    argument = d2;
                    return true;
                case string s3 when DateTime.TryParseExact(s3, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var d3):
                    argument = d3;
                    return true;
                case string s3 when DateTime.TryParseExact(s3, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var d4):
                    argument = d4;
                    return true;
                case string :
                    return false;
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CastString(ref object argument)
        {
            switch (argument)
            {
                case null:
                    return true;
                case int i:
                    argument = i.ToString(CultureInfo.InvariantCulture);
                    return true;
                case double r:
                    argument = r.ToString(CultureInfo.InvariantCulture);
                    return true;
                case bool b:
                    argument = b ? "true" : "false";
                    return true;
                case DateTime d:
                {
                    if (d.Hour == 0 && d.Minute == 0 && d.Second == 0 && d.Millisecond == 0)
                        argument = d.ToString("yyyy-MM-dd");
                    else if (d.Millisecond == 0)
                        argument = d.ToString("yyyy-MM-dd HH:mm:ss");
                    else
                        argument = d.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    return true;
                }
                default:
                    return argument is string;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CastBoolean(ref object argument)
        {
            switch (argument)
            {
                case bool:
                    return true;
                case string s when s == "true":
                    argument = true;
                    return true;
                case string s when s == "false":
                    argument = false;
                    return true;
                case string :
                    return false;
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool CastTo(LuaXTypeDefinition returnType, ref object argument)
        {
            switch (returnType.TypeId)
            {
                case LuaXType.Integer:
                    return CastInteger(ref argument);
                case LuaXType.Real:
                    return CastReal(ref argument);
                case LuaXType.Datetime:
                    return CastDateTime(ref argument);
                case LuaXType.String:
                    return CastString(ref argument);
                case LuaXType.Boolean:
                    return CastBoolean(ref argument);
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