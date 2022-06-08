namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The variable expression.
    /// </summary>
    public class LuaXVariableExpression : LuaXExpression
    {
        public string VariableName { get; }

        public LuaXVariableExpression(string name, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            VariableName = name;
        }

        public override string ToString() => $"var:{VariableName}";
    }
}
