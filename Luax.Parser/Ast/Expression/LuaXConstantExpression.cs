namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The constant expression.
    /// </summary>
    public class LuaXConstantExpression : LuaXExpression
    {
        public LuaXConstant Value { get; }

        /// <summary>
        /// The source of the constant
        /// </summary>
        public class LuaXConstantSource
        {
            /// <summary>
            /// The class to which the constant belong
            /// </summary>
            public LuaXClass Class { get; }
            /// <summary>
            /// The constant definition
            /// </summary>
            public LuaXConstantVariable Constant { get; }
            /// <summary>
            /// The method in which the constant is defined. 
            /// 
            /// If the constant is defined at the class scope, the value will be `null`
            /// </summary>
            public LuaXMethod Method { get; }

            internal LuaXConstantSource(LuaXClass @class, LuaXMethod @method, LuaXConstantVariable @constant)
            {
                Class = @class;
                Method = @method;
                Constant = @constant;
            }
        }

        public LuaXConstantSource Source { get; }

        public LuaXConstantExpression(LuaXConstant value, LuaXElementLocation location, LuaXConstantSource source = null)
            : base(new LuaXTypeDefinition() { TypeId = value.ConstantType }, location)
        {
            Value = value;
            Source = source;
        }

        public LuaXConstantExpression(LuaXConstant value)
            : this(value, value.Location)
        {
        }

        public override string ToString() => $"const:{Value.ConstantTypeFull}:{Value.Value}";
    }
}
