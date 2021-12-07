using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
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
            return expression switch
            {
                LuaXConstantExpression constantExpression => EvaluateConstant(constantExpression),
                LuaXArgumentExpression argumentExpression => EvaluateVariable(expression, argumentExpression.ArgumentName, variables),
                LuaXVariableExpression variableExpression => EvaluateVariable(expression, variableExpression.VariableName, variables),
                LuaXStaticPropertyExpression staticPropertyExpression => EvaluateStaticProperty(staticPropertyExpression, types),
                LuaXInstancePropertyExpression instancePropertyExpression => EvaluateInstanceProperty(instancePropertyExpression, types, runningClass, variables),
                LuaXUnaryOperatorExpression unaryExpression => EvaluateUnary(unaryExpression, types, runningClass, variables),
                LuaXBinaryOperatorExpression binaryExpression => EvaluateBinary(binaryExpression, types, runningClass, variables),
                LuaXStaticCallExpression staticCallExpression => ExecuteStaticCall(staticCallExpression, types, runningClass, variables),
                LuaXInstanceCallExpression instanceCallExpression => ExecuteInstanceCall(instanceCallExpression, types, runningClass, variables),
                LuaXArrayLengthExpression arrayLengthExpression => EvaluateArrayLength(arrayLengthExpression, types, runningClass, variables),
                LuaXArrayAccessExpression arrayAccessExpression => EvaluateArrayAccess(arrayAccessExpression, types, runningClass, variables),
                LuaXCastOperatorExpression castOperatorExpression => EvaluateCast(castOperatorExpression, types, runningClass, variables),
                LuaXTypeNameOperatorExpression typenameExpression => EvaluateTypename(typenameExpression, types, runningClass, variables),
                LuaXNewObjectExpression newObjectExpression => EvaluateNewObject(newObjectExpression, types, variables),
                LuaXNewArrayExpression newArrayExpression => EvaluateNewArray(newArrayExpression, types, runningClass, variables),
                LuaXNewArrayWithInitExpression newArrayWithInitExpression => EvaluateNewArrayWithInit(newArrayWithInitExpression, types, runningClass, variables),
                _ => throw new LuaXExecutionException(expression.Location, $"Unexpected expression type {expression.GetType().Name}"),
            };
        }

        private static object ExecuteInstanceCall(LuaXInstanceCallExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var args = CreateCallArgs(expression.Arguments, types, runningClass, variables);
            var _this = Evaluate(expression.Object, types, runningClass, variables);
            if (_this == null)
                throw new LuaXExecutionException(expression.Location, "The object is not initialized yet");
            if (_this is not LuaXObjectInstance @this)
                throw new LuaXExecutionException(expression.Location, "The target is not an object");
            if (!@this.Class.SearchMethod(expression.MethodName, expression.ExactClass, out var method))
                throw new LuaXExecutionException(expression.Location, $"The method {expression.MethodName} is not found in class");
            LuaXMethodExecutor.Execute(method, types, @this, args, out var r);
            return r;
        }

        private static object ExecuteStaticCall(LuaXStaticCallExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var args = CreateCallArgs(expression.Arguments, types, runningClass, variables);
            if (!types.SearchClass(expression.ClassName, out var @class))
                throw new LuaXExecutionException(expression.Location, $"The class {expression.ClassName} is not found");
            if (!@class.SearchMethod(expression.MethodName, expression.ClassName, out var method))
                throw new LuaXExecutionException(expression.Location, $"The method {expression.MethodName} is not found in class");
            LuaXMethodExecutor.Execute(method, types, null, args, out var r);
            return r;
        }

        private static object[] CreateCallArgs(LuaXExpressionCollection arguments, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            if (arguments == null || arguments.Count == 0)
                return Array.Empty<object>();

            var r = new object[arguments.Count];
            for (int i = 0; i < arguments.Count; i++)
                r[i] = Evaluate(arguments[i], types, runningClass, variables);
            return r;
        }

        private static object EvaluateCast(LuaXCastOperatorExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var argument = Evaluate(expression.Argument, types, runningClass, variables);
            if (!types.CastTo(expression.ReturnType, ref argument))
                throw new LuaXExecutionException(expression.Location, $"Cast to {expression.ReturnType} is not supported");
            return argument;
        }

        private static object EvaluateTypename(LuaXTypeNameOperatorExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var argument = Evaluate(expression.Argument, types, runningClass, variables);
            if (argument == null)
                return "nil";
            else if (argument is int)
                return LuaXTypeDefinition.Integer.ToString();
            else if (argument is double)
                return LuaXTypeDefinition.Real.ToString();
            else if (argument is string)
                return LuaXTypeDefinition.String.ToString();
            else if (argument is DateTime)
                return LuaXTypeDefinition.Datetime.ToString();
            else if (argument is bool)
                return LuaXTypeDefinition.Boolean.ToString();
            else if (argument is LuaXObjectInstance obj)
                return obj.Class.LuaType.TypeOf().ToString();
            else if (argument is LuaXVariableInstanceArray arr)
                return arr.ElementType.ToString() + "[]";
            throw new LuaXExecutionException(expression.Location, "Unsupported type");
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
                throw new LuaXExecutionException(expression.Location, "Numeric argument is required for -");
            }
            else if (expression.Operator == LuaXUnaryOperator.Not)
            {
                if (argument is bool b)
                    return !b;
                throw new LuaXExecutionException(expression.Location, "Boolean argument is required for not");
            }
            throw new LuaXExecutionException(expression.Location, "Unknown operator");
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
                    throw new LuaXExecutionException(expression.Location, "Left argument of math operation is not a number");

                if (right is int i4)
                    f2 = i4;
                else if (right is double d2)
                    f2 = d2;
                else
                    throw new LuaXExecutionException(expression.Location, "Left argument of math operation is not a number");

                return doubleAction(f1, f2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CompareNumbers(LuaXExpression expression, object left, object right)
        {
            int sign;
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
            else
                throw new LuaXExecutionException(expression.Location, "Unexpected relational operator state");

            return sign;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CompareDate(DateTime d1, DateTime d2)
        {
            var l = d1.Ticks - d2.Ticks;
            if (l == 0)
                return 0;
            else if (l < 0)
                return -1;
            else
                return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsRelationOfNumbers(object left, object right)
            => (left is int || left is double) && (right is int || right is double);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsRelationOfObjects(object left, object right)
            => left == null || right == null || left is LuaXObjectInstance || right is LuaXVariableInstanceArray;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CompareObjects(object left, object right)
            => ReferenceEquals(left, right) ? 0 : 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CompareBoolean(bool b1, bool b2)
            => b1 == b2 ? 0 : 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object RelationalOperation(LuaXExpression expression, LuaXBinaryOperator @operator, object left, object right)
        {
            int? sign = null;
            bool canRelate = false;

            if (IsRelationOfNumbers(left, right))
            {
                sign = CompareNumbers(expression, left, right);
                canRelate = true;
            }
            else if (IsRelationOfObjects(left, right))
            {
                sign = CompareObjects(left, right);
                canRelate = false;
            }
            else if (left.GetType() == right.GetType())
            {
                if (left is bool b1)
                {
                    sign = CompareBoolean(b1, (bool)right);
                    canRelate = false;
                }
                else if (left is string s1)
                {
                    sign = string.CompareOrdinal(s1, (string)right);
                    canRelate = true;
                }
                else if (left is DateTime d1)
                {
                    sign = CompareDate(d1, (DateTime)right);
                    canRelate = true;
                }
            }

            if (sign == null || (!canRelate && @operator != LuaXBinaryOperator.Equal && @operator != LuaXBinaryOperator.NotEqual))
                throw new LuaXExecutionException(expression.Location, "An attempt to compare incompatible types");

            return @operator switch
            {
                LuaXBinaryOperator.Equal => sign == 0,
                LuaXBinaryOperator.NotEqual => sign != 0,
                LuaXBinaryOperator.Greater => sign > 0,
                LuaXBinaryOperator.Less => sign < 0,
                LuaXBinaryOperator.GreaterOrEqual => sign >= 0,
                LuaXBinaryOperator.LessOrEqual => sign <= 0,
                _ => throw new LuaXExecutionException(expression.Location, "Unknown relational operator"),
            };
        }

        private static object EvaluateBinary(LuaXBinaryOperatorExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            if (expression.Operator == LuaXBinaryOperator.And || expression.Operator == LuaXBinaryOperator.Or)
                return EvaluateLogical(expression, types, runningClass, variables);
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
                LuaXBinaryOperator.Equal or LuaXBinaryOperator.NotEqual or LuaXBinaryOperator.Less or LuaXBinaryOperator.LessOrEqual or LuaXBinaryOperator.Greater or LuaXBinaryOperator.GreaterOrEqual => RelationalOperation(expression, expression.Operator, left, right),
                _ => throw new LuaXExecutionException(expression.Location, "Unsupported operation"),
            };
        }

        private static object EvaluateLogical(LuaXBinaryOperatorExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var left = Evaluate(expression.LeftArgument, types, runningClass, variables);
            if (left is not bool a)
                throw new LuaXExecutionException(expression.LeftArgument.Location, "Expression is expected to be a boolean value");

            if (expression.Operator == LuaXBinaryOperator.And && !a)
                return false;
            if (expression.Operator == LuaXBinaryOperator.Or && a)
                return true;

            var right = Evaluate(expression.RightArgument, types, runningClass, variables);
            if (right is not bool b)
                throw new LuaXExecutionException(expression.RightArgument.Location, "Expression is expected to be a boolean value");

            if (expression.Operator == LuaXBinaryOperator.And)
                return a && b;
            else if (expression.Operator == LuaXBinaryOperator.Or)
                return a || b;
            throw new LuaXExecutionException(expression.Location, $"Unexpected logical operator {expression.Operator}");
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
                throw new LuaXExecutionException(expression.Location, "The array expression does not return an array value");

            int index;
            object _index = Evaluate(expression.IndexExpression, types, runningClass, variables);
            if (_index is int i)
                index = i;
            else if (_index is double d)
                index = (int)d;
            else
                throw new LuaXExecutionException(expression.Location, "The size expression is not an numeric value");

            if (index < 0)
                index = array.Length + index;

            if (index < 0 || index >= array.Length)
                throw new LuaXExecutionException(expression.Location, "The index is out of range");

            return array[index].Value;
        }

        private static object EvaluateArrayLength(LuaXArrayLengthExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            var array = Evaluate(expression.ArrayExpression, types, runningClass, variables);
            if (array is not LuaXVariableInstanceArray a)
                throw new LuaXExecutionException(expression.Location, "The value is not an array or array is not initialized");
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
                throw new LuaXExecutionException(expression.Location, "The size expression is not an numeric value");

            return new LuaXVariableInstanceArray(expression.ReturnType, size);
        }

        /// <summary>
        /// Evaluates a new array with initialization expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="types"></param>
        /// <param name="runningClass"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static object EvaluateNewArrayWithInit(LuaXNewArrayWithInitExpression expression, LuaXTypesLibrary types, LuaXClassInstance runningClass, LuaXVariableInstanceSet variables)
        {
            int size = expression.InitExpressions.Count;
            object[] objects = new object[size];
            for (int i = 0; i < size; i++)
                objects[i] = Evaluate(expression.InitExpressions[i], types, runningClass, variables);
            return new LuaXVariableInstanceArray(expression.ReturnType, objects);
        }

        /// <summary>
        /// Evaluates a new object expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="types"></param>
        /// <param name="runningClass"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        private static object EvaluateNewObject(LuaXNewObjectExpression expression, LuaXTypesLibrary types, LuaXVariableInstanceSet variables)
        {
            if (!types.SearchClass(expression.ClassName, out var @class))
                throw new LuaXExecutionException(expression.Location, $"Class {expression.ClassName} is not found");

            LuaXVariableInstance variable = variables["this"];
            if (variable != null && variable.Value is LuaXObjectInstance currentInstance &&
                expression.ClassName.StartsWith($"{currentInstance.Class.LuaType.Name}."))
                return @class.New(types, currentInstance);

            return @class.New(types);
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
                throw new LuaXExecutionException(expression.Location, "The expression is not a class instance expression");

            LuaXVariableInstance p = null;
            string className = v.Class.LuaType.Name;
            while (p == null && v != null)
            {
                p = v.Properties[expression.PropertyName];
                v = v.OwnerObjectInstance;
            }

            if (p == null)
                throw new LuaXExecutionException(expression.Location, $"Instance property {className}.{expression.PropertyName} is not found");

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
                throw new LuaXExecutionException(expression.Location, $"Class {expression.ClassName} is not found");

            var p = @class.StaticProperties[expression.PropertyName];

            if (p == null)
                throw new LuaXExecutionException(expression.Location, $"Static property {expression.ClassName}.{expression.PropertyName} is not found");

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
                throw new LuaXExecutionException(expression.Location, $"Variable {name} is not found");
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
