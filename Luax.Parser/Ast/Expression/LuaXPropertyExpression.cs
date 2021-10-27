namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The constant expression.
    /// </summary>
    public class LuaXPropertyExpression : LuaXExpression
    {
        public LuaXExpression Object { get; }
        public string PropertyName { get; }

        internal LuaXPropertyExpression(LuaXExpression @object, string propertyName, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            Object = @object;
            PropertyName = propertyName;
        }
    }
}
