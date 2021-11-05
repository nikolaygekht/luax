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
            
            ExternMethods.Add(this, typeof(StdlibDate));
            ExternMethods.Add(this, typeof(StdlibMix));
            ExternMethods.Add(this, typeof(StdlibAssert));
            ExternMethods.Add(this, typeof(StdlibList));
        }

        public string[] GetClassNames() => mTypes.Keys.ToArray();

        public bool SearchClass(string className, out LuaXClassInstance @class)
            => mTypes.TryGetValue(className, out @class);

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
                            argument = StdlibDate.ToJDN(d);
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
                            argument = StdlibDate.DateToJDN(d);
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
                            argument = StdlibDate.JdnToDate(i);
                            return true;
                        }
                        if (argument is double r)
                        {
                            argument = StdlibDate.JdnToDate(r);
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
