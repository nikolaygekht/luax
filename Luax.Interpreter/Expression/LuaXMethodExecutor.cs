using System;
using Luax.Interpreter.Infrastructure;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Statement;

namespace Luax.Interpreter.Expression
{
    /// <summary>
    /// The class to execute LuaX method
    /// </summary>
    public static class LuaXMethodExecutor
    {
        /// <summary>
        /// The return type
        /// </summary>
        public enum ResultType
        {
            Default,
            Return,
        };

        /// <summary>
        /// Executes the method
        /// </summary>
        /// <param name="method">The method spec</param>
        /// <param name="types">The type library</param>
        /// <param name="this">This object (for instance calls) or `null` for static calls</param>
        /// <param name="args">The method arguments</param>
        /// <param name="result">The output variable that accepts the return value</param>
        /// <returns></returns>
        public static ResultType Execute(LuaXMethod method, LuaXTypesLibrary types, LuaXObjectInstance @this, object[] args, out object result)
        {
            if (method.Extern)
                return ExecuteExtern(method, types, @this, args, out result);

            //create variables
            var variables = new LuaXVariableInstanceSet(method);
            if (args.Length != method.Arguments.Count)
                throw new ArgumentException("The number of method arguments to not match passed arguments");
            //initialize arguments
            for (int i = 0; i < args.Length; i++)
                variables[method.Arguments[i].Name].Value = args[i];
            if (!method.Static)
            {
                variables.Add(@this.Class.LuaType.TypeOf(), "this", @this);
                if (@this.Class.LuaType.ParentClass != null)
                    variables.Add(@this.Class.LuaType.TypeOf(), "super", @this.Class.LuaType.ParentClass.TypeOf());
            }

            types.SearchClass(method.Class.Name, out var currentClass);

            var rt = ExecuteStatements(method.Statements, types, currentClass, variables, out result);
            if (rt == ResultType.Default)
                result = method.ReturnType.DefaultValue();
            return rt;
        }

        private static ResultType ExecuteExtern(LuaXMethod method, LuaXTypesLibrary types, LuaXObjectInstance @this, object[] args, out object result)
        {
            if (!types.ExternMethods.Search(method.Class.Name, method.Name, out var @delegate))
                throw new LuaXAstGeneratorException(method.Location, $"There is no native entry point defined for extern method {method.Class.Name}::{method.Name}");
            result = @delegate.Invoke(@this, args);
            return ResultType.Return;
        }

        private static ResultType ExecuteStatements(LuaXStatementCollection collection, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            foreach (var statement in collection)
            {
                switch (statement)
                {
                    case LuaXAssignVariableStatement assignVariable:
                        ExecuteAssignVariable(assignVariable, types, currentClass, variables);
                        break;
                    case LuaXAssignStaticPropertyStatement assignStaticProperty:
                        ExecuteAssignStaticProperty(assignStaticProperty, types, currentClass, variables);
                        break;
                    case LuaXAssignInstancePropertyStatement assignInstanceProperty:
                        ExecuteAssignInstanceProperty(assignInstanceProperty, types, currentClass, variables);
                        break;
                    case LuaXAssignArrayItemStatement assignArrayItem:
                        ExecuteAssignArrayItem(assignArrayItem, types, currentClass, variables);
                        break;
                    case LuaXCallStatement callStatement:
                        ExecuteCallStatement(callStatement, types, currentClass, variables);
                        break;
                    case LuaXIfStatement @if:
                        break;
                    case LuaXReturnStatement @return:
                        {
                            result = LuaXExpressionEvaluator.Evaluate(@return.Expression, types, currentClass, variables);
                            return ResultType.Return;
                        }
                }
            }
            result = null;
            return ResultType.Default;
        }

        private static void ExecuteAssignVariable(LuaXAssignVariableStatement assign, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables)
        {
            var expr = LuaXExpressionEvaluator.Evaluate(assign.Expression, types, currentClass, variables);
            variables[assign.VariableName].Value = expr;
        }

        private static void ExecuteCallStatement(LuaXCallStatement call, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables)
            => LuaXExpressionEvaluator.Evaluate(call.CallExpression, types, currentClass, variables);

        private static void ExecuteAssignStaticProperty(LuaXAssignStaticPropertyStatement assign, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables)
        {
            var expr = LuaXExpressionEvaluator.Evaluate(assign.Expression, types, currentClass, variables);
            types.SearchClass(assign.ClassName, out var @class);
            @class.StaticProperties[assign.PropertyName].Value = expr;
        }

        private static void ExecuteAssignInstanceProperty(LuaXAssignInstancePropertyStatement assign, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables)
        {
            var target = LuaXExpressionEvaluator.Evaluate(assign.Object, types, currentClass, variables);
            if (target == null)
                throw new LuaXAstGeneratorException(assign.Object.Location, "Object is not initialized yet");
            if (target is not LuaXObjectInstance @object)
                throw new LuaXAstGeneratorException(assign.Object.Location, "The target is not an object");
            var expr = LuaXExpressionEvaluator.Evaluate(assign.Expression, types, currentClass, variables);
            @object.Properties[assign.PropertyName].Value = expr;
        }

        private static void ExecuteAssignArrayItem(LuaXAssignArrayItemStatement assign, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables)
        {
            var target = LuaXExpressionEvaluator.Evaluate(assign.Array, types, currentClass, variables);
            if (target == null)
                throw new LuaXAstGeneratorException(assign.Array.Location, "Array is not initialized yet");
            if (target is not LuaXVariableInstanceArray array)
                throw new LuaXAstGeneratorException(assign.Array.Location, "The target expression is not an array");

            var _index = LuaXExpressionEvaluator.Evaluate(assign.Index, types, currentClass, variables);

            int index;
            if (_index is int i)
                index = i;
            else if (_index is double r)
                index = (int)r;
            else
                throw new LuaXAstGeneratorException(assign.Index.Location, "Index is not a number");

            if (index < 0)
                index = array.Length + index;

            if (index < 0 || index >= array.Length)
                throw new LuaXAstGeneratorException(assign.Index.Location, "Index is out of range");

            var expr = LuaXExpressionEvaluator.Evaluate(assign.Expression, types, currentClass, variables);
            array[index].Value = expr;
        }
    }
}
