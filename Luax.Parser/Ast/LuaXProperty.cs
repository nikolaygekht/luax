using System.Dynamic;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// Definition of a class property
    /// </summary>
    public class LuaXProperty : LuaXVariable
    {
        /// <summary>
        /// The flag indicating whether the property is a static property
        /// </summary>
        public bool Static { get; internal init; }

        /// <summary>
        /// The property visibility
        /// </summary>
        public LuaXVisibility Visibility { get; init; }

        /// <summary>
        /// Attributes
        /// </summary>
        public LuaXAttributeCollection Attributes { get; } = new LuaXAttributeCollection();
    }
}
