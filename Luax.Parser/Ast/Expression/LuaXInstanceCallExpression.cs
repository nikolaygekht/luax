namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// Call of a static method
    /// </summary>
    public class LuaXInstanceCallExpression : LuaXCallExpression
    {
        /// <summary>
        /// The instance of the class to call
        /// </summary>
        public LuaXExpression Object { get; }

        /// <summary>
        /// The exact class name to search the method implementation
        ///
        /// The property is used, for example, to implement super.method call (call a parent class implementation instead of the most recent one)
        /// </summary>
        public string ExactClass { get; }

        internal LuaXInstanceCallExpression(LuaXTypeDefinition returnType, LuaXExpression @object, string methodName, string exactClass, LuaXElementLocation location)
            : base(methodName, returnType, location)
        {
            Object = @object;
            ExactClass = exactClass;
        }

        public override string ToString() => $"call:({Object.ToString()}).{MethodName}({Arguments.ToString()})";
    }
}
