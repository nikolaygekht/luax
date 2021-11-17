using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    public class LuaXThrowStatement : LuaXStatement
    {
        /// <summary>
        /// The expression to throw
        /// </summary>
        public LuaXExpression ThrowExpression { get; }

        public LuaXThrowStatement(LuaXExpression throwExpression, LuaXElementLocation location)
            : base(location)
        {
            ThrowExpression = throwExpression;
        }
    }
}
