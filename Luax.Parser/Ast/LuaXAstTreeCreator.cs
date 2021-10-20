using System;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using Hime.Redist;
using Microsoft.VisualBasic;

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
                body.Classes.Add(ProcessClass(root.Children[i]));
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
            int start = 0;

            if (astNode.Children.Count > 1 && astNode.Children[0].Symbol == "ATTRIBUTES")
                start = 1;

            if (astNode.Children.Count < start + 1)
                throw new LuaXAstGeneratorException(Name, astNode, $"At least {start} children symbol is expected in an class node");

            string name, parent = null;

            if (astNode.Children[start].Symbol != "IDENTIFIER")
                throw new LuaXAstGeneratorException(Name, astNode.Children[start], "IDENTIFIER expected");

            name = astNode.Children[start].Value;

            if (astNode.Children.Count >= start + 2)
            {
                if (astNode.Children[start + 1].Symbol != "PARENT_CLASS" ||
                    astNode.Children[start + 1].Children.Count < 1 ||
                    astNode.Children[start + 1].Children[0].Symbol != "IDENTIFIER")
                    throw new LuaXAstGeneratorException(Name, astNode.Children[start], "IDENTIFIER expected");
                parent = astNode.Children[start + 1].Children[0].Value;
            }

            LuaXClass cls = new LuaXClass(name, parent);

            if (start == 1)
                ProcessAttributes(astNode.Children[0].Children, cls.Attributes);

            return cls;
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
                if (attr.Symbol != "ATTRIBUTE")
                    throw new LuaXAstGeneratorException(Name, attr, "Attribute expected");
                target.Add(ProcessAttribute(attr));
            }
        }

        /// <summary>
        /// Processes attribute
        /// </summary>
        /// <param name="attributeNode"></param>
        /// <returns></returns>
        public LuaXAttribute ProcessAttribute(IAstNode astNode)
        {
            if (astNode.Children.Count < 1)
                throw new LuaXAstGeneratorException(Name, astNode, "At least one child symbol is expected in an attribute");

            if (astNode.Children[0].Symbol != "IDENTIFIER")
                throw new LuaXAstGeneratorException(Name, astNode, "Identifier expected");

            string name = astNode.Children[0].Value;

            var attribute = new LuaXAttribute(name);

            if (astNode.Children.Count == 2 && astNode.Children[1].Symbol == "CONSTANTS")
            {
                var args = astNode.Children[1];
                for (int i = 0; i < args.Children.Count; i++)
                {
                    var arg = args.Children[i];
                    if (arg.Symbol != "CONSTANT")
                        throw new LuaXAstGeneratorException(Name, arg, "Constant expected");
                    attribute.Parameters.Add(ProcessConstant(arg));
                }
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

            if (s == "INTEGER")
                return new LuaXConstant(ProcessIntegerConstant(astNode.Children[0]));
            else if (s == "HEX_INTEGER")
                return new LuaXConstant(ProcessIntegerConstant(astNode.Children[0]));
            if (s == "STRING")
                return new LuaXConstant(ProcessStringConstant(astNode.Children[0]));
            if (s == "BOOLEAN")
                return ProcessBooleanConstant(astNode.Children[0]) ? LuaXConstant.True : LuaXConstant.False;
            if (s == "NEGATIVE_INTEGER")
                return new LuaXConstant(ProcessNegativeIntegerConstant(astNode.Children[0]));
            if (s == "REAL")
                return new LuaXConstant(ProcessRealConstant(astNode.Children[0]));
            if (s == "NEGATIVE_REAL")
                return new LuaXConstant(ProcessNegativeRealConstant(astNode.Children[0]));
            if (s == "NIL")
                return LuaXConstant.Nil;
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
            if (astNode.Children.Count < 1)
                throw new LuaXAstGeneratorException(Name, astNode, "At least one child symbol is expected in a constant");

            if (astNode.Children[0].Symbol != "REAL")
                throw new LuaXAstGeneratorException(Name, astNode, "A REAL symbol is expected");

            return -ProcessRealConstant(astNode.Children[0]);
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
            if (astNode.Children.Count < 1)
                throw new LuaXAstGeneratorException(Name, astNode, "At least one child symbol is expected in a constant");

            if (astNode.Children[0].Symbol != "INTEGER")
                throw new LuaXAstGeneratorException(Name, astNode, "A INTEGER symbol is expected");

            return -ProcessIntegerConstant(astNode.Children[0]);
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
    }
}
