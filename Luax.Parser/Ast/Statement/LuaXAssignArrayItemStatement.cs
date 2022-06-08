using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    /// <summary>
    /// Assign an instance property statement
    /// </summary>
    public class LuaXAssignArrayItemStatement : LuaXAssignStatement
    {
        /// <summary>
        /// The expression to access the object
        /// </summary>
        public LuaXExpression Array { get; }

        /// <summary>
        /// The index expression
        /// </summary>
        public LuaXExpression Index { get; }

        public LuaXAssignArrayItemStatement(LuaXExpression array, LuaXExpression index, LuaXExpression expression, LuaXElementLocation location)
            : base(expression, location)
        {
            Array = array;
            Index = index;
        }
    }
}
