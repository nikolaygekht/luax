namespace Luax.Parser.Ast
{
    public class LuaXPackage : ILuaXNamedObject
    {
        /// <summary>
        /// The package name
        /// </summary>
        public string Name { get; internal init; }
        /// <summary>
        /// The collection of class attributes
        /// </summary>
        public LuaXAttributeCollection Attributes { get; } = new LuaXAttributeCollection();
        /// <summary>
        /// The location of the element in the source
        /// </summary>
        public LuaXElementLocation Location { get; }

        public LuaXPackage(string name, LuaXElementLocation location)
        {
            Name = name;
            Location = location;
        }
    }
}
