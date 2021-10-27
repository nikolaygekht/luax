﻿namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The unary operator.
    /// </summary>
    public class LuaXUnaryOperatorExpression : LuaXExpression
    {
        public LuaXUnaryOperator Operator { get; }
        public LuaXExpression Argument { get; }

        internal LuaXUnaryOperatorExpression(LuaXUnaryOperator @operator, LuaXExpression argument, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            Operator = @operator;
            Argument = argument;
        }
    }
}
