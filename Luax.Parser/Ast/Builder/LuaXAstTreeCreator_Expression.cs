using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
                case "TYPENAME_OP":
                    return ProcessTypename(astNode, currentClass, currentMethod);
                case "ARRAY_ACCESS":
                    return ProcessArrayAccess(astNode, currentClass, currentMethod);
                case "ARRAY_ELEMENT":
                    return ProcessArrayElement(astNode, currentClass, currentMethod);
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
                case "NEW_ARRAY_EXPR_WITH_INIT":
                    return ProcessNewArrayWithInit(astNode, currentClass, currentMethod);
                case "NEW_TABLE_EXPR":
                    return ProcessNewTable(astNode, currentClass);
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

            var t1 = arg1.ReturnType;
            var t2 = arg2.ReturnType;
            var n1 = arg1 is LuaXConstantExpression c1 && c1.Value.IsNil;
            var n2 = arg2 is LuaXConstantExpression c2 && c2.Value.IsNil;

            //check type compatibility
            if (CanCompare(t1, n1, t2, n2))
                return new LuaXBinaryOperatorExpression(@operator, arg1, arg2, LuaXTypeDefinition.Boolean,
                    new LuaXElementLocation(Name, astNode.Children[1]));
            else
                throw new LuaXAstGeneratorException(Name, astNode, "The relational operators must have compatible type on both side");
        }

        private bool CanCompare(LuaXTypeDefinition t1, bool nil1, LuaXTypeDefinition t2, bool nil2)
         => t1.IsNumeric() && t2.IsNumeric() ||  //numbers to numbers
            t1.IsBoolean() && t2.IsBoolean() ||  //bools to bools
            t1.IsDate() && t2.IsDate() ||        //dates to dates
            t1.IsString() && t2.IsString() ||    //strings to strings
                                                 //nil to arrays, objects or strings
            nil1 && (t2.Array || t2.IsObject() || t2.IsString()) ||
            nil2 && (t1.Array || t1.IsObject() || t1.IsString()) ||
            //objects if the classes from the same chain of hierarchy
            t1.IsObject() && t2.IsObject() && (t1.Class == t2.Class || Metadata.IsKindOf(t1.Class, t2.Class) || Metadata.IsKindOf(t2.Class, t1.Class));

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
                (astNode.Children[0].Symbol != "REXPR" && astNode.Children[0].Symbol != "CALLABLE_EXPR") ||
                 astNode.Children[2].Symbol != "REXPR")
                throw new LuaXAstGeneratorException(Name, astNode, "Type and expression are expected");

            var arr = ProcessExpression(astNode.Children[0], currentClass, currentMethod);
            var index = ProcessExpression(astNode.Children[2], currentClass, currentMethod);
            var location = new LuaXElementLocation(Name, astNode);

            if (arr.ReturnType.Array)
            {
                if (index.ReturnType.TypeId != LuaXType.Integer || index.ReturnType.Array)
                {
                    if (index.ReturnType.TypeId == LuaXType.Real && !index.ReturnType.Array)
                        index = new LuaXCastOperatorExpression(index, LuaXTypeDefinition.Integer, location);
                    else
                        throw new LuaXAstGeneratorException(Name, astNode, "Index should be a numeric value");
                }
                return new LuaXArrayAccessExpression(arr, index, arr.ReturnType.ArrayElementType(), location);
            }
            else if (arr.ReturnType.IsObject())
            {
                if (!HasGetMethod(arr.ReturnType.Class, index.ReturnType, currentClass, out var method))
                    throw new LuaXAstGeneratorException(Name, astNode, $"The class {arr.ReturnType.Class} does not have a public instance method get({index.ReturnType})");
                var call = new LuaXInstanceCallExpression(method.ReturnType, arr, "get", null, location);
                call.Arguments.Add(index);
                return call;
            }
            throw new LuaXAstGeneratorException(Name, astNode, "[] operator can be applied to arrays only");
        }

        private bool SearchClassByName(string className, LuaXClass currentClass, out LuaXClass @class)
        {
#pragma warning disable S2692 // "IndexOf" checks should not be for positive numbers: NG: false positive!
            if (className.IndexOf('.') > 0)
                return Metadata.Search(className, out @class);
#pragma warning restore S2692 // 
            string ownerName = currentClass.Name;
            string name = $"{ownerName}.{className}";
            while (name.Length > 0)
            {
                if (Metadata.Search(name, out @class))
                    return true;
                if (ownerName.Length == 0)
                    break;
                int index = ownerName.LastIndexOf('.');
                if (index < 0)
                {
                    ownerName = "";
                    name = className;
                }
                else
                {
                    ownerName = ownerName.Substring(0, index);
                    name = $"{ownerName}.{className}";
                }
            }
            @class = null;
            return false;
        }

        private bool HasGetMethod(string className, LuaXTypeDefinition returnType, LuaXClass currentClass, out LuaXMethod method)
        {
            method = null;
            return SearchClassByName(className, currentClass, out var @class) &&
                    @class.SearchMethod("get", out method, true) &&
                     method.Arguments.Count == 1 &&
                     !method.Static &&
                     method.Visibility != LuaXVisibility.Private &&
                     method.Arguments[0].LuaType.IsTheSame(returnType);
        }

        /// <summary>
        /// Processes array index operation on the left size of assign expression
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessArrayElement(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
            => ProcessArrayAccess(astNode, currentClass, currentMethod);

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

            if (type.TypeId == LuaXType.Object)
                if (!SearchClassByName(type.Class, currentClass, out var realClass))
                    throw new LuaXAstGeneratorException(Name, astNode.Children[2], $"The class {type.Class} is not defined");
                else
                    type = new LuaXTypeDefinition()
                    {
                        TypeId = type.TypeId,
                        Array = type.Array,
                        Class = realClass.Name
                    };

            var arg = ProcessExpression(astNode.Children[5], currentClass, currentMethod);

            //check cast compatibility
            if (arg.ReturnType.IsTheSame(type))
                return arg;

            var customCast = FindCustomCast(arg, type);
            return customCast ?? new LuaXCastOperatorExpression(arg, type, new LuaXElementLocation(Name, astNode));
        }

        /// <summary>
        /// Process cast operator
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessTypename(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count != 4 ||
                astNode.Children[2].Symbol != "REXPR")
                throw new LuaXAstGeneratorException(Name, astNode, "Expression are expected");

            var arg = ProcessExpression(astNode.Children[2], currentClass, currentMethod);

            return new LuaXTypeNameOperatorExpression(arg, new LuaXElementLocation(Name, astNode));
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
                return ProcessStaticProperty(astNode, currentClass, classNameExpression, name, location);
            else
                return ProcessInstanceProperty(astNode, currentClass, leftSide, name, location);
        }

        private LuaXExpression ProcessStaticProperty(IAstNode astNode, LuaXClass currentClass, LuaXClassNameExpression classNameExpression, string name, LuaXElementLocation location)
        {
            if (!SearchClassByName(classNameExpression.Name, currentClass, out var leftSideClass))
                throw new LuaXAstGeneratorException(Name, astNode, $"Class {classNameExpression.Name} is not found in metadata");
            if (leftSideClass.SearchProperty(name, out var property, out string ownerClassName))
            {
                if (!property.Static)
                    throw new LuaXAstGeneratorException(Name, astNode, $"Property {classNameExpression.Name}.{name} is not static");
                if (property.Visibility == LuaXVisibility.Private && classNameExpression.ReturnType.Class != currentClass.Name)
                    throw new LuaXAstGeneratorException(Name, astNode, $"Property {classNameExpression.Name}.{name} is private and cannot be accessed");

                return new LuaXStaticPropertyExpression(ownerClassName, name, property.LuaType, location);
            }
            else if (leftSideClass.SearchConstant(name, out var constant))
                return new LuaXConstantExpression(constant.Value, location);

            throw new LuaXAstGeneratorException(Name, astNode, $"Class {classNameExpression.Name} does not contain property {name}");
        }

        private LuaXExpression ProcessInstanceProperty(IAstNode astNode, LuaXClass currentClass, LuaXExpression leftSide, string name, LuaXElementLocation location)
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
                if (!SearchClassByName(leftSideType.Class, currentClass, out var leftSideClass))
                    throw new LuaXAstGeneratorException(Name, astNode, $"Class {leftSideType.Class} is not found in metadata");
                if (!leftSideClass.SearchProperty(name, out var property, out var _, true))
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

        /// <summary>
        /// Processes variable access
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LuaXExpression ProcessVariable(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count != 1 || astNode.Children[0].Symbol != "IDENTIFIER")
                throw new LuaXAstGeneratorException(Name, astNode, "Identifier is expected here");

            var location = new LuaXElementLocation(Name, astNode);

            var name = astNode.Children[0].Value;

            if (IsThisReference(name, currentMethod))
                return new LuaXVariableExpression("this",
                    new LuaXTypeDefinition
                    {
                        TypeId = LuaXType.Object,
                        Class = currentClass.Name
                    }, location);

            if (IsSuperRefenence(name, currentClass, currentMethod))
                return new LuaXVariableExpression("super",
                    new LuaXTypeDefinition
                    {
                        TypeId = LuaXType.Object,
                        Class = currentClass.ParentClass.Name,
                    }, location);

            if (currentMethod.Arguments.Search(name, out var v1))
                return new LuaXArgumentExpression(name, v1.LuaType, location);
            if (currentMethod.Variables.Search(name, out var v2))
                return new LuaXVariableExpression(name, v2.LuaType, location);
            if (currentMethod.Constants.Search(name, out var c1))
                return new LuaXConstantExpression(c1.Value, location);

            if (currentClass.SearchProperty(name, out var p1, out string ownerClassName))
                return ProcessVariableAsProperty(astNode, ownerClassName, currentMethod, name, p1, location);

            if (currentClass.SearchConstant(name, out var c2))
                return new LuaXConstantExpression(c2.Value, location);

            if (SearchClassByName(name, currentClass, out var realClass))
                return new LuaXClassNameExpression(realClass.Name, location);

            throw new LuaXAstGeneratorException(Name, astNode, $"Identifier {name} is not an argument, property, method or class name");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsThisReference(string name, LuaXMethod currentMethod)
            => !currentMethod.Static && name == "this";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSuperRefenence(string name, LuaXClass currentClass, LuaXMethod currentMethod)
            => !currentMethod.Static && currentClass.HasParent && name == "super";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LuaXExpression ProcessVariableAsProperty(IAstNode astNode, string ownerClassName, LuaXMethod currentMethod, string name, LuaXProperty p1, LuaXElementLocation location)
        {
            if (p1.Static)
                return new LuaXStaticPropertyExpression(ownerClassName, name, p1.LuaType, location);
            else
            {
                if (currentMethod.Static)
                    throw new LuaXAstGeneratorException(Name, astNode, $"Can't access instance property {name} in a static method");

                return new LuaXInstancePropertyExpression(new LuaXVariableExpression("this", new LuaXTypeDefinition { TypeId = LuaXType.Object, Class = ownerClassName }, location),
                    name, p1.LuaType, location);
            }
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
        /// <returns></returns>
        private LuaXExpression ProcessNewTable(IAstNode astNode, LuaXClass currentClass)
        {
            if (astNode.Children.Count < 2 || astNode.Children[1].Symbol != "IDENTIFIER")
                throw new LuaXAstGeneratorException(Name, astNode, "The identifier is expected");

            var @class = astNode.Children[1].Value;
            if (!SearchClassByName(@class, currentClass, out var realClass))
                throw new LuaXAstGeneratorException(Name, astNode, $"The class {@class} is not found");

            return new LuaXNewObjectExpression(realClass.Name, new LuaXElementLocation(Name, astNode));
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

            if (type.TypeId == LuaXType.Object)
                if (!SearchClassByName(type.Class, currentClass, out var realClass))
                    throw new LuaXAstGeneratorException(Name, astNode.Children[3], $"The class {type.Class} is not defined");
                else
                    type = new LuaXTypeDefinition()
                    {
                        TypeId = type.TypeId,
                        Array = type.Array,
                        Class = realClass.Name
                    };

            return new LuaXNewArrayExpression(type, size, new LuaXElementLocation(Name, astNode));
        }

        /// <summary>
        /// Process new array operator with initialization
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpression ProcessNewArrayWithInit(IAstNode astNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            if (astNode.Children.Count < 2 || astNode.Children[1].Symbol != "TYPE_NAME")
                throw new LuaXAstGeneratorException(Name, astNode, "The type is expected");

            AstNodeWrapper decl = new AstNodeWrapper();
            decl.Add(astNode.Children[1]);
            LuaXTypeDefinition type = ProcessTypeDecl(decl, false);

            if (type.TypeId == LuaXType.Object)
                if (!SearchClassByName(type.Class, currentClass, out var arrayItemClass))
                    throw new LuaXAstGeneratorException(Name, astNode.Children[3], $"The class {type.Class} is not defined");
                else
                    type = new LuaXTypeDefinition()
                    {
                        TypeId = type.TypeId,
                        Array = type.Array,
                        Class = arrayItemClass.Name
                    };

            if (astNode.Children.Count < 5 || astNode.Children[4].Symbol != "ARRAY_INIT")
                throw new LuaXAstGeneratorException(Name, astNode, "The array initialization is expected");

            LuaXExpressionCollection initExpressions =
                GetArrayInitializationExpressions(astNode, type, currentClass, currentMethod);
            return new LuaXNewArrayWithInitExpression(type, initExpressions, new LuaXElementLocation(Name, astNode));
        }

        /// <summary>
        /// Process array initialization expressions
        /// </summary>
        /// <param name="astNode">ARRAY_INIT AST node</param>
        /// <param name="targetType"></param>
        /// <param name="currentClass"></param>
        /// <param name="currentMethod"></param>
        /// <returns></returns>
        private LuaXExpressionCollection GetArrayInitializationExpressions(IAstNode astNode, LuaXTypeDefinition targetType, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            LuaXExpressionCollection initExpressions = new LuaXExpressionCollection();
            IAstNode arrayInitAstNode = astNode.Children[4];
            if (arrayInitAstNode.Children[1].Symbol == "ARRAY_INIT_ARGS")
            {
                arrayInitAstNode = arrayInitAstNode.Children[1];
                for (var i = 0; i < arrayInitAstNode.Children.Count; i++)
                {
                    IAstNode expressionAstNode = arrayInitAstNode.Children[i];
                    LuaXExpression initExpression = CastToCompatible(ProcessExpression(expressionAstNode, currentClass, currentMethod), targetType);
                    if (initExpression == null)
                        throw new LuaXAstGeneratorException(Name, expressionAstNode, "The initialization expression should have a compatible type");
                    initExpressions.Add(initExpression);
                }
            }
            return initExpressions;
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

            if (!currentClass.SearchMethod(identifier, out var method))
                throw new LuaXAstGeneratorException(Name, callNode, $"There is no method {identifier} in the class {currentClass.Name}");

            if (method.Static)
                return ProcessCall(new LuaXClassNameExpression(method.Class.Name, new LuaXElementLocation(Name, callNode)), callNode, currentClass, currentMethod);
            else
                return ProcessCall(new LuaXVariableExpression("this", new LuaXTypeDefinition { TypeId = LuaXType.Object, Class = method.Class.Name }, new LuaXElementLocation(Name, callNode)), callNode, currentClass, currentMethod);
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
                callExpression = ProcessStaticCall(subject, currentClass, identifier, callNode, out methodArguments);
            else if (subject.ReturnType.TypeId == LuaXType.Object)
                callExpression = ProcessInstanceCall(subject, currentClass, identifier, callNode, out methodArguments);
            else
                throw new LuaXAstGeneratorException(Name, new LuaXParserError(subject.Location, "The class name or an object expression is expected here"));

            if ((args?.Count ?? 0) != methodArguments.Count)
                throw new LuaXAstGeneratorException(Name, callNode, $"The method {identifier} expects {methodArguments.Count} argument(s), but {args?.Count ?? 0} is provided");

            if (args != null)
                ProcessCallArgs(callExpression, args, methodArguments, callNode, currentClass, currentMethod);

            return callExpression;
        }

        private LuaXCallExpression ProcessStaticCall(LuaXExpression subject, LuaXClass currentClass, string identifier, IAstNode callNode, out LuaXVariableCollection methodArguments)
        {
            if (SearchClassByName(subject.ReturnType.Class, currentClass, out var @class))
            {
                if (!@class.SearchMethod(identifier, out var @method))
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is not found");
                if (!method.Static)
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is not a static method");
                if (method.Visibility == LuaXVisibility.Private && !currentClass.HasInParents(@class) && !currentClass.HasInOwners(@class))
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is a private method");

                methodArguments = method.Arguments;
                return new LuaXStaticCallExpression(method.ReturnType, subject.ReturnType.Class, identifier, new LuaXElementLocation(Name, callNode));
            }
            throw new LuaXAstGeneratorException(Name, callNode, $"Type {subject.ReturnType.Class} is not defined");
        }

        private LuaXCallExpression ProcessInstanceCall(LuaXExpression subject, LuaXClass currentClass, string identifier, IAstNode callNode, out LuaXVariableCollection methodArguments)
        {
            string exactClass = null;
            if (subject is LuaXVariableExpression ve && ve.VariableName == "super")
                exactClass = subject.ReturnType.Class;

            if (SearchClassByName(subject.ReturnType.Class, currentClass, out var @class))
            {
                if (!@class.SearchMethod(identifier, out var @method, true))
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is not found");
                if (method.Static)
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is a static method");
                if (method.Visibility == LuaXVisibility.Private && !currentClass.HasInParents(@class) && !currentClass.HasInOwners(@class))
                    throw new LuaXAstGeneratorException(Name, callNode, $"Method {@class.Name}.{identifier} is a private method");

                methodArguments = method.Arguments;
                return new LuaXInstanceCallExpression(method.ReturnType, subject, identifier, exactClass, new LuaXElementLocation(Name, callNode));
            }
            throw new LuaXAstGeneratorException(Name, callNode, $"Type {subject.ReturnType.Class} is not defined");
        }

        private void ProcessCallArgs(LuaXCallExpression callExpression, IReadOnlyList<IAstNode> args, LuaXVariableCollection methodArguments, IAstNode callNode, LuaXClass currentClass, LuaXMethod currentMethod)
        {
            for (int i = 0; i < args.Count; i++)
            {
                var argExpression = ProcessExpression(args[i], currentClass, currentMethod);
                if (!methodArguments[i].LuaType.IsTheSame(argExpression.ReturnType))
                {
                    var argExpression1 = CastToCompatible(argExpression, methodArguments[i].LuaType);
                    argExpression = argExpression1 ?? throw new LuaXAstGeneratorException(Name, callNode, $"The type of the {i + 1}th argument ({methodArguments[i].Name}) is not compatible with the expected type ({methodArguments[i].LuaType} is expected, {argExpression.ReturnType} is provided)");
                }
                callExpression.Arguments.Add(argExpression);
            }
        }
    }
}
