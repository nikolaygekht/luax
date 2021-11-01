using System;

namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// The attribute to markup the LuaX extern methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LuaXExternMethodAttribute : Attribute
    {
        public string ClassName { get; set; }
        public string MethodName { get; set; }

        public LuaXExternMethodAttribute()
        {
        }
        public LuaXExternMethodAttribute(string className, string methodName)
        {
            ClassName = className;
            MethodName = methodName;
        }
    }
}
