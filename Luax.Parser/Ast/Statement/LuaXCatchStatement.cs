using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    public class LuaXCatchStatement : LuaXStatement
    {
        /// <summary>
        /// The expression to catch
        /// </summary>
        public string CatchIdentifier { get; }

        /// <summary>
        /// The content of catch clause
        /// </summary>
        public LuaXStatementCollection CatchStatements = new LuaXStatementCollection();

        public LuaXCatchStatement(string catchIdentifier, LuaXElementLocation location)
            : base(location)
        {
            CatchIdentifier = catchIdentifier;
        }
    }
}
