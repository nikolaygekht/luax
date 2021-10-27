namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The array index expression.
    /// </summary>
    public class LuaXArrayLengthExpression : LuaXExpression
    {
        public LuaXExpression ArrayExpression { get; }

        internal LuaXArrayLengthExpression(LuaXExpression arrayExpression, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            ArrayExpression = arrayExpression;
        }
    }
}
