using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast.Builder;

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

        /// <summary>
        /// The location of the element in the source
        /// </summary>
        public LuaXElementLocation Location { get; }

        internal LuaXClass(string name, LuaXElementLocation location) : this(name, null, location)
        {
        }

        internal LuaXClass(string name, string parent, LuaXElementLocation location)
        {
            Name = name;
            Parent = parent;
            Location = location;
        }

        internal void Pass2(LuaXAstTreeCreator creator)
        {
            for (int i = 0; i < Methods.Count; i++)
            {
                var method = Methods[i];
                if (method.ReturnType.TypeId == LuaXType.Object && creator.Metadata.Find(method.ReturnType.Class) < 0)
                    throw new LuaXAstGeneratorException(method.Location.Source, new LuaXParserError(method.Location, $"Return type {method.ReturnType.Class} is not defined"));
                for (int j = 0; j < method.Arguments.Count; j++)
                {
                    var arg = method.Arguments[j];
                    if (arg.LuaType.TypeId == LuaXType.Object && creator.Metadata.Find(arg.LuaType.Class) < 0)
                        throw new LuaXAstGeneratorException(arg.Location.Source, new LuaXParserError(arg.Location, $"Argument type {arg.LuaType.Class} is not defined"));
                }

                if (method.Body != null)
                    creator.ProcessBody(method.Body, Methods[i]);
            }
        }
    }
}
