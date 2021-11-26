using Luax.Parser.Ast.LuaExpression;
using System;

/// <summary>
/// FOR LOOP statement
/// </summary>
namespace Luax.Parser.Ast.Statement
{
    public class LuaXForLoopDescription
    {
        public LuaXExpression Start { get; }

        public LuaXExpression Limit { get; }

        public LuaXExpression Step { get; }

        public LuaXVariable Variable { get; }

        internal LuaXForLoopDescription(LuaXVariable identierVar, LuaXExpression start,
            LuaXExpression limit, LuaXExpression step)
        {
            Variable = identierVar;
            Start = start;
            Limit = limit;
            Step = step;
        }
    }
}
