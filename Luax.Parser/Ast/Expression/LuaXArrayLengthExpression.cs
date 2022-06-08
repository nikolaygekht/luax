namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The array index expression.
    /// </summary>
    public class LuaXArrayLengthExpression : LuaXExpression
    {
        public LuaXExpression ArrayExpression { get; }

        public LuaXArrayLengthExpression(LuaXExpression arrayExpression, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            ArrayExpression = arrayExpression;
        }

        public override string ToString() => $"{ArrayExpression.ToString()}.length";
    }
}
