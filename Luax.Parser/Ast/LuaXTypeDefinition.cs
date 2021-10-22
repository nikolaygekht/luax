namespace Luax.Parser.Ast
{
    /// <summary>
    /// The definition of the Lua type
    /// </summary>
    public class LuaXTypeDefinition
    {
        /// <summary>
        /// The lua type.
        /// </summary>
        public LuaXType TypeId { get; internal set; }

        /// <summary>
        /// The flag indicating whether the variable is an array
        /// </summary>
        public bool Array { get; internal set; }
        /// <summary>
        /// The class name if LuaType is a class
        /// </summary>
        public string Class { get; internal set; }
    }
}
