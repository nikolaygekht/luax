using Luax.Parser.Ast.Statement;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// Definition of a class method
    /// </summary>
    public class LuaXMethod : ILuaXNamedObject
    {
        /// <summary>
        /// Class to which the method belongs to
        /// </summary>
        public LuaXClass Class { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="methodClass"></param>
        public LuaXMethod(LuaXClass methodClass)
        {
            Class = methodClass;
        }

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
        /// The method local constants
        /// </summary>
        public LuaXConstantVariableCollection Constants { get; } = new LuaXConstantVariableCollection();

        /// <summary>
        /// The flag indicating whether the method is a static method
        /// </summary>
        public bool Static { get; internal init; }

        /// <summary>
        /// The method visibility
        /// </summary>
        public LuaXVisibility Visibility { get; internal init; }

        /// <summary>
        /// The method is constructor
        /// </summary>
        public bool IsConstructor { get; internal set; }

        /// <summary>
        /// The location of the element in the source
        /// </summary>
        public LuaXElementLocation Location { get; internal init; }

        /// <summary>
        /// The flag indicating that the method is an extern method
        ///
        /// Extern methods are implemented in target platform natively.
        /// </summary>
        public bool Extern { get; internal init; }

        internal IAstNode Body { get; init; }

        /// <summary>
        /// The method body.
        /// </summary>
        public LuaXStatementCollection Statements { get; } = new LuaXStatementCollection();
    }
}
