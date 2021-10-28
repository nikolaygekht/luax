namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The argument expression.
    /// </summary>
    public class LuaXArgumentExpression : LuaXExpression
    {
        public string Name { get; }

        internal LuaXArgumentExpression(string name, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            Name = name;
        }

        public override string ToString()
            => $"arg:{Name}";
    }
}
