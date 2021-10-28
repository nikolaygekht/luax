namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The cast expression.
    /// </summary>
    public class LuaXCastOperatorExpression : LuaXExpression
    {
        public LuaXExpression Argument { get; }

        internal LuaXCastOperatorExpression(LuaXExpression argument, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            Argument = argument;
        }

        public override string ToString() => $"cast<{ReturnType}>({Argument.ToString()})";
    }
}
