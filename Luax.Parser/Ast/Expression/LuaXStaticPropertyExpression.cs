namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The constant expression.
    /// </summary>
    public class LuaXStaticPropertyExpression : LuaXExpression
    {
        public string ClassName { get; }
        public string PropertyName { get; }

        public LuaXStaticPropertyExpression(string className, string propertyName, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            ClassName = className;
            PropertyName = propertyName;
        }

        public override string ToString() => $"property:{ClassName}::{PropertyName}";
    }
}
