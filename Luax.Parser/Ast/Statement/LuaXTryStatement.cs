using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    public class LuaXTryStatement : LuaXStatement
    {
        /// <summary>
        /// Try block body
        /// </summary>
        public LuaXStatementCollection TryStatements { get; }

        /// <summary>
        /// Catch statement
        /// </summary>
        public LuaXCatchClause CatchClause { get; }

        public LuaXTryStatement(LuaXElementLocation location, LuaXCatchClause catchClause, LuaXStatementCollection tryStatements)
            : base(location)
        {
            TryStatements = tryStatements;
            CatchClause = catchClause;
        }
    }
}
