using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    /// <summary>
    /// The base class for all LuaX assignment statement
    /// </summary>

    public abstract class LuaXAssignStatement : LuaXStatement
    {
        public LuaXExpression Expression { get; }

        protected LuaXAssignStatement(LuaXExpression expression, LuaXElementLocation location)
            : base(location)
        {
            Expression = expression;
        }
    }
}
