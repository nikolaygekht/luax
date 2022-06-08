namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The constant expression.
    /// </summary>
    public class LuaXConstantExpression : LuaXExpression
    {
        public LuaXConstant Value { get; }

        public LuaXConstantExpression(LuaXConstant value, LuaXElementLocation location)
            : base(new LuaXTypeDefinition() { TypeId = value.ConstantType }, location)
        {
            Value = value;
        }

        public LuaXConstantExpression(LuaXConstant value)
            : this(value, value.Location)
        {
        }

        public override string ToString() => $"const:{Value.ConstantTypeFull}:{Value.Value}";
    }
}
