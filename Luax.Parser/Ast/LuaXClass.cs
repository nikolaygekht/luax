using System.Collections;
using System.Collections.Generic;
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
        public string Parent { get; internal set; }
        /// <summary>
        /// The flag indicating that the class has a parent class
        /// </summary>
        public bool HasParent => !string.IsNullOrEmpty(Parent);

        /// <summary>
        /// The reference to the parent class
        /// </summary>
        public LuaXClass ParentClass { get; internal set; }

        /// <summary>
        /// The reference to a constructor
        /// </summary>
        public LuaXMethod Constructor { get; internal set; }

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

        private LuaXClassCollection mMetadata;

        internal LuaXClass(string name) : this(name, "object", new LuaXElementLocation("code", new AstNodeWrapper()))
        {
        }

        internal LuaXClass(string name, string parent, LuaXElementLocation location)
        {
            Name = name;
            Parent = parent;
            Location = location;
        }

        internal bool HasInParents(LuaXClass parent)
        {
            string currentClassName = Name;
            string parentName = parent.Name;
            while (!string.IsNullOrEmpty(currentClassName) && currentClassName != "object")
            {
                if (currentClassName.Equals(parentName))
                    return true;
                mMetadata.Search(currentClassName, out var @class);
                if (@class == null)
                {
                    return false;
                }
                currentClassName = @class.Parent;
            }
            return false;
        }

        private void ValidateParentChain(LuaXApplication application)
        {
            HashSet<string> parents = new HashSet<string>();
            string name = Name;
            while (!string.IsNullOrEmpty(name) && name != "object")
            {
                if (parents.Contains(name))
                    throw new LuaXAstGeneratorException(Location, "Class contains itself in the class inheritance chain");
                parents.Add(name);
                application.Classes.Search(name, out var @class);
                name = @class.Parent;
            }
        }

        internal void Pass2(LuaXApplication application, LuaXAstTreeCreator creator)
        {
            ValidateParentChain(application);
            mMetadata = application.Classes;

            //process methods
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
       
        public bool SearchProperty(string propertyName, out LuaXProperty property, out string ownerClassName)
        {
            LuaXPropertyCollection properties = this.Properties;
            string className = this.Name;
            while (!string.IsNullOrEmpty(className))
            {
                var propertyIndex = properties.Find(propertyName);
                if (propertyIndex < 0)
                {
                    bool foundInParents = false;
                    if (ParentClass != null)
                    {
                        foundInParents = ParentClass.SearchProperty(propertyName, out property, out ownerClassName);
                        if (foundInParents)
                        {
                            return true;
                        }
                    }
                    if(!foundInParents)
                    {
                        int indexOfPoint = className.LastIndexOf('.');
                        if (indexOfPoint > 0)
                        {
                            className = className.Substring(0, indexOfPoint);
                            if (mMetadata.Search(className, out var @class))
                            {
                                properties = @class.Properties;
                                continue;
                            }
                        }
                        break;
                    }
                }
                ownerClassName = className;
                property = properties[propertyIndex];
                return true;
            }
            ownerClassName = null;
            property = null;
            return false;
        }

        public bool SearchConstant(string propertyName, out LuaXConstantVariable constant)
        {
            LuaXConstantVariableCollection constants = this.Constants;
            string className = this.Name;
            while (!string.IsNullOrEmpty(className))
            {
                var constantIndex = constants.Find(propertyName);
                if (constantIndex < 0)
                {
                    bool foundInParents = false;
                    if (ParentClass != null)
                    {
                        foundInParents = ParentClass.SearchConstant(propertyName, out constant);
                        if (foundInParents)
                        {
                            return true;
                        }
                    }
                    if (!foundInParents)
                    {
                        int indexOfPoint = className.LastIndexOf('.');
                        if (indexOfPoint > 0)
                        {
                            className = className.Substring(0, indexOfPoint);
                            if (mMetadata.Search(className, out var @class))
                            {
                                constants = @class.Constants;
                                continue;
                            }
                        }
                        break;
                    }
                }
                constant = constants[constantIndex];
                return true;
            }
            constant = null;
            return false;
        }

        public bool SearchMethod(string propertyName, out LuaXMethod method)
        {
            LuaXMethodCollection methods = this.Methods;
            string className = this.Name;
            while (!string.IsNullOrEmpty(className))
            {
                var methodIndex = methods.Find(propertyName);
                if (methodIndex < 0)
                {
                    bool foundInParents = false;
                    if (ParentClass != null)
                    {
                        foundInParents = ParentClass.SearchMethod(propertyName, out method);
                        if (foundInParents)
                        {
                            return true;
                        }
                    }
                    if (!foundInParents)
                    {
                        int indexOfPoint = className.LastIndexOf('.');
                        if (indexOfPoint > 0)
                        {
                            className = className.Substring(0, indexOfPoint);
                            if (mMetadata.Search(className, out var @class))
                            {
                                methods = @class.Methods;
                                continue;
                            }
                        }
                        break;
                    }
                }
                method = methods[methodIndex];
                return true;
            }
            method = null;
            return false;
        }
    }
}
