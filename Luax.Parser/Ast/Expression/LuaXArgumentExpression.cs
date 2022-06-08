namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The argument expression.
    /// </summary>
    public class LuaXArgumentExpression : LuaXExpression
    {
        public string ArgumentName { get; }

        public LuaXArgumentExpression(string name, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            ArgumentName = name;
        }

        public override string ToString()
            => $"arg:{ArgumentName}";
    }
}
