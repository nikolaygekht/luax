namespace Luax.Parser.Ast.LuaExpression
{
    public class LuaXTypeNameOperatorExpression : LuaXExpression
    {
        public LuaXExpression Argument { get; }

        public LuaXTypeNameOperatorExpression(LuaXExpression argument, LuaXElementLocation location)
            : base(LuaXTypeDefinition.String, location)
        {
            Argument = argument;
        }

        public override string ToString() => $"typename({Argument.ToString()})";
    }
}
