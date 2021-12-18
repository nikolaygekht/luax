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

        private static bool IsInitialize(MethodInfo method)
            => method.Name == "Initialize" && method.ReturnType == typeof(void);
        
        private static void Initialize(MethodInfo method, LuaXTypesLibrary library)
        {
            var args = method.GetParameters();
            if (args.Length == 1 && args[0].ParameterType == typeof(LuaXTypesLibrary))
                method.Invoke(null, new object[] { library });
            else if (args.Length == 0)
                method.Invoke(null, Array.Empty<object>());
        }

        private static void ValidateMethod(MethodInfo method, LuaXTypesLibrary library, Type type, LuaXExternMethodAttribute attr, out LuaXMethod luaMethod)
        {
            var args = method.GetParameters();

            if (method.ReturnType != typeof(object))
                throw new ArgumentException($"Type {type.Name} has method {attr.MethodName} that does not match function prototype");

            if (!library.SearchClass(attr.ClassName, out var @class))
                throw new ArgumentException($"Type {type.Name} method {attr.MethodName} refers to LuaX class {attr.ClassName} that does not exist");

            if (!@class.SearchMethod(attr.MethodName, null, out luaMethod))
                throw new ArgumentException($"Type {type.Name} method {attr.MethodName} refers to LuaX method {attr.ClassName}.{attr.MethodName} that does not exist");

            if (args.Length != luaMethod.Arguments.Count + (luaMethod.Static ? 0 : 1))
                throw new ArgumentException($"Type {type.Name} method {attr.MethodName} argument count does not match to the prototype {attr.ClassName}.{attr.MethodName}");

            if (!luaMethod.Static && args[0].ParameterType != typeof(LuaXObjectInstance))
                throw new ArgumentException($"Type {type.Name} method {attr.MethodName} this argument does not match to the prototype {attr.ClassName}.{attr.MethodName}");
        }

        private static bool ValidateParameter(LuaXTypeDefinition luaType, Type parameterType)
        {
            return !(luaType.IsObject() && parameterType != typeof(LuaXObjectInstance) ||
                    luaType.Array && parameterType != typeof(LuaXVariableInstanceArray) ||
                    luaType.IsInteger() && parameterType != typeof(int) ||
                    luaType.IsReal() && parameterType != typeof(double) ||
                    luaType.IsString() && parameterType != typeof(string) ||
                    luaType.IsBoolean() && parameterType != typeof(bool) ||
                    luaType.IsDate() && parameterType != typeof(DateTime));
        }

        public void Add(LuaXTypesLibrary library, Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (IsInitialize(method))
                    Initialize(method, library);

                var attr = method.GetCustomAttribute<LuaXExternMethodAttribute>();
                if (attr != null)
                {
                    ValidateMethod(method, library, type, attr, out var luaMethod);

                    int offset = luaMethod.Static ? 0 : 1;

                    var args = method.GetParameters();
                    for (int i = 0; i < luaMethod.Arguments.Count; i++)
                    {
                        var luaType = luaMethod.Arguments[i].LuaType;
                        if (!ValidateParameter(luaType, args[i + offset].ParameterType))
                            throw new ArgumentException($"Type {type.Name} method {attr.MethodName} {i + 1}th argument does not match to the prototype {attr.ClassName}.{attr.MethodName}");
                    }
                    Add(attr.ClassName, attr.MethodName, method);
                }
            }
        }

        public void Add(string className, string methodName, MethodInfo @delegate)
            => mDelegates.Add($"{className}_{methodName}", @delegate);
    }
}

