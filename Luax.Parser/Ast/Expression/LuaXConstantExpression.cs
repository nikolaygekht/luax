namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The constant expression.
    /// </summary>
    public class LuaXConstantExpression : LuaXExpression
    {
        public LuaXConstant Value { get; }

        internal LuaXConstantExpression(LuaXConstant value)
            : base(new LuaXTypeDefinition() { TypeId = value.ConstantType }, value.Location)
        {
            Value = value;
        }

        public override string ToString() => $"const:{Value.ConstantTypeFull}:{Value.Value}";
    }
}
