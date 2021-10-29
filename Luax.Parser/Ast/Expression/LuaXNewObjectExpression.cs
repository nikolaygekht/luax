namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The new object expression
    /// </summary>
    public class LuaXNewObjectExpression : LuaXExpression
    {
        /// <summary>
        /// The class name to create
        /// </summary>
        public string ClassName { get; }

        internal LuaXNewObjectExpression(string @class, LuaXElementLocation location)
            : base(new LuaXTypeDefinition() { TypeId = LuaXType.Object, Class = @class }, location)
        {
            ClassName = @class;
        }

        public override string ToString() => $"(new {ClassName})";
    }
}
