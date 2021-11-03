namespace Luax.Parser.Ast
{
    public class LuaXConstantVariable : ILuaXNamedObject
    {
        /// <summary>
        /// Constant name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Initial value of the variable
        /// </summary>
        public LuaXConstant Value { get; internal set; }
    }
}
