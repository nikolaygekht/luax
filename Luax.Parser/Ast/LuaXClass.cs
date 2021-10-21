using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// LuaX class declaration
    /// </summary>
    public class LuaXClass
    {
        /// <summary>
        /// The class name
        /// </summary>
        public string Name { get; internal init; }
        /// <summary>
        /// The parent class
        /// </summary>
        public string Parent { get; internal init; }
        /// <summary>
        /// The flag indicating that the class has a parent class
        /// </summary>
        public bool HasParent => !string.IsNullOrEmpty(Parent);

        /// <summary>
        /// The collection of class attributes
        /// </summary>
        public LuaXAttributeCollection Attributes { get; } = new LuaXAttributeCollection();

        /// <summary>
        /// The collection of class properties
        /// </summary>
        public LuaXPropertyCollection Properties { get; } = new LuaXPropertyCollection();

        /// <summary>
        /// The collection of the class methods
        /// </summary>
        public LuaXMethodCollection Methods { get; } = new LuaXMethodCollection();

        internal LuaXClass(string name) : this(name, null)
        {
        }

        internal LuaXClass(string name, string parent)
        {
            Name = name;
            Parent = parent;
        }
    }
}
