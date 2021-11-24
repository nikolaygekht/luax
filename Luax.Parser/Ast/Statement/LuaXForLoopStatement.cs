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

        public LuaXExpression Limit { get; }

        public LuaXExpression Iterator { get; }

        public LuaXVariable Variable { get; }

        internal LuaXForLoopStatement(LuaXVariable identierVar, LuaXExpressionCollection expressions, LuaXElementLocation location) : base(location)
        {
            Variable = identierVar;
            Start = expressions[0];
            Limit = expressions[1];
            if (expressions.Count == 3)
                Iterator = expressions[2];
            else
                Iterator = new LuaXConstantExpression(new LuaXConstant(1, location));
        }
    }
}
