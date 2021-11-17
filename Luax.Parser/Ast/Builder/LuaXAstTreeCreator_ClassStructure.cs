using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Hime.Redist;
using Luax.Parser.Ast.LuaExpression;
using Luax.Parser.Ast.Statement;
using Microsoft.VisualBasic.FileIO;

namespace Luax.Parser.Ast.Builder
{
    internal partial class LuaXAstTreeCreator
    {
        public string Name { get; }

        public LuaXClassCollection Metadata { get; }

        public LuaXAstTreeCreator(string name, LuaXClassCollection metadata = null)
        {
            Metadata = metadata;
            Name = name;
        }

        /// <summary>
        /// Processes the root node of the AST tree.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public LuaXBody Create(ASTNode root) => Create(new AstNodeWrapper(root));

        /// <summary>
        /// Processes the root node of the AST tree.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public LuaXBody Create(IAstNode root)
        {
            if (root.Symbol != "ROOT")
                throw new LuaXAstGeneratorException(Name, root, $"Unexpected element {root.Symbol}");

            var body = new LuaXBody(Name);

            for (int i = 0; i < root.Children.Count; i++)
            {
                if (root.Children[i].Symbol != "CLASS_DECLARATION")
                    throw new LuaXAstGeneratorException(Name, root, $"Unexpected element {root.Symbol}. Class declaration is expected");
                var @class = ProcessClass(root.Children[i]);
                if (body.Classes.Contains(@class.Name))
                    throw new LuaXAstGeneratorException(Name, root, $"The class with the name {@class.Name} already defined");
                body.Classes.Add(@class);
            }
            return body;
        }

        /// <summary>
        /// Processes the class declaration
        /// </summary>
        /// <param name="classNode"></param>
        /// <returns></returns>
        public LuaXClass ProcessClass(IAstNode astNode)
        {
            IAstNode attributes = null;
            string name = null;
            string parent = null;
            IAstNode location = astNode;
            int elementStartsAt = -1;
            for (int i = 0; i < astNode.Children.Count; i++)
            {
                var child = astNode.Children[i];
                switch (child.Symbol)
                {
                    case "CLASS":
                        location = child;
                        continue;
                    case "END":
                        continue;
                    case "ATTRIBUTES":
                        attributes = child;
                        continue;
                    case "IDENTIFIER":
                        name = child.Value;
                        continue;
                    case "PARENT_CLASS":
                        parent = FindParentClassName(child);
                        continue;
                }
                if (child.Symbol == "CLASS_ELEMENT")
                {
                    elementStartsAt = i;
                    break;
                }
                throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected symbol {child.Symbol}");
            }

            var @class = new LuaXClass(name, parent ?? "object", new LuaXElementLocation(Name, location));

            if (attributes != null)
                ProcessAttributes(attributes.Children, @class.Attributes);

            if (elementStartsAt > 0)
            {
                for (int i = elementStartsAt; i < astNode.Children.Count; i++)
                {
                    var child = astNode.Children[i];
                    if (child.Symbol == "CLASS_ELEMENT")
                        ProcessClassElement(child, @class);
                }
            }
            return @class;
        }

        private string FindParentClassName(IAstNode node)
        {
            string parent = null;
            for (int j = 0; j < node.Children.Count; j++)
            {
                var child1 = node.Children[j];
                if (child1.Symbol == "COLON")
                    continue;
                if (child1.Symbol == "IDENTIFIER")
                {
                    parent = child1.Value;
                    continue;
                }
                throw new LuaXAstGeneratorException(Name, node, $"Unexpected symbol {child1.Symbol}");
            }
            return parent;
        }

        /// <summary>
        /// Processes attributes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void ProcessAttributes(AstNodeCollection source, LuaXAttributeCollection target)
        {
            for (int i = 0; i < source.Count; i++)
            {
                var attr = source[i];
                if (attr.Symbol == "COMMA")
                    continue;
                if (attr.Symbol == "ATTRIBUTE")
                {
                    target.Add(ProcessAttribute(attr));
                    continue;
                }
                throw new LuaXAstGeneratorException(Name, attr, $"Unexpected symbol {attr.Symbol}");
            }
        }

        /// <summary>
        /// Processes attribute
        /// </summary>
        /// <param name="attributeNode"></param>
        /// <returns></returns>
        public LuaXAttribute ProcessAttribute(IAstNode astNode)
        {
            LuaXAttribute attribute = null;

            for (int i = 0; i < astNode.Children.Count; i++)
            {
                var child = astNode.Children[i];

                switch (child.Symbol)
                {
                    case "AT":
                    case "L_ROUND_BRACKET":
                    case "R_ROUND_BRACKET":
                        continue;

                    case "IDENTIFIER":
                        attribute = new LuaXAttribute(child.Value, new LuaXElementLocation(Name, astNode));
                        continue;

                    case "CONSTANTS":
                        if (attribute == null)
                            throw new LuaXAstGeneratorException(Name, astNode, "Identifier expected");
                        ProcessAttributeParameter(attribute, child);
                        continue;
                }
                throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected symbol {child.Symbol}");
            }
            return attribute;
        }

        private void ProcessAttributeParameter(LuaXAttribute attribute, IAstNode child)
        {
            for (int j = 0; j < child.Children.Count; j++)
            {
                var child1 = child.Children[j];
                if (child1.Symbol == "COMMA")
                    continue;
                if (child1.Symbol == "CONSTANT")
                {
                    attribute.Parameters.Add(ProcessConstant(child1));
                    continue;
                }
                throw new LuaXAstGeneratorException(Name, child, "Constant expected");
            }
        }

        /// <summary>
        /// Processes the constant node
        /// </summary>
        /// <param name="astNode"></param>
        /// <returns></returns>
        public LuaXConstant ProcessConstant(IAstNode astNode)
        {
            if (astNode.Children.Count < 1)
                throw new LuaXAstGeneratorException(Name, astNode, "At least one child symbol is expected in a constant");

            var s = astNode.Children[0].Symbol;
            var l = new LuaXElementLocation(Name, astNode);
            return s switch
            {
                "INTEGER" => new LuaXConstant(ProcessIntegerConstant(astNode.Children[0]), l),
                "HEX_INTEGER" => new LuaXConstant(ProcessIntegerConstant(astNode.Children[0]), l),
                "STRING" => new LuaXConstant(ProcessStringConstant(astNode.Children[0]), l),
                "BOOLEAN" => new LuaXConstant(ProcessBooleanConstant(astNode.Children[0]), l),
                "NEGATIVE_INTEGER" => new LuaXConstant(ProcessNegativeIntegerConstant(astNode.Children[0]), l),
                "REAL" => new LuaXConstant(ProcessRealConstant(astNode.Children[0]), l),
                "NEGATIVE_REAL" => new LuaXConstant(ProcessNegativeRealConstant(astNode.Children[0]), l),
                "NIL" => new LuaXConstant(LuaXType.Object, null, l),
                _ => throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected child symbol {s} is expected in a constant"),
            };
        }

        /// <summary>
        /// Processes the real constant node
        /// </summary>
        /// <param name="astNode"></param>
        /// <returns></returns>
        private double ProcessRealConstant(IAstNode astNode)
        {
            if (string.IsNullOrEmpty(astNode.Value) || !double.TryParse(astNode.Value.Replace("_", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                throw new LuaXAstGeneratorException(Name, astNode, "At real value is expected");

            return x;
        }

        /// <summary>
        /// Processes the negative real constant node
        /// </summary>
        /// <param name="astNode"></param>
        /// <returns></returns>
        private double ProcessNegativeRealConstant(IAstNode astNode)
        {
            if (astNode.Children.Count < 2)
                throw new LuaXAstGeneratorException(Name, astNode, "At least two child symbols is expected in a constant");

            if (astNode.Children[1].Symbol != "REAL")
                throw new LuaXAstGeneratorException(Name, astNode, "A REAL symbol is expected");

            return -ProcessRealConstant(astNode.Children[1]);
        }

        /// <summary>
        /// Process the integer constant node
        /// </summary>
        /// <param name="astNode"></param>
        /// <returns></returns>
        private int ProcessIntegerConstant(IAstNode astNode)
        {
            int x = 0;
            bool parsed = false;
            string value = astNode.Value;

            if (!string.IsNullOrEmpty(value))
            {
                NumberStyles styles;
                if (value.StartsWith("0x") || value.StartsWith("0X"))
                {
                    value = value[2..];
                    styles = NumberStyles.HexNumber;
                }
                else
                    styles = NumberStyles.Integer;

                parsed = int.TryParse(value.Replace("_", ""), styles, CultureInfo.InvariantCulture, out x);
            }

            if (!parsed)
                throw new LuaXAstGeneratorException(Name, astNode, "An integer value is expected");

            return x;
        }

        /// <summary>
        /// Processes the negative integer constant node
        /// </summary>
        /// <param name="astNode"></param>
        /// <returns></returns>
        private int ProcessNegativeIntegerConstant(IAstNode astNode)
        {
            if (astNode.Children.Count < 2)
                throw new LuaXAstGeneratorException(Name, astNode, "At least two child symbols is expected in a constant");

            if (astNode.Children[1].Symbol != "INTEGER")
                throw new LuaXAstGeneratorException(Name, astNode, "A INTEGER symbol is expected");

            return -ProcessIntegerConstant(astNode.Children[1]);
        }

        /// <summary>
        /// Processes the boolean constant node
        /// </summary>
        /// <param name="astNode"></param>
        /// <returns></returns>
        private bool ProcessBooleanConstant(IAstNode astNode)
        {
            if (astNode.Children.Count < 1)
                throw new LuaXAstGeneratorException(Name, astNode, "At least one child symbol is expected in a constant");

            if (astNode.Children[0].Symbol == "BOOLEAN_TRUE")
                return true;
            if (astNode.Children[0].Symbol == "BOOLEAN_FALSE")
                return false;
            throw new LuaXAstGeneratorException(Name, astNode, "A TRUE or FALSE symbol is expected");
        }

        /// <summary>
        /// Processes the string constant node
        /// </summary>
        /// <param name="astNode"></param>
        /// <returns></returns>
        private string ProcessStringConstant(IAstNode astNode)
        {
            if (astNode.Children.Count < 1)
                throw new LuaXAstGeneratorException(Name, astNode, "At least one child symbol is expected in a constant");

            if (astNode.Children[0].Symbol != "STRINGDQ")
                throw new LuaXAstGeneratorException(Name, astNode, "A STRING symbol is expected");

            var v = astNode.Children[0].Value[1..^1];
            if (v.Contains('\\'))
            {
                var sb = new StringBuilder();
                for (int i = 0; i < v.Length; i++)
                {
                    if (v[i] == '\\')
                    {
                        i++;
                        switch (v[i])
                        {
                            case '\\':
                                sb.Append('\\');
                                break;
                            case '"':
                                sb.Append('"');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                        }
                    }
                    else
                        sb.Append(v[i]);
                }
                v = sb.ToString();
            }
            return v;
        }

        public void ProcessConstantDeclarationInClass(IAstNode node, LuaXClass @class)
        {
            IAstNode attributes = null;
            for (int i = 0; i < node.Children.Count; i++)
            {
                if (node.Children[i].Symbol == "ATTRIBUTES")
                    attributes = node.Children[i];
                else if (node.Children[i].Symbol == "CONST_DECLARATION")
                {
                    var decl = ProcessConstantDeclaration(node.Children[i]);
                    if (@class.Constants.Contains(decl.Name))
                        throw new LuaXAstGeneratorException(Name, node, "The constant with the name specified is already defined");

                    if (@class.Properties.Contains(decl.Name))
                        throw new LuaXAstGeneratorException(Name, node, "The variable with the name specified is already defined");

                    if (attributes != null)
                        ProcessAttributes(attributes.Children, decl.Attributes);

                    @class.Constants.Add(decl);
                }
            }
        }

        /// <summary>
        /// Processes a class element
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        public void ProcessClassElement(IAstNode node, LuaXClass @class)
        {
            for (int j = 0; j < node.Children.Count; j++)
            {
                var child = node.Children[j];

                if (child.Symbol == "PROPERTY")
                    ProcessProperty(child, @class);
                else if (child.Symbol == "FUNCTION_DECLARATION")
                    ProcessFunction(child, @class);
                else if (child.Symbol == "CLASS_CONST_DECLARATION")
                    ProcessConstantDeclarationInClass(child, @class);
                else if (child.Symbol == "EXTERN_DECLARATION")
                    ProcessExtern(child, @class);
                else
                    throw new LuaXAstGeneratorException(Name, node, $"Unexpected symbol {child.Symbol}");
            }
        }

        private LuaXVisibility ProcessVisibility(IAstNode node)
        {
            if (node.Children.Count != 1)
                throw new LuaXAstGeneratorException(Name, node, "Visibility is expected here");
            if (node.Children[0].Symbol == "VISIBILITY_PUBLIC")
                return LuaXVisibility.Public;
            else if (node.Children[0].Symbol == "VISIBILITY_INTERNAL")
                return LuaXVisibility.Internal;
            else if (node.Children[0].Symbol == "VISIBILITY_PRIVATE")
                return LuaXVisibility.Private;
            else
                throw new LuaXAstGeneratorException(Name, node, "Visibility is expected here");
        }

        /// <summary>
        /// Processes a PROPERTY node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        public void ProcessProperty(IAstNode node, LuaXClass @class)
        {
            bool @static = false;
            LuaXVisibility visibility = LuaXVisibility.Private;
            IAstNode attributes = null;

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                switch (child.Symbol)
                {
                    case "ATTRIBUTES":
                        attributes = child;
                        break;
                    case "VISIBILITY":
                        visibility = ProcessVisibility(child);
                        break;
                    case "STATIC":
                        @static = true;
                        break;
                    case "DECLARATION":
                        ProcessDeclarationInProperty(child, @class, @static, visibility, attributes);
                        break;
                }
            }
        }

        private void ProcessDeclarationInProperty(IAstNode child, LuaXClass @class, bool @static, LuaXVisibility visibility, IAstNode attributes)
        {
            LuaXPropertyFactory factory = new LuaXPropertyFactory(@static, visibility);
            for (int j = 0; j < child.Children.Count; j++)
            {
                var child1 = child.Children[j];
                if (child1.Symbol == "DECL_LIST")
                {
                    ProcessDeclarationList(child1, factory, p =>
                        ProcessDeclarationInProperty(p, @class, child, attributes));
                }
                else if (child1.Symbol != "VAR" && child1.Symbol != "EOS")
                    throw new LuaXAstGeneratorException(Name, child, $"One or more DECL is expected here but found {child1.Symbol}");
            }
        }

        private void ProcessDeclarationInProperty(LuaXProperty p, LuaXClass @class, IAstNode child, IAstNode attributes)
        {
            if (@class.Properties.Contains(p.Name))
                throw new LuaXAstGeneratorException(Name, child, $"The property {p.Name} already exists");
            if (@class.Constants.Contains(p.Name))
                throw new LuaXAstGeneratorException(Name, child, $"The constant {p.Name} already exists");
            if (attributes != null)
                ProcessAttributes(attributes.Children, p.Attributes);
            @class.Properties.Add(p);
        }

        /// <summary>
        /// Processes a list of declaration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="factory"></param>
        /// <param name="addAction"></param>
        public void ProcessDeclarationList<T>(IAstNode node, LuaXVariableFactory<T> factory, Action<T> addAction)
            where T : LuaXVariable, new()
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "DECL")
                    addAction(ProcessDeclaration<T>(child, factory));
                else if (child.Symbol != "COMMA")
                    throw new LuaXAstGeneratorException(Name, child, "DECL is expected here");
            }
        }

        /// <summary>
        /// Processes a declaration part of a variable or a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public T ProcessDeclaration<T>(IAstNode node, LuaXVariableFactory<T> factory)
            where T : LuaXVariable, new()
        {
            string name = null;
            LuaXTypeDefinition type = null;
            var l = new LuaXElementLocation(Name, node);

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "IDENTIFIER")
                    name = child.Value;
                else if (child.Symbol == "TYPE_DECL")
                    type = ProcessTypeDecl(child, false);
                else if (child.Symbol != "COLON")
                    throw new LuaXAstGeneratorException(Name, child, $"Unexpected symbol {child.Value}");
            }

            if (name == null)
                throw new LuaXAstGeneratorException(Name, node, "IDENTIFIER is expected");

            if (type == null)                   //NOSONAR -- false positive
                throw new LuaXAstGeneratorException(Name, node, "TYPE_DECL is expected");

            return factory.Create(name, type, l);
        }

        public bool ProcessTypeName(IAstNode node, bool allowVoid, out LuaXType type, out string className)
        {
            if (node.Children.Count != 1)
                throw new LuaXAstGeneratorException(Name, node, "TYPE_NAME specification is expected here");

            type = LuaXType.Void;
            className = null;

            var child = node.Children[0];
            if (child.Symbol == "IDENTIFIER")
            {
                type = LuaXType.Object;
                className = child.Value;
            }
            else if (child.Symbol == "TYPE_INT")
                type = LuaXType.Integer;
            else if (child.Symbol == "TYPE_REAL")
                type = LuaXType.Real;
            else if (child.Symbol == "TYPE_BOOLEAN")
                type = LuaXType.Boolean;
            else if (child.Symbol == "TYPE_DATETIME")
                type = LuaXType.Datetime;
            else if (child.Symbol == "TYPE_STRING")
                type = LuaXType.String;
            else if (child.Symbol == "TYPE_VOID")
            {
                if (!allowVoid)
                    throw new LuaXAstGeneratorException(Name, node, "Type void cannot be used in this context");
                type = LuaXType.Void;
            }
            else
                return false;

            return true;
        }

        public LuaXTypeDefinition ProcessTypeDecl(IAstNode node, bool allowVoid)
        {
            LuaXType? type = null;
            bool array = false;
            string @class = null;

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "TYPE_NAME")
                {
                    if (ProcessTypeName(child, allowVoid, out var t, out @class))
                        type = t;
                }
                else if (child.Symbol == "ARRAY_DECL")
                    array = true;
            }

            if (type == null)
                throw new LuaXAstGeneratorException(Name, node, "TYPE_DECL is expected");

            return new LuaXTypeDefinition()
            {
                TypeId = type.Value,
                Array = array,
                Class = @class
            };
        }

        public LuaXConstantVariable ProcessConstantDeclaration(IAstNode node)
        {
            string name = null;
            LuaXConstant value = null;

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "IDENTIFIER")
                    name = child.Value;
                else if (child.Symbol == "CONSTANT")
                    value = ProcessConstant(child);
            }

            if (name == null)
                throw new LuaXAstGeneratorException(Name, node, "Identifier is expected here");
            if (value == null)          //NOSONAR -- false positive
                throw new LuaXAstGeneratorException(Name, node, "Constant value is expected here");

            return new LuaXConstantVariable() { Name = name, Value = value };
        }

        /// <summary>
        /// Processes a Function node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        public void ProcessFunction(IAstNode node, LuaXClass @class)
        {
            bool @static = false;
            LuaXVisibility visibility = LuaXVisibility.Private;
            string name = null;
            LuaXTypeDefinition returnType = null;

            IAstNode attributes = null;
            IAstNode arguments = null;
            IAstNode body = null;

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                switch (child.Symbol)
                {
                    case "ATTRIBUTES":
                        attributes = child;
                        break;
                    case "VISIBILITY":
                        visibility = ProcessVisibility(child);
                        break;
                    case "STATIC":
                        @static = true;
                        break;
                    case "IDENTIFIER":
                        name = child.Value;
                        break;
                    case "FUNCTION_DECLARATION_ARGS":
                        arguments = child;
                        break;
                    case "TYPE_DECL":
                        returnType = ProcessTypeDecl(child, true);
                        break;
                    case "STATEMENTS":
                        body = child;
                        break;
                }
            }

            if (name == null)
                throw new LuaXAstGeneratorException(Name, node, "IDENTIFIER is expected here");

            if (returnType == null)             //NOSONAR -- false positive
                throw new LuaXAstGeneratorException(Name, node, "TYPE_DECL is expected here");

            LuaXMethod method = new LuaXMethod(@class)
            {
                Name = name,
                Static = @static,
                Visibility = visibility,
                ReturnType = returnType,
                Extern = false,
                Location = new LuaXElementLocation(Name, node),
                Body = body
            };

            if (attributes != null)
                ProcessAttributes(attributes.Children, method.Attributes);

            ProcessMethodDefinition(node, @class, method, arguments);
        }

        /// <summary>
        /// Processes a Extern node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        public void ProcessExtern(IAstNode node, LuaXClass @class)
        {
            bool @static = false;
            LuaXVisibility visibility = LuaXVisibility.Private;
            string name = null;
            LuaXTypeDefinition returnType = null;

            IAstNode arguments = null;

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];

                switch (child.Symbol)
                {
                    case "VISIBILITY":
                        visibility = ProcessVisibility(child);
                        break;
                    case "STATIC":
                        @static = true;
                        break;
                    case "IDENTIFIER":
                        name = child.Value;
                        break;
                    case "FUNCTION_DECLARATION_ARGS":
                        arguments = child;
                        break;
                    case "TYPE_DECL":
                        returnType = ProcessTypeDecl(child, true);
                        break;
                    case "EOS":
                        break;
                }
            }

            if (name == null)
                throw new LuaXAstGeneratorException(Name, node, "IDENTIFIER is expected here");

            if (returnType == null)             //NOSONAR -- false positive
                throw new LuaXAstGeneratorException(Name, node, "TYPE_DECL is expected here");

            LuaXMethod method = new LuaXMethod(@class)
            {
                Name = name,
                Static = @static,
                Visibility = visibility,
                ReturnType = returnType,
                Extern = true,
                Location = new LuaXElementLocation(Name, node)
            };

            ProcessMethodDefinition(node, @class, method, arguments);
        }

        private void ProcessMethodDefinition(IAstNode node, LuaXClass @class, LuaXMethod method, IAstNode arguments)
        {
            if (arguments?.Children.Count > 1 &&
                arguments.Children[1].Symbol == "DECL_LIST")
            {
                ProcessDeclarationList(arguments.Children[1], new LuaXVariableFactory<LuaXVariable>(), v =>
                {
                    if (method.Arguments.Contains(v.Name))
                        throw new LuaXAstGeneratorException(Name, node, $"The method already has argument with the name {v.Name}");
                    method.Arguments.Add(v);
                });
            }

            if (@class.Methods.Contains(method.Name))
                throw new LuaXAstGeneratorException(Name, node, $"The method with the name {method.Name} already exists");

            if (!method.Static && method.Name == @class.Name && method.ReturnType.IsVoid() &&
                 method.Arguments.Count == 0)
            {
                method.IsConstructor = true;
                @class.Constructor = method;
            }

            @class.Methods.Add(method);
        }
    }
}
