using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Statement
{
    /// <summary>
    /// Assign a local variable statement
    /// </summary>
    public class LuaXAssignVariableStatement : LuaXAssignStatement
    {
        /// <summary>
        /// The name of the variable to assign
        /// </summary>
        public string VariableName { get; }

        public LuaXAssignVariableStatement(string variableName, LuaXExpression expression, LuaXElementLocation location)
            : base(expression, location)
        {
            VariableName = variableName;
        }
    }
}
