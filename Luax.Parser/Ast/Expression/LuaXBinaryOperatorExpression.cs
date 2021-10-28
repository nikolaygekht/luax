namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The variable expression.
    /// </summary>
    public class LuaXBinaryOperatorExpression : LuaXExpression
    {
        public LuaXBinaryOperator Operator { get; }
        public LuaXExpression LeftArgument { get; }
        public LuaXExpression RightArgument { get; }

        internal LuaXBinaryOperatorExpression(LuaXBinaryOperator @operator, LuaXExpression left, LuaXExpression right, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            Operator = @operator;
            LeftArgument = left;
            RightArgument = right;
        }

        public override string ToString() => $"({LeftArgument.ToString()} {Operator} {RightArgument.ToString()})";
    }
}
