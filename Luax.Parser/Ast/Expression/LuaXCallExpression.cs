namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The base method for all call expression
    /// </summary>
    public abstract class LuaXCallExpression : LuaXExpression
    {
        /// <summary>
        /// The name of the method to call
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Call arguments
        /// </summary>
        public LuaXExpressionCollection Arguments { get; } = new LuaXExpressionCollection();

        protected LuaXCallExpression(string methodName, LuaXTypeDefinition returnType, LuaXElementLocation location)
            : base(returnType, location)
        {
            MethodName = methodName;
        }
    }
}
