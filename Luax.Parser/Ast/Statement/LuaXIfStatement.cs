namespace Luax.Parser.Ast.Statement
{
    /// <summary>
    /// IF statement
    /// </summary>
    public class LuaXIfStatement : LuaXStatement
    {
        /// <summary>
        /// The list of clauses
        /// </summary>
        public LuaXIfClauseCollection Clauses { get; } = new LuaXIfClauseCollection();

        /// <summary>
        /// The clause content
        /// </summary>
        public LuaXStatementCollection ElseClause { get; } = new LuaXStatementCollection();

        public LuaXIfStatement(LuaXElementLocation location) : base(location)
        {
        }
    }
}
