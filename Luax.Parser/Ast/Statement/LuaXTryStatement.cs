using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    public class LuaXTryStatement : LuaXStatement
    {
        /// <summary>
        /// Try block body
        /// </summary>
        public LuaXStatementCollection TryStatements { get; } = new LuaXStatementCollection();

        /// <summary>
        /// Catch statement
        /// </summary>
        public LuaXCatchClause CatchStatement;

        public LuaXTryStatement(LuaXElementLocation location)
            : base(location)
        {
        }
    }
}
