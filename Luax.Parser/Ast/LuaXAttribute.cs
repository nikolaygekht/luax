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
        /// The location of the element in the source
        /// </summary>
        public LuaXElementLocation Location { get; }

        /// <summary>
        /// The collection of the parameters
        /// </summary>
        public LuaXConstantCollection Parameters { get; } = new LuaXConstantCollection();

        public LuaXAttribute(string name, LuaXElementLocation location)
        {
            Name = name;
            Location = location;
        }
    }
}
