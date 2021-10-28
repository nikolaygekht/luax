using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    public class LuaXCallStatement : LuaXStatement
    {
        /// <summary>
        /// The expression to call
        /// </summary>
        public LuaXExpression CallExpression { get; }

        public LuaXCallStatement(LuaXExpression callExpression, LuaXElementLocation location)
            : base(location)
        {
            CallExpression = callExpression;
        }
    }
}
