namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The new array expression
    /// </summary>
    public class LuaXNewArrayExpression : LuaXExpression
    {
        /// <summary>
        /// The type of the array's elements
        /// </summary>
        public LuaXTypeDefinition ElementType { get; }

        /// <summary>
        /// The size expression
        /// </summary>
        public LuaXExpression SizeExpression { get; }

        internal LuaXNewArrayExpression(LuaXTypeDefinition elementType, LuaXExpression size, LuaXElementLocation location)
            : base(elementType.ArrayOf(), location)
        {
            ElementType = elementType;
            SizeExpression = size;
        }

        public override string ToString() => $"(new {ElementType}[{SizeExpression.ToString()}])";
    }
}
