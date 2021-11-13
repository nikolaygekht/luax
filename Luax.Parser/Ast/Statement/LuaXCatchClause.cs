using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    public class LuaXCatchClause
    {
        /// <summary>
        /// The location of the clause
        /// </summary>
        public LuaXElementLocation Location { get; }

        /// <summary>
        /// The identifier to catch
        /// </summary>
        public string CatchIdentifier { get; }

        /// <summary>
        /// The content of catch clause
        /// </summary>
        public LuaXStatementCollection CatchStatements = new LuaXStatementCollection();

        public LuaXCatchClause(string catchIdentifier, LuaXElementLocation location)
        {
            CatchIdentifier = catchIdentifier;
            Location = location;
        }
    }
}
