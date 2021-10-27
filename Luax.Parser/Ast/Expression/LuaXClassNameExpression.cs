namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The constant expression.
    /// </summary>
    public class LuaXClassNameExpression : LuaXExpression
    {
        public string Name { get; }

        internal LuaXClassNameExpression(string name, LuaXElementLocation location)
            : base(new LuaXTypeDefinition() { TypeId = LuaXType.ClassName, Class = name }, location)
        {
            Name = name;
        }
    }
}
