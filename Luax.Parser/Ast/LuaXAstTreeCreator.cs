using System;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Hime.Redist;

namespace Luax.Parser.Ast
{
    internal class LuaXAstTreeCreator
    {
        public string Name { get; }

        public LuaXAstTreeCreator(string name)
        {
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
                if (child.Symbol == "CLASS")
                {
                    location = child;
                    continue;
                }
                if (child.Symbol == "END")
                    continue;

                if (child.Symbol == "ATTRIBUTES")
                {
                    attributes = child;
                    continue;
                }
                if (child.Symbol == "IDENTIFIER")
                {
                    name = child.Value;
                    continue;
                }
                if (child.Symbol == "PARENT_CLASS")
                {
                    for (int j = 0; j < child.Children.Count; j++)
                    {
                        var child1 = child.Children[j];
                        if (child1.Symbol == "COLON")
                            continue;
                        if (child1.Symbol == "IDENTIFIER")
                        {
                            parent = child1.Value;
                            continue;
                        }
                        throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected symbol {child1.Symbol}");
                    }
                    continue;
                }
                if (child.Symbol == "CLASS_ELEMENT")
                {
                    elementStartsAt = i;
                    break;
                }
                throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected symbol {child.Symbol}");
            }

            var @class = new LuaXClass(name, parent, new LuaXElementLocation(Name, location));
            if (attributes != null)
                ProcessAttributes(attributes.Children, @class.Attributes);

            if (elementStartsAt > 0)
            {
                for (int i = elementStartsAt; i < astNode.Children.Count; i++)
                {
                    var child = astNode.Children[i];
                    if (child.Symbol == "END")
                        continue;
                    if (child.Symbol == "CLASS_ELEMENT")
                    {
                        for (int j = 0; j < child.Children.Count; j++)
                        {
                            var child1 = child.Children[j];
                            if (child1.Symbol == "PROPERTY")
                                ProcessProperty(child1, @class);
                            else if (child1.Symbol == "FUNCTION_DECLARATION")
                                ProcessFunction(child1, @class);
                            else
                                throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected symbol {child.Symbol}");
                        }
                    }
                }
            }

            return @class;
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

                if (child.Symbol == "AT" ||
                    child.Symbol == "L_ROUND_BRACKET" || child.Symbol == "R_ROUND_BRACKET")
                {
                    continue;
                }
                else if (child.Symbol == "IDENTIFIER")
                {
                    attribute = new LuaXAttribute(child.Value, new LuaXElementLocation(Name, astNode));
                    continue;
                }
                else if (child.Symbol == "CONSTANTS")
                {
                    if (attribute == null)
                        throw new LuaXAstGeneratorException(Name, astNode, "Identifier expected");

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
                        throw new LuaXAstGeneratorException(Name, astNode, "Constant expected");
                    }
                    continue;
                }
                throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected symbol {child.Symbol}");
            }
            return attribute;
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
            if (s == "INTEGER")
                return new LuaXConstant(ProcessIntegerConstant(astNode.Children[0]), l);
            else if (s == "HEX_INTEGER")
                return new LuaXConstant(ProcessIntegerConstant(astNode.Children[0]), l);
            if (s == "STRING")
                return new LuaXConstant(ProcessStringConstant(astNode.Children[0]), l);
            if (s == "BOOLEAN")
                return new LuaXConstant(ProcessBooleanConstant(astNode.Children[0]), l);
            if (s == "NEGATIVE_INTEGER")
                return new LuaXConstant(ProcessNegativeIntegerConstant(astNode.Children[0]), l);
            if (s == "REAL")
                return new LuaXConstant(ProcessRealConstant(astNode.Children[0]), l);
            if (s == "NEGATIVE_REAL")
                return new LuaXConstant(ProcessNegativeRealConstant(astNode.Children[0]), l);
            if (s == "NIL")
                return new LuaXConstant(LuaXType.Class, null, l);
            throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected child symbol {s} is expected in a constant");
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

        /// <summary>
        /// Processes a class element
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        public void ProcessClassElement(IAstNode node, LuaXClass @class)
        {
            if (node.Children.Count == 1)
            {
                node = node.Children[0];

                if (node.Symbol == "PROPERTY")
                {
                    ProcessProperty(node, @class);
                    return;
                }
                else if (node.Symbol == "FUNCTION_DECLARATION")
                {
                    ProcessFunction(node, @class);
                    return;
                }
            }
            throw new LuaXAstGeneratorException(Name, node, "A property or a function is expected here");
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

            LuaXPropertyFactory factory = null;
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "VISIBILITY")
                    visibility = ProcessVisibility(child);
                else if (child.Symbol == "STATIC")
                    @static = true;
                else if (child.Symbol == "DECLARATION")
                {
                    for (int j = 0; j < child.Children.Count; j++)
                    {
                        var child1 = child.Children[j];
                        factory = new LuaXPropertyFactory(@static, visibility);
                        if (child1.Symbol == "DECL_LIST")
                        {
                            ProcessDeclarationList<LuaXProperty>(child1, factory, p =>
                            {
                                if (@class.Properties.Contains(p.Name))
                                    throw new LuaXAstGeneratorException(Name, child, $"The property {p.Name} already exists");
                                @class.Properties.Add(p);
                            });
                        }
                        else if (child1.Symbol != "VAR" && child1.Symbol != "EOS")
                            throw new LuaXAstGeneratorException(Name, child, $"One or more DECL is expected here but found {child1.Symbol}");
                    }
                    if (factory == null)
                        factory = new LuaXPropertyFactory(@static, visibility);
                }
            }
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

#pragma warning disable S2589 // Boolean expressions should not be gratuitous: NG: it is false positive
            if (type == null)
                throw new LuaXAstGeneratorException(Name, node, "TYPE_DECL is expected");
#pragma warning restore S2589 

            return factory.Create(name, type, l);
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
                    if (child.Children.Count != 1)
                        throw new LuaXAstGeneratorException(Name, node, "TYPE_NAME specification is expected here");

                    var child1 = child.Children[0];
                    if (child1.Symbol == "IDENTIFIER")
                    {
                        type = LuaXType.Class;
                        @class = child1.Value;
                    }
                    else if (child1.Symbol == "TYPE_INT")
                        type = LuaXType.Integer;
                    else if (child1.Symbol == "TYPE_REAL")
                        type = LuaXType.Real;
                    else if (child1.Symbol == "TYPE_BOOLEAN")
                        type = LuaXType.Boolean;
                    else if (child1.Symbol == "TYPE_DATETIME")
                        type = LuaXType.Datetime;
                    else if (child1.Symbol == "TYPE_STRING")
                        type = LuaXType.String;
                    else if (child1.Symbol == "TYPE_VOID")
                    {
                        if (!allowVoid)
                            throw new LuaXAstGeneratorException(Name, node, "TYPE_VOID cannot be used in the variable declaration");
                        type = LuaXType.Void;
                    }
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
                if (child.Symbol == "ATTRIBUTES")
                    attributes = child;
                else if (child.Symbol == "VISIBILITY")
                    visibility = ProcessVisibility(child);
                else if (child.Symbol == "STATIC")
                    @static = true;
                else if (child.Symbol == "IDENTIFIER")
                    name = child.Value;
                else if (child.Symbol == "FUNCTION_DECLARATION_ARGS")
                    arguments = child;
                else if (child.Symbol == "TYPE_DECL")
                    returnType = ProcessTypeDecl(child, true);
                else if (child.Symbol == "STATEMENTS")
                    body = child;
            }

            if (name == null)
                throw new LuaXAstGeneratorException(Name, node, "IDENTIFIER is expected here");

#pragma warning disable S2589 // Boolean expressions should not be gratuitous: NG: false positive here
            if (returnType == null)
                throw new LuaXAstGeneratorException(Name, node, "TYPE_DECL is expected here");
#pragma warning restore S2589 

            LuaXMethod method = new LuaXMethod()
            {
                Name = name,
                Static = @static,
                Visibility = visibility,
                ReturnType = returnType,
                Location = new LuaXElementLocation(Name, node)
            };

            if (attributes != null)
                ProcessAttributes(attributes.Children, method.Attributes);

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

            if (body != null)
                ProcessBody(node, method);

            @class.Methods.Add(method);
        }

        /// <summary>
        /// Processes the method body
        /// </summary>
        /// <param name="node"></param>
        /// <param name="method"></param>
        public void ProcessBody(IAstNode node, LuaXMethod method)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol != "STATEMENT" || child.Children.Count != 1)
                    throw new LuaXAstGeneratorException(Name, child, "STATEMENT is expected");
                child = child.Children[0];
                if (child.Symbol == "DECLARATION")
                {
                    if (child.Children.Count < 2 || child.Children[1].Symbol != "DECL_LIST")
                        throw new LuaXAstGeneratorException(Name, child, "One or more DECL is expected here");

                    ProcessDeclarationList<LuaXVariable>(child.Children[1], new LuaXVariableFactory<LuaXVariable>(), v =>
                    {
                        if (method.Arguments.Contains(v.Name) || method.Variables.Contains(v.Name))
                            throw new LuaXAstGeneratorException(Name, child, $"Variable {v.Name} already exists");
                        method.Variables.Add(v);
                    });
                }
                else
                    throw new LuaXAstGeneratorException(Name, child, $"Unexpected symbol {child.Symbol}");
            }
        }
    }
}
