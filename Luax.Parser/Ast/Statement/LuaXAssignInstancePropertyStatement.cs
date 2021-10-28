using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    /// <summary>
    /// Assign an instance property statement
    /// </summary>
    public class LuaXAssignInstancePropertyStatement : LuaXAssignStatement
    {
        /// <summary>
        /// The expression to access the object
        /// </summary>
        public LuaXExpression Object { get; }

        /// <summary>
        /// The name of the property
        /// </summary>
        public string PropertyName { get; }

        internal LuaXAssignInstancePropertyStatement(LuaXExpression @object, string propertyName, LuaXExpression expression, LuaXElementLocation location)
            : base(expression, location)
        {
            Object = @object;
            PropertyName = propertyName;
        }
    }
}
