using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    /// <summary>
    /// Assign a static property statement
    /// </summary>
    public class LuaXAssignStaticPropertyStatement : LuaXAssignStatement
    {
        /// <summary>
        /// The name of the class
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// The name of the property
        /// </summary>
        public string PropertyName { get; }

        internal LuaXAssignStaticPropertyStatement(string className, string propertyName, LuaXExpression expression, LuaXElementLocation location)
            : base(expression, location)
        {
            ClassName = className;
            PropertyName = propertyName;
        }
    }
}
