using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    /// <summary>
    /// One clause of an IF statement
    /// </summary>
    public class LuaXIfClause
    {
        /// <summary>
        /// The location of the clause
        /// </summary>
        public LuaXElementLocation Location { get; }
        /// <summary>
        /// The condition when the clause is execute
        /// </summary>
        public LuaXExpression Condition { get; }
        
        /// <summary>
        /// The clause content
        /// </summary>
        public LuaXStatementCollection Statements { get; } = new LuaXStatementCollection();

        internal LuaXIfClause(LuaXExpression condition, LuaXElementLocation location)
        {
            Condition = condition;
            Location = location;
        }
    }
}
