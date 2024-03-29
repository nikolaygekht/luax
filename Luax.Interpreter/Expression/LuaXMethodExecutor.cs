﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Luax.Interpreter.Infrastructure;
using Luax.Interpreter.Infrastructure.Stdlib;
using Luax.Parser.Ast;
using Luax.Parser.Ast.LuaExpression;
using Luax.Parser.Ast.Statement;

namespace Luax.Interpreter.Expression
{
    /// <summary>
    /// The class to execute LuaX method
    /// </summary>
    public static class LuaXMethodExecutor
    {
        /// <summary>
        /// The debugging event
        /// </summary>
        public static event EventHandler<BeforeStatementExecutionEventArgs> BeforeStatementExecution;

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
            types.SearchClass(method.Class.Name, out var currentClass);

            if (!method.Static && @this.Class.LuaType.Name != currentClass.LuaType.Name && @this.Class.LuaType.Name.StartsWith($"{currentClass.LuaType.Name}."))
            {
                @this = findMethodOwner(method, @this.OwnerObjectInstance, currentClass.LuaType.Name);
            }

            if (method.Extern)
                return ExecuteExtern(method, types, @this, args, out result);

            if (method.IsConstructor)
                HandleConstructor(method, types, @this);

            var variables = CreateVariablesForMethod(method, @this, args);
            var rt = ExecuteStatements(method, method.Statements, types, currentClass, variables, out result);
            if (rt == ResultType.ReachForEnd || rt == ResultType.ReturnDefault)
                result = method.ReturnType.DefaultValue();
            return rt;
        }

        private static LuaXObjectInstance findMethodOwner(LuaXMethod method, LuaXObjectInstance owner, string className)
        {
            while (owner != null)
            {
                LuaXClass parent = owner.Class.LuaType;
                bool toBreak = false;
                while (parent != null)
                {
                    if (parent.Name == className)
                    {
                        toBreak = true;
                        break;
                    }
                    if (!parent.HasParent)
                        break;
                    parent = parent.ParentClass;
                }
                if (toBreak)
                    break;
                owner = owner.OwnerObjectInstance;
            }

            return owner ?? throw new LuaXExecutionException(method.Location, $"Owner instance {className} is not found");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static LuaXVariableInstanceSet CreateVariablesForMethod(LuaXMethod method, LuaXObjectInstance @this, object[] args)
        {
            //create variables
            LuaXVariableInstanceSet variables = new LuaXVariableInstanceSet(method);
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

            return variables;
        }

        private static void HandleConstructor(LuaXMethod method, LuaXTypesLibrary types, LuaXObjectInstance @this)
        {
            if (method.Class.ParentClass?.Constructor != null)
                Execute(method.Class.ParentClass?.Constructor, types, @this, Array.Empty<object>(), out _);
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
                if (e is TargetInvocationException && e.InnerException != null)
                    e = e.InnerException;
                throw new LuaXExecutionException(method, method.Location, e.Message, e);
            }
            return ResultType.Return;
        }

        private static bool ExecuteStatement(LuaXMethod callingMethod, LuaXTypesLibrary types,
            LuaXClassInstance currentClass, LuaXVariableInstanceSet variables,
            LuaXStatement statement,
            out object result, out ResultType rt)
        {
            BeforeStatementExecution?.Invoke(null, new BeforeStatementExecutionEventArgs()
            {
                Method = callingMethod,
                Statement = statement,
                TypesLibrary = types,
                LocalVariables = variables,
            });

            rt = ResultType.ReachForEnd;
            result = null;
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
                    rt = ExecuteIf(callingMethod, @if, types, currentClass, variables, out result);
                    break;
                case LuaXThrowStatement @throw:
                    ExecuteThrowStatement(callingMethod, @throw, types, currentClass, variables);
                    break;
                case LuaXTryStatement @try:
                    rt = ExecuteTryStatement(callingMethod, @try, types, currentClass, variables, out result);
                    break;
                case LuaXReturnStatement @return when @return.Expression == null:
                    result = null;
                    rt = ResultType.ReturnDefault;
                    return true;
                case LuaXReturnStatement @return when @return.Expression != null:
                    result = LuaXExpressionEvaluator.Evaluate(@return.Expression, types, currentClass,
                        variables);
                    rt = ResultType.Return;
                    return true;
                case LuaXWhileStatement @while:
                    rt = ExecuteWhile(callingMethod, @while, types, currentClass, variables, out result);
                    break;
                case LuaXRepeatStatement @repeat:
                    rt = ExecuteRepeat(callingMethod, @repeat, types, currentClass, variables, out result);
                    break;
                case LuaXBreakStatement:
                    result = null;
                    rt = ResultType.Break;
                    return true;
                case LuaXContinueStatement:
                    result = null;
                    rt = ResultType.Continue;
                    return true;
                case LuaXForStatement @for:
                    rt = ExecuteFor(callingMethod, @for, types, currentClass, variables, out result);
                    break;
            }
            return rt != ResultType.ReachForEnd;
        }

        private static ResultType ExecuteStatements(LuaXMethod callingMethod, LuaXStatementCollection collection,
            LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables,
            out object result)
        {
            result = null;
            foreach (var statement in collection)
            {
                try
                {
                    if (ExecuteStatement(callingMethod, types, currentClass, variables, statement, out result, out var resultType))
                        return resultType;
                }
                catch (LuaXExecutionException e1)
                {
                    var newFrame = new LuaXStackFrame(callingMethod, statement.Location);
                    //In case if exception was rethrown - we don't need to add duplicate frame. As Well as for each statement in a call
                    if (!e1.LuaXStackTrace.GetLastFrame().IsTheSame(newFrame) &&
                        !ReferenceEquals(e1.LuaXStackTrace.GetLastFrame().CallSite, callingMethod))
                        e1.LuaXStackTrace.Add(newFrame);
                    throw;
                }
                catch (Exception e2)
                {
                    throw new LuaXExecutionException(callingMethod, statement.Location, e2.Message, e2);
                }
            }

            return ResultType.ReachForEnd;
        }

        private static ResultType ExecuteTryStatement(LuaXMethod callingMethod, LuaXTryStatement @try, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            try
            {
                var tryResult = ExecuteStatements(callingMethod, @try.TryStatements, types, currentClass, variables, out result);
                return tryResult;
            }
            catch (Exception ex)
            {
                if (@try.CatchClause.CatchIdentifier == null)
                {
                    var catchResult = ExecuteStatements(callingMethod, @try.CatchClause.CatchStatements, types, currentClass, variables, out result);
                    return catchResult;
                }
                else
                {
                    var exceptionVariable = variables[@try.CatchClause.CatchIdentifier];
                    if (types.SearchClass(exceptionVariable.LuaType.Class, out var exceptionClass))
                    {
                        var exceptionObject = CreateExceptionInstance(exceptionClass, ex);
                        variables[@try.CatchClause.CatchIdentifier].Value = exceptionObject;
                        var catchResult = ExecuteStatements(callingMethod, @try.CatchClause.CatchStatements, types, currentClass, variables, out result);
                        return catchResult;
                    }

                    throw new LuaXExecutionException(@try.CatchClause.Location, $"Type '{exceptionVariable.LuaType.Class}' is not defined");
                }
            }
        }

        private static LuaXObjectInstance CreateExceptionInstance(LuaXClassInstance exceptionClass, Exception ex)
        {
            var exceptionObject = new LuaXObjectInstance(exceptionClass);
            exceptionObject.Properties["message"].Value = ex.Message;

            if (ex is LuaXExecutionException executionException)
            {
                foreach (var property in executionException.Properties)
                {
                    if (property.Name == "message")
                        continue;

                    exceptionObject.Properties[property.Name].Value = property.Value;
                }
            }
            return exceptionObject;
        }

        private static void ExecuteThrowStatement(LuaXMethod callingMethod, LuaXThrowStatement @throw, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables)
        {
            var exprResult = LuaXExpressionEvaluator.Evaluate(@throw.ThrowExpression, types, currentClass, variables);

            if (exprResult is LuaXObjectInstance result && types.IsKindOf(result.Class.LuaType.Name, "exception"))
            {
                throw new LuaXExecutionException(callingMethod, @throw.Location, result.Properties["message"].Value.ToString(), result.Properties);
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

            LuaXVariableInstance p = null;
            while (p == null && @object != null)
            {
                p = @object.Properties[assign.PropertyName];
                @object = @object.OwnerObjectInstance;
            }
            if (p == null)
                throw new LuaXExecutionException(assign.Object.Location, $"Property {assign.PropertyName} is not found");

            p.Value = expr;
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

        private static ResultType ExecuteIf(LuaXMethod callingMethod, LuaXIfStatement ifStatement, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            for (int i = 0; i < ifStatement.Clauses.Count; i++)
            {
                var clause = ifStatement.Clauses[i];
                var v = LuaXExpressionEvaluator.Evaluate(clause.Condition, types, currentClass, variables);
                if (v is bool b)
                {
                    if (b)
                        return ExecuteStatements(callingMethod, clause.Statements, types, currentClass, variables, out result);
                }
                else
                    throw new LuaXExecutionException(clause.Location, "Condition of if statement is not a boolean value");
            }

            if (ifStatement.ElseClause != null)
                return ExecuteStatements(callingMethod, ifStatement.ElseClause, types, currentClass, variables, out result);

            result = null;
            return ResultType.ReachForEnd;
        }

        private static ResultType ExecuteWhile(LuaXMethod callingMethod, LuaXWhileStatement whileStatement, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            while (true)
            {
                var v = LuaXExpressionEvaluator.Evaluate(whileStatement.WhileCondition, types, currentClass, variables);
                if (v is bool b)
                {
                    if (b)
                    {
                        ResultType statementsResult = ExecuteStatements(callingMethod, whileStatement.Statements, types, currentClass, variables, out result);
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

        private static ResultType ExecuteFor(LuaXMethod callingMethod, LuaXForStatement forStatement, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            var variableType = forStatement.ForLoopDescription.Variable.LuaType;
            var loopVariableName = forStatement.ForLoopDescription.Variable.Name;

            var stepValue = LuaXExpressionEvaluator.Evaluate(forStatement.ForLoopDescription.Step, types, currentClass, variables);
            var stepExpression = new LuaXBinaryOperatorExpression(LuaXBinaryOperator.Add,
                new LuaXVariableExpression(forStatement.ForLoopDescription.Variable.Name, variableType,
                forStatement.ForLoopDescription.Variable.Location),
                new LuaXConstantExpression(new LuaXConstant(variableType.TypeId, stepValue, forStatement.ForLoopDescription.Step.Location)),
                LuaXTypeDefinition.Boolean, forStatement.ForLoopDescription.Step.Location);
            var stepStatement = new LuaXAssignVariableStatement(loopVariableName, stepExpression,
                forStatement.ForLoopDescription.Step.Location);

            var selectOperationExpression = new LuaXBinaryOperatorExpression(LuaXBinaryOperator.GreaterOrEqual,
               forStatement.ForLoopDescription.Step,
               new LuaXConstantExpression(new LuaXConstant(variableType.TypeId, 0, forStatement.ForLoopDescription.Step.Location)),
               LuaXTypeDefinition.Boolean, forStatement.ForLoopDescription.Step.Location);

            bool selectLimitOpValue = (bool)LuaXExpressionEvaluator.Evaluate(selectOperationExpression, types, currentClass, variables);
            var limitCompareOp = selectLimitOpValue ? LuaXBinaryOperator.LessOrEqual : LuaXBinaryOperator.GreaterOrEqual;

            var limitValue = LuaXExpressionEvaluator.Evaluate(forStatement.ForLoopDescription.Limit, types, currentClass, variables);
            LuaXConstantExpression limitConstExpression = new LuaXConstantExpression(new LuaXConstant(variableType.TypeId, limitValue, forStatement.ForLoopDescription.Limit.Location));

            var limitExpression = new LuaXBinaryOperatorExpression(limitCompareOp,
                                new LuaXVariableExpression(forStatement.ForLoopDescription.Variable.Name,
                                variableType, forStatement.ForLoopDescription.Variable.Location),
                                limitConstExpression, LuaXTypeDefinition.Boolean,
                                limitConstExpression.Location);

            if (limitExpression.ReturnType.IsBoolean())
            {
                var assignStatement = new LuaXAssignVariableStatement(loopVariableName, forStatement.ForLoopDescription.Start,
                    forStatement.ForLoopDescription.Start.Location);
                ExecuteAssignVariable(assignStatement, types, currentClass, variables);

                while ((bool)LuaXExpressionEvaluator.Evaluate(limitExpression, types, currentClass, variables))
                {
                    ResultType statementsResult = ExecuteStatements(callingMethod, forStatement.Statements, types, currentClass, variables, out result);
                    if (statementsResult == ResultType.Break)
                        break;
                    if (statementsResult != ResultType.Continue && statementsResult != ResultType.ReachForEnd)
                        return statementsResult;

                    ExecuteAssignVariable(stepStatement, types, currentClass, variables);
                }
            }
            else
                throw new LuaXExecutionException(forStatement.Location, "Condition part of for statement is not a boolean value");

            result = null;
            return ResultType.ReachForEnd;
        }

        private static ResultType ExecuteRepeat(LuaXMethod callingMethod, LuaXRepeatStatement repeatStatement, LuaXTypesLibrary types, LuaXClassInstance currentClass, LuaXVariableInstanceSet variables, out object result)
        {
            while (true)
            {
                ResultType statementsResult = ExecuteStatements(callingMethod, repeatStatement.Statements, types, currentClass, variables, out result);
                if (statementsResult == ResultType.Break)
                    break;
                if (statementsResult != ResultType.Continue && statementsResult != ResultType.ReachForEnd)
                    return statementsResult;

                var v = LuaXExpressionEvaluator.Evaluate(repeatStatement.UntilCondition, types, currentClass, variables);
                if (v is bool b)
                {
                    if (!b)
                        break;
                }
                else
                    throw new LuaXExecutionException(repeatStatement.Location, "Condition of until statement is not a boolean value");
            }

            return ResultType.ReachForEnd;
        }
    }
}
