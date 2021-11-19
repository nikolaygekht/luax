using Luax.Parser.Ast.LuaExpression;

/// <summary>
/// FOR statement
/// </summary>
namespace Luax.Parser.Ast.Statement
{
    public class LuaXForStatement : LuaXStatement
    {
        /// <summary>
        /// The statement when the loop body is execute
        /// </summary>
        public LuaXForLoopStatement ForLoopStatement { get; }

        /// <summary>
        /// The loop body content
        /// </summary>
        public LuaXStatementCollection Statements { get; } = new LuaXStatementCollection();

        internal LuaXForStatement(LuaXElementLocation location, LuaXForLoopStatement forloopStatement) : base(location)
        {
            ForLoopStatement = forloopStatement;
        }
    }
}
