namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The array access expression.
    /// </summary>
    public class LuaXArrayAccessExpression : LuaXExpression
    {
        public LuaXExpression ArrayExpression { get; }
        public LuaXExpression IndexExpression { get; }

        internal LuaXArrayAccessExpression(LuaXExpression arrayExpression, LuaXExpression indexExpression, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            ArrayExpression = arrayExpression;
            IndexExpression = indexExpression;
        }
    }
}
