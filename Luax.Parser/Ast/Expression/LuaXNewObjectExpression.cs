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

        public LuaXNewObjectExpression(string @class, LuaXElementLocation location)
            : base(new LuaXTypeDefinition() { TypeId = LuaXType.Object, Class = @class }, location)
        {
            ClassName = @class;
        }

        public override string ToString() => $"(new {ClassName})";
    }
}
