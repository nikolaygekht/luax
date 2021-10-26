namespace Luax.Parser.Ast
{
    /// <summary>
    /// Definition of a class method
    /// </summary>
    public class LuaXMethod
    {
        /// <summary>
        /// The name of the method
        /// </summary>
        public string Name { get; internal init; }

        /// <summary>
        /// The return type of the method
        /// </summary>
        public LuaXTypeDefinition ReturnType { get; internal init; }

        /// <summary>
        /// The attributes associated with the method
        /// </summary>
        public LuaXAttributeCollection Attributes { get; } = new LuaXAttributeCollection();

        /// <summary>
        /// The method arguments
        /// </summary>
        public LuaXVariableCollection Arguments { get; } = new LuaXVariableCollection();

        /// <summary>
        /// The method local variables
        /// </summary>
        public LuaXVariableCollection Variables { get; } = new LuaXVariableCollection();

        /// <summary>
        /// The flag indicating whether the method is a static method
        /// </summary>
        public bool Static { get; internal init; }

        /// <summary>
        /// The method visibility
        /// </summary>
        public LuaXVisibility Visibility { get; internal init; }

        /// <summary>
        /// The location of the element in the source
        /// </summary>
        public LuaXElementLocation Location { get; internal init; }
    }
}
