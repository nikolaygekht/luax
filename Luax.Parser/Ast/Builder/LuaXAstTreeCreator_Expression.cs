using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Security;
using System.Text;
using Hime.Redist;
using Luax.Parser.Ast.LuaExpression;

namespace Luax.Parser.Ast.Builder
{
    internal partial class LuaXAstTreeCreator
    {
        public LuaXExpression ProcessExpression(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (IsPassTroughExpression(astNode))
                return ProcessExpression(astNode.Children[0], currentClass, currentMethod);

            switch (astNode.Symbol)
            {
                case "CONSTANT":
                    return ProcessConstantExpression(astNode);
                case "VARIABLE":
                    return ProcessVariable(astNode, currentClass, currentMethod);
                case "PROPERTY":
                    return ProcessProperty(astNode, currentClass, currentMethod);
                case "BRACKET_EXPRESSION":
                    return ProcessBracket(astNode, currentClass, currentMethod);
                default:
                    throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected symbol {astNode.Symbol}");
            }
        }

        /// <summary>
        /// PRocesses bracket expression
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessBracket(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count != 3)
                throw new LuaXAstGeneratorException(Name, astNode, "Expression is expected");
            return ProcessExpression(astNode.Children[1], currentClass, currentMethod);
        }

        /// <summary>
        /// Processes property
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessProperty(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count != 3 || astNode.Children[2].Symbol != "IDENTIFIER")
                throw new LuaXAstGeneratorException(Name, astNode, "Expression and property name are expected");

            var leftSide = ProcessExpression(astNode.Children[0], currentClass, currentMethod);
            var name = astNode.Children[2].Value;
            var location = new LuaXElementLocation(Name, astNode);

            if (leftSide is LuaXClassNameExpression classNameExpression)
            {
                //static property call
                if (!Metadata.Search(classNameExpression.Name, out var leftSideClass))
                    throw new LuaXAstGeneratorException(Name, astNode, $"Class {classNameExpression.Name} is not found in metadata");
                if (!leftSideClass.Properties.Search(name, out var property))
                    throw new LuaXAstGeneratorException(Name, astNode, $"Class {classNameExpression.Name} does not contain property {name}");
                if (!property.Static)
                    throw new LuaXAstGeneratorException(Name, astNode, $"Property {classNameExpression.Name}.{name} is not static");
                if (property.Visibility == LuaXVisibility.Private && classNameExpression.ReturnType.Class != currentClass.Name)
                    throw new LuaXAstGeneratorException(Name, astNode, $"Property {classNameExpression.Name}.{name} is private and cannot be accessed");

                return new LuaXStaticPropertyExpression(classNameExpression.ReturnType.Class, name, property.LuaType, location);
            }
            else
            {
                //instance property call
                var leftSideType = leftSide.ReturnType;
                if (leftSideType.Array)
                {
                    if (name != "length")
                        throw new LuaXAstGeneratorException(Name, astNode, $"The array does not have the property {name}");
                    return new LuaXArrayLengthExpression(leftSide, new LuaXTypeDefinition() { TypeId = LuaXType.Integer }, location);
                }
                else if (leftSideType.TypeId == LuaXType.Object)
                {
                    if (!Metadata.Search(leftSideType.Class, out var leftSideClass))
                        throw new LuaXAstGeneratorException(Name, astNode, $"Class {leftSideType.Class} is not found in metadata");
                    if (!leftSideClass.Properties.Search(name, out var property))
                        throw new LuaXAstGeneratorException(Name, astNode, $"Class {leftSideType.Class} does not contain property {name}");
                    if (property.Static)
                        throw new LuaXAstGeneratorException(Name, astNode, $"Property {leftSideType.Class}.{name} is static");
                    if (property.Visibility == LuaXVisibility.Private && leftSideType.Class != currentClass.Name)
                        throw new LuaXAstGeneratorException(Name, astNode, $"Property {leftSideType.Class}.{name} is private and cannot be accessed");
                    return new LuaXPropertyExpression(leftSide, name, property.LuaType, location);
                }
                else
                    throw new LuaXAstGeneratorException(Name, astNode, $"The left side argument of the property access expression is not a class or an array");
            }
        }

        /// <summary>
        /// Processes variable access
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        public LuaXExpression ProcessVariable(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count != 1 || astNode.Children[0].Symbol != "IDENTIFIER")
                throw new LuaXAstGeneratorException(Name, astNode, "Identifier is expected here");

            var location = new LuaXElementLocation(Name, astNode);

            var name = astNode.Children[0].Value;

            if (!currentMethod.Static && name == "this")
                return new LuaXVariableExpression("this",
                    new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Object,
                        Class = currentClass.Name
                    }, location);

            LuaXVariable v2 = null;
            if (currentMethod.Arguments.Search(name, out var v1) || currentMethod.Variables.Search(name, out v2))
                return new LuaXVariableExpression(name, (v1 ?? v2).LuaType, location);
            if (currentClass.Properties.Search(name, out var p1))
            {
                if (p1.Static)
                    return new LuaXStaticPropertyExpression(currentClass.Name, name, p1.LuaType, location);
                else
                {
                    if (currentMethod.Static)
                        throw new LuaXAstGeneratorException(Name, astNode, $"Can't access instance property {name} in a static method");

                    return new LuaXPropertyExpression(new LuaXVariableExpression("this", new LuaXTypeDefinition() { TypeId = LuaXType.Object, Class = currentClass.Name }, location),
                        name, p1.LuaType, location);
                }
            }

            if (Metadata.Search(name, out _))
                return new LuaXClassNameExpression(name, location);

            throw new LuaXAstGeneratorException(Name, astNode, $"Identifier {name} is not an argument, property, method or class name");
        }

        /// <summary>
        /// Checks whether the node is a tree node to pass trough
        /// </summary>
        /// <param name="astNode"></param>
        /// <returns></returns>
        public bool IsPassTroughExpression(IAstNode astNode)
        {
            switch (astNode.Symbol)
            {
                case "EXPR":
                case "REXPR":
                case "SIMPLE_EXPR":
                case "OR_BOOL_EXPR":
                case "AND_BOOL_EXPR":
                case "UX_BOOL_EXPR":
                case "REL_EXPR":
                case "ADD_EXPR":
                case "MUL_EXPR":
                case "POWER_EXPR":
                case "UNARY_EXPR":
                case "CALLABLE_EXPR":
                case "ASSIGN_TARGET":
                    if (astNode.Children.Count == 1 && string.IsNullOrEmpty(astNode.Value))
                        return true;
                    throw new LuaXAstGeneratorException(Name, astNode, $"The symbol {astNode.Symbol} is expected to have one child only and have no value");
                default:
                    return false;
            }
        }

        /// <summary>
        /// Processes a constant expression node
        /// </summary>
        /// <param name="astNode"></param>
        /// <returns></returns>
        public LuaXExpression ProcessConstantExpression(IAstNode astNode) => new LuaXConstantExpression(ProcessConstant(astNode));
    }
}
