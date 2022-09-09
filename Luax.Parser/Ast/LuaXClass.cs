using Luax.Parser.Ast.Builder;
using System.Collections.Generic;

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
        public LuaXClass ParentClass { get; set; }

        /// <summary>
        /// The reference to a constructor
        /// </summary>
        public LuaXMethod Constructor { get; set; }

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

        public LuaXClass(string name) : this(name, "object", new LuaXElementLocation("code", new AstNodeWrapper()))
        {
        }

        public LuaXClass(string name, string parent, LuaXElementLocation location)
        {
            Name = name;
            Parent = parent;
            Location = location;
        }

        internal bool HasInOwners(LuaXClass @class)
        {
            return Name.StartsWith($"{@class.Name}.");
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
                if (@class == null)
                    throw new LuaXAstGeneratorException(Location, $"Parent class {name} is not found in metadata");
                name = @class.Parent;
            }
        }

        internal void Pass2(LuaXApplication application, LuaXAstTreeCreator creator)
        {
            ValidateParentChain(application);
            mMetadata = application.Classes;
            PropertiesPass2(creator);
            MethodsPass2(creator);
        }

        private void MethodsPass2(LuaXAstTreeCreator creator)
        {
            for (int i = 0; i < Methods.Count; i++)
            {
                var method = Methods[i];

                if (Properties.Find(method.Name) >= 0)
                    throw new LuaXAstGeneratorException(method.Location.Source, new LuaXParserError(method.Location, $"The class already have property/method with same name"));

                if (method.ReturnType.TypeId == LuaXType.Object && !creator.Metadata.Exists(method.ReturnType.Class))
                    throw new LuaXAstGeneratorException(method.Location.Source, new LuaXParserError(method.Location, $"Return type {method.ReturnType.Class} is not defined"));

                for (int j = 0; j < method.Arguments.Count; j++)
                {
                    var arg = method.Arguments[j];
                    if (arg.LuaType.TypeId == LuaXType.Object && !creator.Metadata.Exists(arg.LuaType.Class))
                        throw new LuaXAstGeneratorException(arg.Location.Source, new LuaXParserError(arg.Location, $"Argument type {arg.LuaType.Class} is not defined"));
                }

                ValidateOverrideSignatgure(method);

                if (method.Body != null)
                    creator.ProcessBody(method.Body, this, Methods[i]);
            }
        }

        private void PropertiesPass2(LuaXAstTreeCreator creator)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                var property = Properties[i];

                if (Methods.Find(property.Name) >= 0)
                    throw new LuaXAstGeneratorException(property.Location.Source, new LuaXParserError(property.Location, $"The class already have property/method with same name"));

                if (property.LuaType.TypeId == LuaXType.Object && !creator.Metadata.Exists(property.LuaType.Class))
                    throw new LuaXAstGeneratorException(property.Location.Source, new LuaXParserError(property.Location, $"Property type {property.LuaType.Class} is not defined"));
            }
        }

        private void ValidateOverrideSignatgure(LuaXMethod method)
        {
            if (ParentClass == null || Parent == "object")
                return;

            if (!ParentClass.SearchMethod(method.Name, out var baseMethod))
                return;

            if (method.Arguments.Count != baseMethod.Arguments.Count)
                throw new LuaXAstGeneratorException(method.Location, $"Method {Name}.{method.Name} is overridden, but has different number of parameters");

            for (int i = 0; i < method.Arguments.Count; i++)
                if (!AreOverrideArgumentsCompatible(method.Arguments[i].LuaType, baseMethod.Arguments[i].LuaType))
                    throw new LuaXAstGeneratorException(method.Location, $"Method {Name}.{method.Name} is overridden, but argument {i + 1} has incompatible type");
        }

        private bool AreOverrideArgumentsCompatible(LuaXTypeDefinition @override, LuaXTypeDefinition @base)
        {
            if (@override.IsTheSame(@base))
                return true;

            //non-object types should match exactly
            if (!@override.IsObject() || !@base.IsObject())
                return false;

            return mMetadata.IsKindOf(@override.Class, @base.Class);
        }

        public bool SearchProperty(string propertyName, out LuaXProperty property, out string ownerClassName, bool doNotLookInOwnerClasses = false)
        {
            LuaXPropertyCollection properties = this.Properties;
            string className = this.Name;
            while (!string.IsNullOrEmpty(className))
            {
                var propertyIndex = properties.Find(propertyName);
                if (propertyIndex < 0)
                {
                    if (ParentClass != null && ParentClass.SearchProperty(propertyName, out property, out ownerClassName))
                    {
                        return true;
                    }
                    if (!doNotLookInOwnerClasses && HasOwnerClass(in className, out className, out var @class))
                    {
                        properties = @class.Properties;
                        continue;
                    }
                    break;
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
                    if (ParentClass != null && ParentClass.SearchConstant(propertyName, out constant))
                    {
                        return true;
                    }
                    if (HasOwnerClass(in className, out className, out var @class))
                    {
                        constants = @class.Constants;
                        continue;
                    }
                    break;
                }
                constant = constants[constantIndex];
                return true;
            }
            constant = null;
            return false;
        }

        public bool SearchMethod(string propertyName, out LuaXMethod method, bool doNotLookInOwnerClasses = false)
        {
            LuaXMethodCollection methods = this.Methods;
            string className = this.Name;
            while (!string.IsNullOrEmpty(className))
            {
                var methodIndex = methods.Find(propertyName);
                if (methodIndex < 0)
                {
                    if (ParentClass != null && ParentClass.SearchMethod(propertyName, out method, doNotLookInOwnerClasses))
                    {
                        return true;
                    }
                    if(!doNotLookInOwnerClasses && HasOwnerClass(in className, out className, out var @class))
                    {
                        methods = @class.Methods;
                        continue;
                    }
                    break;
                }
                method = methods[methodIndex];
                return true;
            }
            method = null;
            return false;
        }

        public bool HasOwnerClass(in string sourceClassName, out string resultClassName, out LuaXClass @class)
        {
            int indexOfPoint = sourceClassName.LastIndexOf('.');
            if (indexOfPoint > 0)
            {
                resultClassName = sourceClassName[..indexOfPoint];
                if (mMetadata.Search(sourceClassName, out @class))
                    return true;
            }
            resultClassName = null;
            @class = null;
            return false;
        }
    }
}
