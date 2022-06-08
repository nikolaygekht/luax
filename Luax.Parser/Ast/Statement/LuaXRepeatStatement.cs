using Luax.Parser.Ast.LuaExpression;

/// <summary>
/// WHILE statement
/// </summary>
namespace Luax.Parser.Ast.Statement
{
    public class LuaXRepeatStatement : LuaXStatement
    {
        /// <summary>
        /// The condition when the loop body is execute
        /// </summary>
        public LuaXExpression UntilCondition { get; }

        /// <summary>
        /// The loop body content
        /// </summary>
        public LuaXStatementCollection Statements { get; } = new LuaXStatementCollection();

        public LuaXRepeatStatement(LuaXElementLocation location, LuaXExpression untilCondition) : base(location)
        {
            UntilCondition = untilCondition;
        }
    }
}
