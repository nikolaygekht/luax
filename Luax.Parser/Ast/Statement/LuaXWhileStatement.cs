using Luax.Parser.Ast.LuaExpression;


/// <summary>
/// WHILE statement
/// </summary>
namespace Luax.Parser.Ast.Statement
{
    public class LuaXWhileStatement : LuaXStatement
    {
        /// <summary>
        /// The condition when the loop body is execute
        /// </summary>
        public LuaXExpression WhileCondition { get; }

        /// <summary>
        /// The loop body content
        /// </summary>
        public LuaXStatementCollection Statements { get; } = new LuaXStatementCollection();

        internal LuaXWhileStatement(LuaXElementLocation location, LuaXExpression whileCondition) : base(location)
        {
            WhileCondition = whileCondition;
        }
    }
}
