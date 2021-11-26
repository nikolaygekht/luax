using System.Text;

namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The new array expression with initialization
    /// </summary>
    public class LuaXNewArrayWithInitExpression : LuaXExpression
    {
        /// <summary>
        /// The type of the array's elements
        /// </summary>
        public LuaXTypeDefinition ElementType { get; }

        /// <summary>
        /// The initialization expressions
        /// </summary>
        public LuaXExpressionCollection InitExpressions { get; }

        internal LuaXNewArrayWithInitExpression(LuaXTypeDefinition elementType, LuaXExpressionCollection initExpressions, LuaXElementLocation location)
            : base(elementType.ArrayOf(), location)
        {
            ElementType = elementType;
            InitExpressions = initExpressions;
        }

        public override string ToString() => $"(new {ElementType}[]{{{InitExpressions.ToString()}}})";
    }
}
