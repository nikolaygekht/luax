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

        internal LuaXInstanceCallExpression(LuaXTypeDefinition returnType, LuaXExpression @object, string methodName, LuaXElementLocation location)
            : base(methodName, returnType, location)
        {
            Object = @object;
        }

        public override string ToString() => $"call:({Object.ToString()}).{MethodName}({Arguments.ToString()})";
    }
}
