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
    public class LuaXClass : ILuaXNamedObject
    {
        public static LuaXClass Object { get; } = new LuaXClass("object", null, new LuaXElementLocation("internal", new AstNodeWrapper()));

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
        /// The reference to the parent class
        /// </summary>
        public LuaXClass ParentClass { get; internal set; }

        public LuaXTypeDefinition TypeOf() => new LuaXTypeDefinition() { TypeId = LuaXType.Object, Class = Name };

        /// <summary>
        /// The collection of class attributes
        /// </summary>
        public LuaXAttributeCollection Attributes { get; } = new LuaXAttributeCollection();

        /// <summary>
        /// The collection of class properties
        /// </summary>
        public LuaXPropertyCollection Properties { get; } = new LuaXPropertyCollection();

        /// <summary>
        /// The collection of class properties
        /// </summary>
        public LuaXConstantVariableCollection Constants { get; } = new LuaXConstantVariableCollection();

        /// <summary>
        /// The collection of the class methods
        /// </summary>
        public LuaXMethodCollection Methods { get; } = new LuaXMethodCollection();

        /// <summary>
        /// The location of the element in the source
        /// </summary>
        public LuaXElementLocation Location { get; }

        internal LuaXClass(string name) : this(name, "object", new LuaXElementLocation("code", new AstNodeWrapper()))
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
                if (method.ReturnType.TypeId == LuaXType.Object && !creator.Metadata.Exists(method.ReturnType.Class))
                    throw new LuaXAstGeneratorException(method.Location.Source, new LuaXParserError(method.Location, $"Return type {method.ReturnType.Class} is not defined"));

                for (int j = 0; j < method.Arguments.Count; j++)
                {
                    var arg = method.Arguments[j];
                    if (arg.LuaType.TypeId == LuaXType.Object && !creator.Metadata.Exists(arg.LuaType.Class))
                        throw new LuaXAstGeneratorException(arg.Location.Source, new LuaXParserError(arg.Location, $"Argument type {arg.LuaType.Class} is not defined"));
                }

                if (method.Body != null)
                    creator.ProcessBody(method.Body, this, Methods[i]);
            }
        }

        public bool SearchProperty(string propertyName, out LuaXProperty property)
        {
            var propertyIndex = Properties.Find(propertyName);
            if (propertyIndex < 0)
            {
                if (ParentClass == null)
                {
                    property = null;
                    return false;
                }
                return ParentClass.SearchProperty(propertyName, out property);
            }
            property = this.Properties[propertyIndex];
            return true;
        }

        public bool SearchConstant(string propertyName, out LuaXConstantVariable constant)
        {
            var constantIndex = Constants.Find(propertyName);
            if (constantIndex < 0)
            {
                if (ParentClass == null)
                {
                    constant = null;
                    return false;
                }
                return ParentClass.SearchConstant(propertyName, out constant);
            }
            constant = this.Constants[constantIndex];
            return true;
        }

        public bool SearchMethod(string propertyName, out LuaXMethod method)
        {
            var methodIndex = Methods.Find(propertyName);
            if (methodIndex < 0)
            {
                if (ParentClass == null)
                {
                    method = null;
                    return false;
                }
                return ParentClass.SearchMethod(propertyName, out method);
            }
            method = this.Methods[methodIndex];
            return true;
        }
    }
}
