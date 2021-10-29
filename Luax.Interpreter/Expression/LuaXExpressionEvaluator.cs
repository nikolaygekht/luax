﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Luax.Interpreter.Infrastructure;
using Luax.Parser.Ast;
using Luax.Parser.Ast.LuaExpression;
using LinqExpression = System.Linq.Expressions.Expression;

namespace Luax.Interpreter.Expression
{
    /// <summary>
    /// Expression evaluator
    /// </summary>
    public static class LuaXExpressionEvaluator
    {
        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <param name="types">The type library</param>
        /// <param name="runningClass">The instance of the currently running class</param>
        /// <param name="variables">The variables of the currently running method</param>
        /// <returns></returns>
        public static object Evaluate(LuaXExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            if (expression is LuaXConstantExpression constantExpression)
                return EvaluateConstant(constantExpression);
            if (expression is LuaXArgumentExpression argumentExpression)
                return EvaluateVariable(expression, argumentExpression.ArgumentName, variables);
            if (expression is LuaXVariableExpression variableExpression)
                return EvaluateVariable(expression, variableExpression.VariableName, variables);
            if (expression is LuaXStaticPropertyExpression staticPropertyExpression)
                return EvaluateStaticProperty(staticPropertyExpression, types);
            if (expression is LuaXInstancePropertyExpression instancePropertyExpression)
                return EvaluateInstanceProperty(instancePropertyExpression, types, runningClass, variables);
            if (expression is LuaXNewObjectExpression newObjectExpression)
                return EvaluateNewObject(newObjectExpression, types);
            if (expression is LuaXNewArrayExpression newArrayExpression)
                return EvaluateNewArray(newArrayExpression, types, runningClass, variables);
            if (expression is LuaXArrayAccessExpression arrayAccessExpression)
                return EvaluateArrayAccess(arrayAccessExpression, types, runningClass, variables);
            if (expression is LuaXCastOperatorExpression castOperatorExpression)
                return EvaluateCast(castOperatorExpression, types, runningClass, variables);
            if (expression is LuaXUnaryOperatorExpression unaryExpression)
                return EvaluateUnary(unaryExpression, types, runningClass, variables);
            if (expression is LuaXBinaryOperatorExpression binaryExpression)
                return EvaluateBinary(binaryExpression, types, runningClass, variables);
            if (expression is LuaXArrayLengthExpression arrayLengthExpression)
                return EvaluateArrayLength(arrayLengthExpression, types, runningClass, variables);
            throw new LuaXAstGeneratorException(expression.Location, $"Unexpected expression type {expression.GetType().Name}");
        }

        private static object EvaluateCast(LuaXCastOperatorExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var argument = Evaluate(expression.Argument, types, runningClass, variables);
            if (!types.CastTo(expression.ReturnType, ref argument))
                throw new LuaXAstGeneratorException(expression.Location, $"Cast to {expression.ReturnType} is not supported");
            return argument;
        }

        private static object EvaluateUnary(LuaXUnaryOperatorExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var argument = Evaluate(expression.Argument, types, runningClass, variables);
            if (expression.Operator == LuaXUnaryOperator.Minus)
            {
                if (argument is int i)
                    return -i;
                if (argument is double d)
                    return -d;
                throw new LuaXAstGeneratorException(expression.Location, "Numeric argument is required for -");
            }
            else if (expression.Operator == LuaXUnaryOperator.Not)
            {
                if (argument is bool b)
                    return !b;
                throw new LuaXAstGeneratorException(expression.Location, "Boolean argument is required for not");
            }
            throw new LuaXAstGeneratorException(expression.Location, "Unknown operator");
        }

        private static object MathOperation(LuaXExpression expression, object left, object right, Func<int, int, int> integerAction, Func<double, double, double> doubleAction)
        {
            if (left is int i1 && right is int i2)
                return integerAction(i1, i2);
            else
            {
                double f1, f2;
                if (left is int i3)
                    f1 = i3;
                else if (left is double d1)
                    f1 = d1;
                else
                    throw new LuaXAstGeneratorException(expression.Location, "Left argument of math operation is not a number");

                if (right is int i4)
                    f2 = i4;
                else if (right is double d2)
                    f2 = d2;
                else
                    throw new LuaXAstGeneratorException(expression.Location, "Left argument of math operation is not a number");

                return doubleAction(f1, f2);
            }
        }

        private static object LogicalOperation(LuaXExpression expression, object left, object right, Func<bool, bool, bool> action)
        {
            if (left is bool i1 && right is bool i2)
                return action(i1, i2);
            throw new LuaXAstGeneratorException(expression.Location, "Both argument of the operation must be boolean");
        }

        private static object RelationalOperation(LuaXExpression expression, LuaXBinaryOperator @operator, object left, object right)
        {
            int? sign = null;
            bool canRelate = false;

            if ((left is int || left is double) &&
                (right is int || right is double))
            {
                var r = MathOperation(expression, left, right, (a, b) => a - b, (a, b) => a - b);
                if (r is int ir)
                {
                    if (ir == 0)
                        sign = 0;
                    else if (ir < 0)
                        sign = -1;
                    else
                        sign = 1;
                }
                else if (r is double dr)
                {
                    if (dr == 0)
                        sign = 0;
                    else if (dr < 0)
                        sign = -1;
                    else
                        sign = 1;
                }
                canRelate = true;
            }
            else if (left is bool b1 && right is bool b2)
            {
                sign = b1 == b2 ? 0 : 1;
                canRelate = false;
            }
            else if (left == null || right == null || left is LuaXObjectInstance || right is LuaXVariableInstanceArray)
            {
                sign = ReferenceEquals(left, right) ? 0 : 1;
                canRelate = false;
            }
            else if (left is string s1 && right is string s2)
            {
                sign = string.CompareOrdinal(s1, s2);
                canRelate = true;
            }
            else if (left is DateTime d1 && right is DateTime d2)
            {
                var l = d1.Ticks - d2.Ticks;
                if (l == 0)
                    sign = 0;
                else if (l < 0)
                    sign = -1;
                else
                    sign = 1;
                canRelate = true;
            }

            if (sign == null || (!canRelate && @operator != LuaXBinaryOperator.Equal && @operator != LuaXBinaryOperator.NotEqual))
                throw new LuaXAstGeneratorException(expression.Location, "An attempt to compare incompatible types");

            return @operator switch
            {
                LuaXBinaryOperator.Equal => sign == 0,
                LuaXBinaryOperator.NotEqual => sign != 0,
                LuaXBinaryOperator.Greater => sign > 0,
                LuaXBinaryOperator.Less => sign < 0,
                LuaXBinaryOperator.GreaterOrEqual => sign >= 0,
                LuaXBinaryOperator.LessOrEqual => sign <= 0,
                _ => throw new LuaXAstGeneratorException(expression.Location, "Unknown relational operator"),
            };
        }

        private static object EvaluateBinary(LuaXBinaryOperatorExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var left = Evaluate(expression.LeftArgument, types, runningClass, variables);
            var right = Evaluate(expression.RightArgument, types, runningClass, variables);

            if (expression.Operator == LuaXBinaryOperator.Concat)
            {
                if (left is not string)
                    types.CastTo(LuaXTypeDefinition.String, ref left);
                if (right is not string)
                    types.CastTo(LuaXTypeDefinition.String, ref right);

                return (string)left + (string)right;
            }

            return expression.Operator switch
            {
                LuaXBinaryOperator.Add => MathOperation(expression, left, right, (a1, a2) => a1 + a2, (a1, a2) => a1 + a2),
                LuaXBinaryOperator.Subtract => MathOperation(expression, left, right, (a1, a2) => a1 - a2, (a1, a2) => a1 - a2),
                LuaXBinaryOperator.Multiply => MathOperation(expression, left, right, (a1, a2) => a1 * a2, (a1, a2) => a1 * a2),
                LuaXBinaryOperator.Divide => MathOperation(expression, left, right, (a1, a2) => a1 / a2, (a1, a2) => a1 / a2),
                LuaXBinaryOperator.Reminder => MathOperation(expression, left, right, (a1, a2) => a1 % a2, (_, _) => 0.0),
                LuaXBinaryOperator.Power => MathOperation(expression, left, right, (a1, a2) => (int)Math.Pow(a1, a2), (a1, a2) => Math.Pow(a1, a2)),
                LuaXBinaryOperator.And => LogicalOperation(expression, left, right, (a1, a2) => a1 && a2),
                LuaXBinaryOperator.Or => LogicalOperation(expression, left, right, (a1, a2) => a1 || a2),
                LuaXBinaryOperator.Equal or LuaXBinaryOperator.NotEqual or LuaXBinaryOperator.Less or LuaXBinaryOperator.LessOrEqual or LuaXBinaryOperator.Greater or LuaXBinaryOperator.GreaterOrEqual => RelationalOperation(expression, expression.Operator, left, right),
                _ => throw new LuaXAstGeneratorException(expression.Location, "The index is out of range"),
            };
        }

        /// <summary>
        /// Evaluates access to an array item
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="types"></param>
        /// <param name="runningClass"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static object EvaluateArrayAccess(LuaXArrayAccessExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            object _array = Evaluate(expression.ArrayExpression, types, runningClass, variables);

            if (_array is not LuaXVariableInstanceArray array)
                throw new LuaXAstGeneratorException(expression.Location, "The array expression does not return an array value");

            int index;
            object _index = Evaluate(expression.IndexExpression, types, runningClass, variables);
            if (_index is int i)
                index = i;
            else if (_index is double d)
                index = (int)d;
            else
                throw new LuaXAstGeneratorException(expression.Location, "The size expression is not an numeric value");

            if (index < 0)
                index = array.Length + index;

            if (index < 0 || index >= array.Length)
                throw new LuaXAstGeneratorException(expression.Location, "The index is out of range");

            return array[index].Value;
        }

        private static object EvaluateArrayLength(LuaXArrayLengthExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var array = Evaluate(expression.ArrayExpression, types, runningClass, variables);
            if (array is not LuaXVariableInstanceArray a)
                throw new LuaXAstGeneratorException(expression.Location, $"The value is not an array or array is not initialized");
            return a.Length;
        }

        /// <summary>
        /// Evaluates a new array expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="types"></param>
        /// <param name="runningClass"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static object EvaluateNewArray(LuaXNewArrayExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            int size;
            object _size = Evaluate(expression.SizeExpression, types, runningClass, variables);
            if (_size is int i)
                size = i;
            else if (_size is double d)
                size = (int)d;
            else
                throw new LuaXAstGeneratorException(expression.Location, "The size expression is not an numeric value");

            return new LuaXVariableInstanceArray(expression.ReturnType, size);
        }

        /// <summary>
        /// Evaluates a new object expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="types"></param>
        /// <param name="runningClass"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static object EvaluateNewObject(LuaXNewObjectExpression expression, LuaXTypesLibrary types)
        {
            if (!types.SearchClass(expression.ClassName, out var @class))
                throw new LuaXAstGeneratorException(expression.Location, $"Class {expression.ClassName} is not found");

            return new LuaXObjectInstance(@class);
        }

        /// <summary>
        /// Evaluate an instance property access
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="types"></param>
        /// <param name="runningClass"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static object EvaluateInstanceProperty(LuaXInstancePropertyExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            object _v = Evaluate(expression.Object, types, runningClass, variables);
            if (_v is not LuaXObjectInstance v)
                throw new LuaXAstGeneratorException(expression.Location, "The expression is not a class instance expression");

            var p = v.Properties[expression.PropertyName];

            if (p == null)
                throw new LuaXAstGeneratorException(expression.Location, $"Instance property {v.Class.LuaType.Name}.{expression.PropertyName} is not found");

            return p.Value;
        }

        /// <summary>
        /// Evaluate a static property access
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="runningClass"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static object EvaluateStaticProperty(LuaXStaticPropertyExpression expression, LuaXTypesLibrary types)
        {
            if (!types.SearchClass(expression.ClassName, out var @class))
                throw new LuaXAstGeneratorException(expression.Location, $"Class {expression.ClassName} is not found");

            var p = @class.StaticProperties[expression.PropertyName];

            if (p == null)
                throw new LuaXAstGeneratorException(expression.Location, $"Static property {expression.ClassName}.{expression.PropertyName} is not found");

            return p.Value;
        }

        /// <summary>
        /// Evaluates as a variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static object EvaluateVariable(LuaXExpression expression, string name, LuaXVariableInstanceSet variables)
        {
            var v = variables[name];
            if (v == null)
                throw new LuaXAstGeneratorException(expression.Location, $"Variable {name} is not found");
            return v.Value;
        }

        /// <summary>
        /// Evaluate as a constant
        /// </summary>
        /// <param name="constant"></param>
        /// <returns></returns>
        private static object EvaluateConstant(LuaXConstantExpression constant) => constant.Value.Value;
    }
}
