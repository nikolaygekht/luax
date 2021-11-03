namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The constant expression.
    /// </summary>
    public class LuaXConstantExpression : LuaXExpression
    {
        public LuaXConstant Value { get; }

        internal LuaXConstantExpression(LuaXConstant value, LuaXElementLocation location)
            : base(new LuaXTypeDefinition() { TypeId = value.ConstantType }, location)
        {
            Value = value;
        }

        internal LuaXConstantExpression(LuaXConstant value)
            : this(value, value.Location)
        {
        }

        public override string ToString() => $"const:{Value.ConstantTypeFull}:{Value.Value}";
    }
}
