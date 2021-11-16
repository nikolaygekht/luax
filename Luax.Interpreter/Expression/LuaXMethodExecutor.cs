using System;
using System.Reflection;
using Luax.Interpreter.Infrastructure;
using Luax.Interpreter.Infrastructure.Stdlib;
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
            ReachForEnd,
            ReturnDefault,
            Return,
            Break,
            Continue,
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
                    variables.Add(@this.Class.LuaType.ParentClass.TypeOf(), "super", @this);
            }

            types.SearchClass(method.Class.Name, out var currentClass);

            var rt = ExecuteStatements(method.Statements, types, currentClass, variables, out result);
            if (rt == ResultType.ReachForEnd || rt == ResultType.ReturnDefault)
                result = method.ReturnType.DefaultValue();
            return rt;
        }

        private static ResultType ExecuteExtern(LuaXMethod method, LuaXTypesLibrary types, LuaXObjectInstance @this, object[] args, out object result)
        {
            if (!types.ExternMethods.Search(method.Class.Name, method.Name, out var @delegate))
                throw new LuaXAstGeneratorException(method.Location, $"There is no native entry point defined for extern method {method.Class.Name}::{method.Name}");
            try
            {
                if (!method.Static)
                {
                    var args1 = new object[args.Length + 1];
                    args1[0] = @this;
                    for (int i = 0; i < args.Length; i++)
                        args1[i + 1] = args[i];
                    args = args1;
                }
                result = @delegate.Invoke(null, args);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(TargetInvocationException) && e.InnerException != null)
                    e = e.InnerException;
                throw new LuaXExecutionException(method.Location, e.Message, e);
            }
            return ResultType.Return;
        }

        private static ResultType ExecuteStatements(LuaXStatementCollection collection, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            foreach (var statement in collection)
            {
                try
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
                            {
                                var r = ExecuteIf(@if, types, currentClass, variables, out result);
                                if (r != ResultType.ReachForEnd)
                                    return r;
                            }
                            break;
                        case LuaXThrowStatement @throw:
                            ExecuteThrowStatement(@throw, types, currentClass, variables);
                            break;
                        case LuaXTryStatement @try:
                            var tryResult = ExecuteTryStatement(@try, types, currentClass, variables, out result);
                            if (tryResult != ResultType.ReachForEnd)
                                return tryResult;
                            break;
                        case LuaXReturnStatement @return:
                            {
                                if (@return.Expression == null)
                                {
                                    result = null;
                                    return ResultType.ReturnDefault;
                                }

                                result = LuaXExpressionEvaluator.Evaluate(@return.Expression, types, currentClass, variables);
                                return ResultType.Return;
                            }
                        case LuaXWhileStatement @while:
                            {
                                var r = ExecuteWhile(@while, types, currentClass, variables, out result);
                                if (r != ResultType.ReachForEnd)
                                    return r;
                            }
                            break;
                        case LuaXBreakStatement:
                            {
                                result = null;
                                return ResultType.Break;
                            }
                        case LuaXContinueStatement:
                            {
                                result = null;
                                return ResultType.Continue;
                            }
                    }
                }
                catch (LuaXExecutionException e1)
                {
                    e1.Locations.Add(statement.Location);
                    throw;
                }
                catch (Exception e2)
                {
                    throw new LuaXExecutionException(statement.Location, e2.Message, e2);
                }
            }
            result = null;
            return ResultType.ReachForEnd;
        }

        private static ResultType ExecuteTryStatement(LuaXTryStatement @try, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            try
            {
                var tryResult = ExecuteStatements(@try.TryStatements, types, currentClass, variables, out result);
                return tryResult;
            }
            catch (LuaXExecutionException ex)
            {
                if (types.SearchClass("exception", out var exceptionClass))
                {
                    var exceptionObject = new LuaXObjectInstance(exceptionClass);
                    exceptionObject.Properties["code"].Value = ex.Code;
                    exceptionObject.Properties["message"].Value = ex.Message;

                    variables[@try.CatchStatement.CatchIdentifier].Value = exceptionObject;

                    var catchResult = ExecuteStatements(@try.CatchStatement.CatchStatements, types, currentClass, variables, out result);
                    return catchResult;
                }

                throw new LuaXExecutionException(@try.CatchStatement.Location, "Type exception is not defined");
            }
        }

        private static void ExecuteThrowStatement(LuaXThrowStatement @throw, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables)
        {
            var exprResult = LuaXExpressionEvaluator.Evaluate(@throw.ThrowExpression, types, currentClass, variables);

            if (exprResult is LuaXObjectInstance result && types.IsKindOf(result.Class.LuaType.Name, "exception"))
            {
                throw new LuaXExecutionException(@throw.ThrowExpression.Location, result.Properties["message"].Value.ToString(), (int)result.Properties["code"].Value);
            }

            throw new LuaXExecutionException(@throw.Location, "Result of throw statement is not an exception");
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
                throw new LuaXExecutionException(assign.Object.Location, "Object is not initialized yet");
            if (target is not LuaXObjectInstance @object)
                throw new LuaXExecutionException(assign.Object.Location, "The target is not an object");
            var expr = LuaXExpressionEvaluator.Evaluate(assign.Expression, types, currentClass, variables);
            @object.Properties[assign.PropertyName].Value = expr;
        }

        private static void ExecuteAssignArrayItem(LuaXAssignArrayItemStatement assign, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables)
        {
            var target = LuaXExpressionEvaluator.Evaluate(assign.Array, types, currentClass, variables);
            if (target == null)
                throw new LuaXExecutionException(assign.Array.Location, "Array is not initialized yet");
            if (target is not LuaXVariableInstanceArray array)
                throw new LuaXExecutionException(assign.Array.Location, "The target expression is not an array");

            var _index = LuaXExpressionEvaluator.Evaluate(assign.Index, types, currentClass, variables);

            int index;
            if (_index is int i)
                index = i;
            else if (_index is double r)
                index = (int)r;
            else
                throw new LuaXExecutionException(assign.Index.Location, "Index is not a number");

            if (index < 0)
                index = array.Length + index;

            if (index < 0 || index >= array.Length)
                throw new LuaXExecutionException(assign.Index.Location, "Index is out of range");

            var expr = LuaXExpressionEvaluator.Evaluate(assign.Expression, types, currentClass, variables);
            array[index].Value = expr;
        }

        private static ResultType ExecuteIf(LuaXIfStatement ifStatement, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            for (int i = 0; i < ifStatement.Clauses.Count; i++)
            {
                var clause = ifStatement.Clauses[i];
                var v = LuaXExpressionEvaluator.Evaluate(clause.Condition, types, currentClass, variables);
                if (v is bool b)
                {
                    if (b)
                        return ExecuteStatements(clause.Statements, types, currentClass, variables, out result);
                }
                else
                    throw new LuaXExecutionException(clause.Location, "Condition of if statement is not a boolean value");
            }

            if (ifStatement.ElseClause != null)
                return ExecuteStatements(ifStatement.ElseClause, types, currentClass, variables, out result);

            result = null;
            return ResultType.ReachForEnd;
        }

        private static ResultType ExecuteWhile(LuaXWhileStatement whileStatement, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            while (true)
            {
                var v = LuaXExpressionEvaluator.Evaluate(whileStatement.WhileCondition, types, currentClass, variables);
                if (v is bool b)
                {
                    if (b)
                    {
                        ResultType statementsResult = ExecuteStatements(whileStatement.Statements, types, currentClass, variables, out result);
                        if (statementsResult == ResultType.Break)
                            break;
                        else if (statementsResult != ResultType.Continue && statementsResult != ResultType.ReachForEnd)
                            return statementsResult;
                    }
                    else
                        break;
                }
                else
                    throw new LuaXExecutionException(whileStatement.Location, "Condition of while statement is not a boolean value");
            }

            result = null;
            return ResultType.ReachForEnd;
        }
    }
}
