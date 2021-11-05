using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Luax.Parser;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// The library of extern method entry points
    /// </summary>
    public class LuaXExternMethodsLibrary
    {
        private readonly Dictionary<string, MethodInfo> mDelegates = new Dictionary<string, MethodInfo>();

        public bool Search(string className, string methodName, out MethodInfo @delegate)
            => mDelegates.TryGetValue($"{className}_{methodName}", out @delegate);

        public void Add(string className, string methodName, MethodInfo @delegate)
            => mDelegates.Add($"{className}_{methodName}", @delegate);

        public void Add(LuaXTypesLibrary library, Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.Name == "Initialize" && method.ReturnType == typeof(void))
                {
                    var args = method.GetParameters();
                    if (args.Length == 1 && args[0].ParameterType == typeof(LuaXTypesLibrary))
                    {
                        method.Invoke(null, new object[] { library });
                    }
                    continue;
                }

                var attr = method.GetCustomAttribute<LuaXExternMethodAttribute>();
                if (attr != null)
                {
                    var args = method.GetParameters();

                    if (method.ReturnType != typeof(object))
                        throw new ArgumentException($"Type {type.Name} has method {attr.MethodName} that does not match function prototype");

                    if (!library.SearchClass(attr.ClassName, out var @class))
                        throw new ArgumentException($"Type {type.Name} method {attr.MethodName} refers to LuaX class {attr.ClassName} that does not exist");

                    if (!@class.SearchMethod(attr.MethodName, null, out var @luaMethod))
                        throw new ArgumentException($"Type {type.Name} method {attr.MethodName} refers to LuaX method {attr.ClassName}.{attr.MethodName} that does not exist");

                    int offset = luaMethod.Static ? 0 : 1;

                    if (args.Length != luaMethod.Arguments.Count + offset)
                        throw new ArgumentException($"Type {type.Name} method {attr.MethodName} argument count does not match to the prototype {attr.ClassName}.{attr.MethodName}");

                    if (!luaMethod.Static && args[0].ParameterType != typeof(LuaXObjectInstance))
                        throw new ArgumentException($"Type {type.Name} method {attr.MethodName} this argument does not match to the prototype {attr.ClassName}.{attr.MethodName}");

                    for (int i = 0; i < luaMethod.Arguments.Count; i++)
                    {
                        var luaType = luaMethod.Arguments[i].LuaType;
                        if (luaType.IsObject() && args[i + offset].ParameterType != typeof(LuaXObjectInstance) ||
                            luaType.Array && args[i + offset].ParameterType != typeof(LuaXVariableInstanceArray) ||
                            luaType.IsInteger() && args[i + offset].ParameterType != typeof(int) ||
                            luaType.IsReal() && args[i + offset].ParameterType != typeof(double) ||
                            luaType.IsString() && args[i + offset].ParameterType != typeof(string) ||
                            luaType.IsBoolean() && args[i + offset].ParameterType != typeof(bool) ||
                            luaType.IsDate() && args[i + offset].ParameterType != typeof(DateTime))
                            throw new ArgumentException($"Type {type.Name} method {attr.MethodName} {i + 1}th argument does not match to the prototype {attr.ClassName}.{attr.MethodName}");
                    }
                    Add(attr.ClassName, attr.MethodName, method);
                }
            }
        }
    }
}
