using Luax.Parser.Ast.LuaExpression;
using System;

/// <summary>
/// FOR LOOP statement
/// </summary>
namespace Luax.Parser.Ast.Statement
{
    public class LuaXForLoopStatement : LuaXStatement
    {
        private LuaXExpressionCollection forLoopExpressions;
        private LuaXElementLocation luaXElementLocation;

        public LuaXExpression Start { get; }

        public LuaXExpression Condition { get; }

        public LuaXExpression Iterator { get; }

        public string VariableName { get; }

        internal LuaXForLoopStatement(LuaXVariable identierVar, LuaXExpressionCollection expressions, LuaXElementLocation location) : base(location)
        {
            VariableName = identierVar.Name;
            Start = expressions[0];
            if (expressions.Count == 3)
                Iterator = expressions[2];
            else
                Iterator = new LuaXConstantExpression(new LuaXConstant(1, location));

            var operation = SelectConditionOperation(Iterator);

            Condition = new LuaXBinaryOperatorExpression(operation,
                new LuaXVariableExpression(identierVar.Name, identierVar.LuaType, identierVar.Location),
                expressions[1], LuaXTypeDefinition.Boolean, expressions[1].Location);
        }

        private static LuaXBinaryOperator SelectConditionOperation(LuaXExpression expression)
        {
            if (expression is LuaXConstantExpression c && !c.Value.IsNil && c.Value.AsInteger() < 0)
                return LuaXBinaryOperator.GreaterOrEqual;
            else
                return LuaXBinaryOperator.LessOrEqual;
        }
    }
}
