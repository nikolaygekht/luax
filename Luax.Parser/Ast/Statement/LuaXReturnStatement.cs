using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    /// <summary>
    /// The return statement
    /// </summary>
    public class LuaXReturnStatement : LuaXStatement
    {
        public bool HasExpression => Expression != null;

        public LuaXExpression Expression { get; }

        public LuaXReturnStatement(LuaXExpression expression, LuaXElementLocation location)
            : base(location)
        {
            Expression = expression;
        }
    }
}
