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
        private readonly Dictionary<string, LuaXExternMethodImplementationDelegate> mDelegates = new Dictionary<string, LuaXExternMethodImplementationDelegate>();

        public bool Search(string className, string methodName, out LuaXExternMethodImplementationDelegate @delegate)
            => mDelegates.TryGetValue($"{className}_{methodName}", out @delegate);

        public void Add(string className, string methodName, LuaXExternMethodImplementationDelegate @delegate)
            => mDelegates.Add($"{className}_{methodName}", @delegate);

        public void Add(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<LuaXExternMethodAttribute>();
                if (attr != null)
                {
                    var args = method.GetParameters();
                    if (method.ReturnType != typeof(object) ||
                        args.Length != 2 ||
                        args[0].ParameterType != typeof(LuaXObjectInstance) ||
                        args[1].ParameterType != typeof(object[]))
                        throw new ArgumentException($"Type {type.Name} has method {attr.MethodName} that does not match {nameof(LuaXExternMethodImplementationDelegate)} prototype");
                    Add(attr.ClassName, attr.MethodName, method.CreateDelegate<LuaXExternMethodImplementationDelegate>());
                }
            }
        }
    }
}
