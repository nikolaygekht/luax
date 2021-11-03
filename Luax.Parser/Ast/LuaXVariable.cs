using System.Dynamic;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// Definition of a variable or property
    /// </summary>
    public class LuaXVariable : ILuaXNamedObject
    {
        /// <summary>
        /// The name of the variable
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The type definition
        /// </summary>
        public LuaXTypeDefinition LuaType { get; internal set; }

        /// <summary>
        /// The location of the element in the source
        /// </summary>
        public LuaXElementLocation Location { get; internal set; }
    }
}
