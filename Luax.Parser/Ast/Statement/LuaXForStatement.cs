using Luax.Parser.Ast.LuaExpression;

/// <summary>
/// FOR statement
/// </summary>
namespace Luax.Parser.Ast.Statement
{
    public class LuaXForStatement : LuaXStatement
    {
        /// <summary>
        /// The loop description
        /// </summary>
        public LuaXForLoopDescription ForLoopDescription { get; }

        /// <summary>
        /// The loop body content
        /// </summary>
        public LuaXStatementCollection Statements { get; } = new LuaXStatementCollection();

        internal LuaXForStatement(LuaXElementLocation location, LuaXForLoopDescription description) : base(location)
        {
            ForLoopDescription = description;
        }
    }
}
