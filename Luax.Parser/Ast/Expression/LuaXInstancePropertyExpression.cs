namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The constant expression.
    /// </summary>
    public class LuaXInstancePropertyExpression : LuaXExpression
    {
        public LuaXExpression Object { get; }
        public string PropertyName { get; }

        internal LuaXInstancePropertyExpression(LuaXExpression @object, string propertyName, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            Object = @object;
            PropertyName = propertyName;
        }

        public override string ToString() => $"property:({Object.ToString()}).{PropertyName}";
    }
}
