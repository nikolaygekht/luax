namespace Luax.Parser.Ast
{
    public class LuaXConstantVariable : ILuaXNamedObject
    {
        /// <summary>
        /// Attributes
        /// </summary>
        public LuaXAttributeCollection Attributes { get; } = new LuaXAttributeCollection();

        /// <summary>
        /// Constant name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initial value of the variable
        /// </summary>
        public LuaXConstant Value { get; set; }
    }
}
