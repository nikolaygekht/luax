using System;
using System.Collections.Generic;
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
                case "CAST_OP":
                    return ProcessCast(astNode, currentClass, currentMethod);
                case "ARRAY_ACCESS":
                    return ProcessArrayAccess(astNode, currentClass, currentMethod);
                case "MINUS_OP":
                    if (astNode.Children.Count == 2)
                        return ProcessNegateOperator(astNode, currentClass, currentMethod);
                    else
                        return ProcessBinaryMathOperator(LuaXBinaryOperator.Subtract, astNode, currentClass, currentMethod);
                case "PLUS_OP":
                    if (astNode.Children.Count == 2)
                        return ProcessExpression(astNode.Children[1], currentClass, currentMethod);
                    else
                        return ProcessBinaryMathOperator(LuaXBinaryOperator.Add, astNode, currentClass, currentMethod);
                case "MUL_OP":
                    return ProcessBinaryMathOperator(LuaXBinaryOperator.Multiply, astNode, currentClass, currentMethod);
                case "DIV_OP":
                    return ProcessBinaryMathOperator(LuaXBinaryOperator.Divide, astNode, currentClass, currentMethod);
                case "REM_OP":
                    return ProcessBinaryMathOperator(LuaXBinaryOperator.Reminder, astNode, currentClass, currentMethod);
                case "POWER_OP":
                    return ProcessBinaryMathOperator(LuaXBinaryOperator.Power, astNode, currentClass, currentMethod);
                case "CONCAT_OP":
                    return ProcessConcatOperator(astNode, currentClass, currentMethod);
                case "EQ_OP":
                    return ProcessBinaryRelationalOperator(LuaXBinaryOperator.Equal, astNode, currentClass, currentMethod);
                case "NEQ_OP":
                    return ProcessBinaryRelationalOperator(LuaXBinaryOperator.NotEqual, astNode, currentClass, currentMethod);
                case "GT_OP":
                    return ProcessBinaryRelationalOperator(LuaXBinaryOperator.Greater, astNode, currentClass, currentMethod);
                case "GE_OP":
                    return ProcessBinaryRelationalOperator(LuaXBinaryOperator.GreaterOrEqual, astNode, currentClass, currentMethod);
                case "LT_OP":
                    return ProcessBinaryRelationalOperator(LuaXBinaryOperator.Less, astNode, currentClass, currentMethod);
                case "LE_OP":
                    return ProcessBinaryRelationalOperator(LuaXBinaryOperator.LessOrEqual, astNode, currentClass, currentMethod);
                case "NOT_OP":
                    return ProcessNotOperator(astNode, currentClass, currentMethod);
                case "AND_OP":
                    return ProcessBinaryLogicalOperator(LuaXBinaryOperator.And, astNode, currentClass, currentMethod);
                case "OR_OP":
                    return ProcessBinaryLogicalOperator(LuaXBinaryOperator.Or, astNode, currentClass, currentMethod);
                case "BRACKET_EXPR":
                    return ProcessBracket(astNode, currentClass, currentMethod);
                case "NEW_ARRAY_EXPR":
                    return ProcessNewArray(astNode, currentClass, currentMethod);
                case "NEW_TABLE_EXPR":
                    return ProcessNewTable(astNode);
                case "LOCAL_CALL":
                    return ProcessLocalCall(astNode, currentClass, currentMethod);
                case "METHOD_CALL":
                    return ProcessMethodCall(astNode, currentClass, currentMethod);
                default:
                    throw new LuaXAstGeneratorException(Name, astNode, $"Unexpected symbol {astNode.Symbol}");
            }
        }

        /// <summary>
        /// Process a binary math operator
        /// </summary>
        /// <param name="operator"></param>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessBinaryMathOperator(LuaXBinaryOperator @operator, IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count < 3)
                throw new LuaXAstGeneratorException(Name, astNode, "Two expressions are expected here");

            var arg1 = ProcessExpression(astNode.Children[0], currentClass, currentMethod);
            var arg2 = ProcessExpression(astNode.Children[2], currentClass, currentMethod);

            if (!arg1.ReturnType.IsNumeric())
                throw new LuaXAstGeneratorException(Name, astNode.Children[0], "A numeric expression is expected here");
            if (!arg2.ReturnType.IsNumeric())
                throw new LuaXAstGeneratorException(Name, astNode.Children[2], "A numeric expression is expected here");

            return new LuaXBinaryOperatorExpression(@operator, arg1, arg2,
                arg1.ReturnType.IsReal() || arg2.ReturnType.IsReal() ? LuaXTypeDefinition.Real : LuaXTypeDefinition.Integer,
                new LuaXElementLocation(Name, astNode.Children[1]));
        }

        /// <summary>
        /// Process a binary logical operator
        /// </summary>
        /// <param name="operator"></param>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessBinaryLogicalOperator(LuaXBinaryOperator @operator, IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count < 3)
                throw new LuaXAstGeneratorException(Name, astNode, "Two expressions are expected here");

            var arg1 = ProcessExpression(astNode.Children[0], currentClass, currentMethod);
            var arg2 = ProcessExpression(astNode.Children[2], currentClass, currentMethod);

            if (!arg1.ReturnType.IsBoolean())
                throw new LuaXAstGeneratorException(Name, astNode.Children[0], "A boolean expression is expected here");
            if (!arg2.ReturnType.IsBoolean())
                throw new LuaXAstGeneratorException(Name, astNode.Children[2], "A boolean expression is expected here");

            return new LuaXBinaryOperatorExpression(@operator, arg1, arg2, LuaXTypeDefinition.Boolean,
                new LuaXElementLocation(Name, astNode.Children[1]));
        }

        /// <summary>
        /// Process a binary relational operator
        /// </summary>
        /// <param name="operator"></param>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessBinaryRelationalOperator(LuaXBinaryOperator @operator, IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count < 3)
                throw new LuaXAstGeneratorException(Name, astNode, "Two expressions are expected here");
            var arg1 = ProcessExpression(astNode.Children[0], currentClass, currentMethod);
            var arg2 = ProcessExpression(astNode.Children[2], currentClass, currentMethod);

            //check type compatibility
            if (arg1.ReturnType.IsNumeric() && arg2.ReturnType.IsNumeric() ||
                arg1.ReturnType.IsString() && arg2.ReturnType.IsString() ||
                arg1.ReturnType.IsDate() && arg2.ReturnType.IsDate() ||
                arg1.ReturnType.IsBoolean() && arg2.ReturnType.IsBoolean())
            {
                return new LuaXBinaryOperatorExpression(@operator, arg1, arg2, LuaXTypeDefinition.Boolean,
                    new LuaXElementLocation(Name, astNode.Children[1]));
            }
            else
                throw new LuaXAstGeneratorException(Name, astNode, "The relational operators must have compatible type on both side");
        }

        /// <summary>
        /// Process a string concat operator
        /// </summary>
        /// <param name="node"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessConcatOperator(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count < 3)
                throw new LuaXAstGeneratorException(Name, astNode, "Two expressions are expected here");

            var arg1 = ProcessExpression(astNode.Children[0], currentClass, currentMethod);
            var arg2 = ProcessExpression(astNode.Children[2], currentClass, currentMethod);

            if (!arg1.ReturnType.IsString())
                arg1 = arg1.CastTo(LuaXTypeDefinition.String);
            if (!arg2.ReturnType.IsString())
                arg2 = arg2.CastTo(LuaXTypeDefinition.String);

            return new LuaXBinaryOperatorExpression(LuaXBinaryOperator.Concat, arg1, arg2, LuaXTypeDefinition.String,
                new LuaXElementLocation(Name, astNode.Children[1]));
        }

        /// <summary>
        /// Process an unary not operator
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessNotOperator(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count < 2)
                throw new LuaXAstGeneratorException(Name, astNode, "The expression is expected here");
            var arg = ProcessExpression(astNode.Children[1], currentClass, currentMethod);

            if (!arg.ReturnType.IsBoolean())
                throw new LuaXAstGeneratorException(Name, astNode, "The boolean expression is expected here");

            return new LuaXUnaryOperatorExpression(LuaXUnaryOperator.Not, arg, LuaXTypeDefinition.Boolean, new LuaXElementLocation(Name, astNode));
        }

        /// <summary>
        /// Process an unary negate operator
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessNegateOperator(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count < 2)
                throw new LuaXAstGeneratorException(Name, astNode, "The expression is expected here");
            var arg = ProcessExpression(astNode.Children[1], currentClass, currentMethod);

            if (!arg.ReturnType.IsNumeric())
                throw new LuaXAstGeneratorException(Name, astNode, "The numeric expression is expected here");

            return new LuaXUnaryOperatorExpression(LuaXUnaryOperator.Minus, arg, arg.ReturnType, new LuaXElementLocation(Name, astNode));
        }

        /// <summary>
        /// Processes array index operation
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessArrayAccess(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count != 4 ||
                astNode.Children[0].Symbol != "REXPR" ||
                astNode.Children[2].Symbol != "REXPR")
                throw new LuaXAstGeneratorException(Name, astNode, "Type and expression are expected");

            var arr = ProcessExpression(astNode.Children[0], currentClass, currentMethod);

            if (!arr.ReturnType.Array)
                throw new LuaXAstGeneratorException(Name, astNode, "[] operator can be applied to arrays only");

            var location = new LuaXElementLocation(Name, astNode);

            var index = ProcessExpression(astNode.Children[2], currentClass, currentMethod);
            if (index.ReturnType.TypeId != LuaXType.Integer || index.ReturnType.Array)
            {
                if (index.ReturnType.TypeId == LuaXType.Real && !index.ReturnType.Array)
                    index = new LuaXCastOperatorExpression(index, LuaXTypeDefinition.Integer, location);
                else
                    throw new LuaXAstGeneratorException(Name, astNode, "Index should be a numeric value");
            }
            return new LuaXArrayAccessExpression(arr, index, arr.ReturnType.ArrayElementType(), location);
        }

        /// <summary>
        /// Process cast operator
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessCast(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count != 7 ||
                astNode.Children[2].Symbol != "TYPE_NAME" ||
                astNode.Children[5].Symbol != "REXPR")
                throw new LuaXAstGeneratorException(Name, astNode, "Type and expression are expected");

            var decl = new AstNodeWrapper();
            decl.Add(astNode.Children[2]);
            var type = ProcessTypeDecl(decl, false);

            if (type.TypeId == LuaXType.Object && !Metadata.Search(type.Class, out _))
                throw new LuaXAstGeneratorException(Name, astNode.Children[2], $"The class {type.Class} is not defined");

            var arg = ProcessExpression(astNode.Children[5], currentClass, currentMethod);

            return new LuaXCastOperatorExpression(arg, type, new LuaXElementLocation(Name, astNode));
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
                    return new LuaXArrayLengthExpression(leftSide, LuaXTypeDefinition.Integer, location);
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
                    return new LuaXInstancePropertyExpression(leftSide, name, property.LuaType, location);
                }
                else
                    throw new LuaXAstGeneratorException(Name, astNode, "The left side argument of the property access expression is not a class or an array");
            }
        }

        /// <summary>
        /// Processes variable access
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessVariable(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
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

            if (currentMethod.Arguments.Search(name, out var v1))
                return new LuaXArgumentExpression(name, v1.LuaType, location);
            if (currentMethod.Variables .Search(name, out var v2))
                return new LuaXVariableExpression(name, v2.LuaType, location);
            if (currentClass.Properties.Search(name, out var p1))
            {
                if (p1.Static)
                    return new LuaXStaticPropertyExpression(currentClass.Name, name, p1.LuaType, location);
                else
                {
                    if (currentMethod.Static)
                        throw new LuaXAstGeneratorException(Name, astNode, $"Can't access instance property {name} in a static method");

                    return new LuaXInstancePropertyExpression(new LuaXVariableExpression("this", new LuaXTypeDefinition() { TypeId = LuaXType.Object, Class = currentClass.Name }, location),
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
        private bool IsPassTroughExpression(IAstNode astNode)
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
                case "CALL":
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
        private LuaXExpression ProcessConstantExpression(IAstNode astNode) => new LuaXConstantExpression(ProcessConstant(astNode));

        /// <summary>
        /// Processes a new object/table operation
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessNewTable(IAstNode astNode)
        {
            if (astNode.Children.Count < 2 || astNode.Children[1].Symbol != "IDENTIFIER")
                throw new LuaXAstGeneratorException(Name, astNode, "The identifier is expected");

            var @class = astNode.Children[1].Value;
            if (!Metadata.Search(@class, out _))
                throw new LuaXAstGeneratorException(Name, astNode, $"The class {@class} is not found");

            return new LuaXNewObjectExpression(@class, new LuaXElementLocation(Name, astNode));
        }

        /// <summary>
        /// Process new array operator
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessNewArray(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count < 2 || astNode.Children[1].Symbol != "TYPE_NAME")
                throw new LuaXAstGeneratorException(Name, astNode, "The type is expected");
            if (astNode.Children.Count < 4 || astNode.Children[3].Symbol != "REXPR")
                throw new LuaXAstGeneratorException(Name, astNode, "The expression is expected");

            var size = ProcessExpression(astNode.Children[3], currentClass, currentMethod);
            if (!size.ReturnType.IsNumeric())
                throw new LuaXAstGeneratorException(Name, astNode, "The expression should be a numeric expression");

            if (!size.ReturnType.IsInteger())
                size = size.CastTo(LuaXTypeDefinition.Integer);

            var decl = new AstNodeWrapper();
            decl.Add(astNode.Children[1]);
            var type = ProcessTypeDecl(decl, false);

            if (type.TypeId == LuaXType.Object && !Metadata.Search(type.Class, out _))
                throw new LuaXAstGeneratorException(Name, astNode.Children[3], $"The class {type.Class} is not defined");

            return new LuaXNewArrayExpression(type, size, new LuaXElementLocation(Name, astNode));
        }

        /// <summary>
        /// Processes a local call
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="callNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessLocalCall(IAstNode callNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            string identifier = null;
            for (int i = 0; i < callNode.Children.Count; i++)
            {
                var child = callNode.Children[i];
                if (child.Symbol == "IDENTIFIER")
                    identifier = child.Value;
            }

            if (identifier == null)
                throw new LuaXAstGeneratorException(Name, callNode, "The identifier is expected");

            if (!currentClass.Methods.Search(identifier, out var method))
                throw new LuaXAstGeneratorException(Name, callNode, $"There is no method {identifier} in the class {currentClass.Name}");

            if (method.Static)
                return ProcessCall(new LuaXClassNameExpression(currentClass.Name, new LuaXElementLocation(Name, callNode)), callNode, currentClass, currentMethod);
            else
                return ProcessCall(new LuaXVariableExpression("this", new LuaXTypeDefinition() { TypeId = LuaXType.Object, Class = currentClass.Name }, new LuaXElementLocation(Name, callNode)), callNode, currentClass, currentMethod);
        }

        private LuaXExpression ProcessMethodCall(IAstNode callNode, LuaXClass currentClass, LuaXMethod currentMethod)
            => ProcessCall(ProcessExpression(callNode.Children[0], currentClass, currentMethod), callNode, currentClass, currentMethod);

        /// <summary>
        /// Processes call expression (method name and arguments)
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="callNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessCall(LuaXExpression subject, IAstNode callNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            string identifier = null;
            IReadOnlyList<IAstNode> args = null;
            for (int i = 0; i < callNode.Children.Count; i++)
            {
                var child = callNode.Children[i];
                if (child.Symbol == "IDENTIFIER")
                    identifier = child.Value;
                if (child.Symbol == "CALL_BRACKET" && child.Children.Count > 2 && child.Children[1].Symbol == "CALL_ARGS")
                    args = child.Children[1].Children;
            }

            if (identifier == null)
                throw new LuaXAstGeneratorException(Name, callNode, "The identifier is expected");

            LuaXCallExpression callExpression;
            LuaXVariableCollection methodArguments;

            if (subject.ReturnType.TypeId == LuaXType.ClassName)
            {
                Metadata.Search(subject.ReturnType.Class, out var @class);
                if (!@class.Methods.Search(identifier, out var @method))
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is not found");
                if (!method.Static)
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is not a static method");
                callExpression = new LuaXStaticCallExpression(method.ReturnType, subject.ReturnType.Class, identifier, new LuaXElementLocation(Name, callNode));
                methodArguments = method.Arguments;
            }
            else if (subject.ReturnType.TypeId == LuaXType.Object)
            {
                Metadata.Search(subject.ReturnType.Class, out var @class);
                if (!@class.Methods.Search(identifier, out var @method))
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is not found");
                if (method.Static)
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is a static method");
                callExpression = new LuaXInstanceCallExpression(method.ReturnType, subject, identifier, new LuaXElementLocation(Name, callNode));
                methodArguments = method.Arguments;
            }
            else
                throw new LuaXAstGeneratorException(Name, new LuaXParserError(subject.Location, "The class name or an object expression is expected here"));

            if ((args?.Count ?? 0) != methodArguments.Count)
                throw new LuaXAstGeneratorException(Name, callNode, $"The method {identifier} expects {methodArguments.Count} argument(s), but {args?.Count ?? 0} is provided");

            if (args != null)
            {
                for (int i = 0; i < args.Count; i++)
                {
                    var argExpression = ProcessExpression(args[i], currentClass, currentMethod);
                    if (!methodArguments[i].LuaType.Equals(argExpression.ReturnType))
                    {
                        var argExpression1 = CastToCompatible(argExpression, methodArguments[i].LuaType);
                        argExpression = argExpression1 ?? throw new LuaXAstGeneratorException(Name, callNode, $"The type of the {i + 1}th argument ({methodArguments[i].Name}) is not compatible with the expected type ({methodArguments[i].LuaType} is expected, {argExpression.ReturnType} is provided)");
                    }
                    callExpression.Arguments.Add(argExpression);
                }
            }
            return callExpression;
        }
    }
}
