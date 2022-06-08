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

        public bool IsMath => Operator == LuaXBinaryOperator.Add || Operator == LuaXBinaryOperator.Subtract ||
            Operator == LuaXBinaryOperator.Multiply || Operator == LuaXBinaryOperator.Divide || Operator == LuaXBinaryOperator.Reminder ||
            Operator == LuaXBinaryOperator.Power;

        public bool IsRelational => Operator == LuaXBinaryOperator.Equal || Operator == LuaXBinaryOperator.NotEqual ||
            Operator == LuaXBinaryOperator.Less || Operator == LuaXBinaryOperator.LessOrEqual ||
            Operator == LuaXBinaryOperator.Greater || Operator == LuaXBinaryOperator.GreaterOrEqual;

        public bool IsLogical => Operator == LuaXBinaryOperator.And || Operator == LuaXBinaryOperator.Or;

        public LuaXBinaryOperatorExpression(LuaXBinaryOperator @operator, LuaXExpression left, LuaXExpression right, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            Operator = @operator;
            LeftArgument = left;
            RightArgument = right;
        }

        public override string ToString() => $"({LeftArgument.ToString()} {Operator} {RightArgument.ToString()})";
    }
}
