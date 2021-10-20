namespace Luax.Parser.Ast
{
    /// <summary>
    /// The LuaX attribute
    /// </summary>
    public class LuaXAttribute
    {
        /// <summary>
        /// The name of the attribute
        /// </summary>
        public string Name { get; internal init; }

        /// <summary>
        /// The collection of the parameters
        /// </summary>
        public LuaXConstantCollection Parameters { get; } = new LuaXConstantCollection();

        internal LuaXAttribute(string name)
        {
            Name = name;
        }
    }
}
