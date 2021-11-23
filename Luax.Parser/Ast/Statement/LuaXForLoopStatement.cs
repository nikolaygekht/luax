using Luax.Parser.Ast.LuaExpression;
using System;

/// <summary>
/// FOR LOOP statement
/// </summary>
namespace Luax.Parser.Ast.Statement
{
    public class LuaXForLoopStatement : LuaXStatement
    {
        public LuaXExpression Start { get; }

        public LuaXExpression Condition { get; private set; }

        public LuaXExpression Iterator { get; }

        public LuaXVariable Variable { get; }

        public bool NeedDetectConditionAtRuntime { get; private set; }

        internal LuaXForLoopStatement(LuaXVariable identierVar, LuaXExpressionCollection expressions, LuaXElementLocation location) : base(location)
        {
            Variable = identierVar;
            Start = expressions[0];
            Condition = expressions[1];
            if (expressions.Count == 3)
                Iterator = expressions[2];
            else
                Iterator = new LuaXConstantExpression(new LuaXConstant(1, location));

            TransformCondition(identierVar);
        }

        private void TransformCondition(LuaXVariable identierVar)
        {
            if (Iterator is LuaXConstantExpression it && !it.Value.IsNil)
            {
                var operation = it.Value.AsInteger() >= 0 ? LuaXBinaryOperator.LessOrEqual : LuaXBinaryOperator.GreaterOrEqual;

                Condition = new LuaXBinaryOperatorExpression(operation,
                                new LuaXVariableExpression(identierVar.Name, identierVar.LuaType, identierVar.Location),
                                Condition, LuaXTypeDefinition.Boolean, Condition.Location);
                NeedDetectConditionAtRuntime = false;
            }
            else
                NeedDetectConditionAtRuntime = true;
        }
    }
}
