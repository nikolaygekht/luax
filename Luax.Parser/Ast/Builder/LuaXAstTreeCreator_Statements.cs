using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Hime.Redist;
using Luax.Parser.Ast.LuaExpression;
using Luax.Parser.Ast.Statement;
using Microsoft.VisualBasic.FileIO;

namespace Luax.Parser.Ast.Builder
{
    internal partial class LuaXAstTreeCreator
    {
        /// <summary>
        /// Depth of nested loops
        /// </summary>
        private int mLoopDepth;
        /// <summary>
        /// Processes the method body
        /// </summary>
        /// <param name="node"></param>
        /// <param name="method"></param>
        public void ProcessBody(IAstNode node, LuaXClass @class, LuaXMethod method)
        {
            mLoopDepth = 0;
            ProcessStatements(node.Children, @class, method, method.Statements);
        }

        private void ProcessStatements(IReadOnlyList<IAstNode> nodes, LuaXClass @class, LuaXMethod method, LuaXStatementCollection statements)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var child = nodes[i];
                if (child.Symbol != "STATEMENT" || child.Children.Count != 1)
                    throw new LuaXAstGeneratorException(Name, child, "STATEMENT is expected");
                child = child.Children[0];
                switch (child.Symbol)
                {
                    case "DECLARATION":
                        ProcessDeclarationStatement(child, method);
                        break;
                    case "CONST_DECLARATION":
                        ProcessConstantDeclarationInMethod(child, method);
                        break;
                    case "CALL_STMT":
                        ProcessCallStatement(child, @class, method, statements);
                        break;
                    case "ASSIGN_STMT":
                        ProcessAssignStatement(child, @class, method, statements);
                        break;
                    case "IF_STMT":
                        ProcessesIfStatement(child, @class, method, statements);
                        break;
                    case "TRY_STMT":
                        ProcessTryStatement(child, @class, method, statements);
                        break;
                    case "THROW_STMT":
                        ProcessThrowStatement(child, @class, method, statements);
                        break;
                    case "RETURN_STMT":
                        ProcessesReturnStatement(child, @class, method, statements);
                        break;
                    case "REPEAT_STMT":
                        ProcessesRepeatStatement(child, @class, method, statements);
                        break;
                    case "WHILE_STMT":
                        ProcessesWhileStatement(child, @class, method, statements);
                        break;
                    case "BREAK_STMT":
                        ProcessesBreakStatement(child, statements);
                        break;
                    case "CONTINUE_STMT":
                        ProcessesContinueStatement(child, statements);
                        break;
                    default:
                        throw new LuaXAstGeneratorException(Name, child, $"Unexpected symbol {child.Symbol}");
                }
            }
        }

        private void ProcessThrowStatement(IAstNode node, LuaXClass @class, LuaXMethod method, LuaXStatementCollection statements)
        {
            if (node.Children.Count == 3 && node.Children[1].Symbol != "REXPR")
                throw new LuaXAstGeneratorException(Name, node, "Expression is expected here");

            var throwExpr = ProcessExpression(node.Children[1], @class, method);

            if (!throwExpr.ReturnType.IsObject() || !Metadata.IsKindOf(throwExpr.ReturnType.Class, "exception"))
                throw new LuaXAstGeneratorException(Name, node, "Expression with return type exception is expected here");

            var throwStmt = new LuaXThrowStatement(throwExpr, new LuaXElementLocation(Name, node));

            statements.Add(throwStmt);
        }

        private LuaXCatchClause ProcessCatchClause(IAstNode node, LuaXClass @class, LuaXMethod method)
        {
            if (node.Children.Count < 3 || node.Children[1].Symbol != "IDENTIFIER" ||
                !method.Variables.Search(node.Children[1].Value, out var v1) || v1 == null ||
                !Metadata.IsKindOf(v1.LuaType.Class, "exception"))
                throw new LuaXAstGeneratorException(Name, node, "Identifier of declared variable of type exception is expected here");

            var catchClause = new LuaXCatchClause(node.Children[1].Value, new LuaXElementLocation(Name, node));
            ProcessStatements(node.Children[2].Children, @class, method, catchClause.CatchStatements);
            return catchClause;
        }

        private void ProcessTryStatement(IAstNode node, LuaXClass @class, LuaXMethod method, LuaXStatementCollection statements)
        {
            if (node.Children.Count < 3 || node.Children[2].Symbol != "CATCH_CLAUSE")
                throw new LuaXAstGeneratorException(Name, node, "Catch clause is expected here");

            var tryStatements = new LuaXStatementCollection();
            LuaXCatchClause catchClause = null;
            foreach (var child in node.Children)
            {
                if (child.Symbol == "END")
                    break;

                switch (child.Symbol)
                {
                    case "STATEMENTS":
                        ProcessStatements(child.Children, @class, method, tryStatements);
                        break;
                    case "CATCH_CLAUSE":
                        catchClause = ProcessCatchClause(node.Children[2], @class, method);
                        break;
                    default:
                        continue;
                }
            }

            var stmt = new LuaXTryStatement(new LuaXElementLocation(Name, node), catchClause, tryStatements);
            statements.Add(stmt);
        }

        private void ProcessDeclarationStatement(IAstNode node, LuaXMethod method)
        {
            if (node.Children.Count < 2 || node.Children[1].Symbol != "DECL_LIST")
                throw new LuaXAstGeneratorException(Name, node, "One or more DECL is expected here");

            ProcessDeclarationList<LuaXVariable>(node.Children[1], new LuaXVariableFactory<LuaXVariable>(), v =>
            {
                if (method.Arguments.Contains(v.Name) || method.Variables.Contains(v.Name))
                    throw new LuaXAstGeneratorException(Name, node, $"Variable {v.Name} already exists");
                if (method.Constants.Contains(v.Name))
                    throw new LuaXAstGeneratorException(Name, node, $"Constant {v.Name} already exists");
                method.Variables.Add(v);
            });
        }

        /// <summary>
        /// Processes the assignment statement
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        /// <param name="method"></param>
        private void ProcessAssignStatement(IAstNode node, LuaXClass @class, LuaXMethod method, LuaXStatementCollection statements)
        {
            if (node.Children.Count < 1 || node.Children[0].Symbol != "ASSIGN_TARGET")
                throw new LuaXAstGeneratorException(Name, node, "A target expression is expected here");
            var target = ProcessExpression(node.Children[0], @class, method);
            if (node.Children.Count < 3 || node.Children[2].Symbol != "REXPR")
                throw new LuaXAstGeneratorException(Name, node, "A source expression is expected here");
            var source = ProcessExpression(node.Children[2], @class, method);

            if (!target.ReturnType.IsTheSame(source.ReturnType))
            {
                var source1 = CastToCompatible(source, target.ReturnType);
                source = source1 ?? throw new LuaXAstGeneratorException(Name, node, $"The types are incompatible. The target is {target.ReturnType} and source is {source.ReturnType}");
            }

            var location = new LuaXElementLocation(Name, node);
            LuaXStatement stmt;

            if (target is LuaXVariableExpression e1)
                stmt = new LuaXAssignVariableStatement(e1.VariableName, source, location);
            else if (target is LuaXStaticPropertyExpression e2)
                stmt = new LuaXAssignStaticPropertyStatement(e2.ClassName, e2.PropertyName, source, location);
            else if (target is LuaXInstancePropertyExpression e3)
                stmt = new LuaXAssignInstancePropertyStatement(e3.Object, e3.PropertyName, source, location);
            else if (target is LuaXArrayAccessExpression e4)
                stmt = new LuaXAssignArrayItemStatement(e4.ArrayExpression, e4.IndexExpression, source, location);
            else if (target is LuaXInstanceCallExpression e5 && e5.MethodName == "get" &&
                e5.Arguments.Count == 1 &&
                e5.ReturnType.IsTheSame(source.ReturnType))
            {
                Metadata.Search(e5.Object.ReturnType.Class, out var @class1);
                var found = class1.SearchMethod("set", out var @method1);
                if (!found || method1.Static || method1.Visibility == LuaXVisibility.Private ||
                    !method1.ReturnType.IsVoid() ||
                     method1.Arguments.Count != 2 ||
                    !method1.Arguments[0].LuaType.IsTheSame(e5.Arguments[0].ReturnType) ||
                    !method1.Arguments[1].LuaType.IsTheSame(source.ReturnType))
                    throw new LuaXAstGeneratorException(Name, node, $"The class {class1.Name} does not have public instance method set({e5.Arguments[0].ReturnType}, {source.ReturnType}) : void");

                var expr = new LuaXInstanceCallExpression(LuaXTypeDefinition.Void, e5.Object, "set", null, location);
                expr.Arguments.Add(e5.Arguments[0]);
                expr.Arguments.Add(source);
                stmt = new LuaXCallStatement(expr, location);
            }
            else
                throw new LuaXAstGeneratorException(Name, node, "Assign to the target specified is not supported");
            statements.Add(stmt);
        }

        /// <summary>
        /// Cast expression to the type specified if it is possible
        ///
        /// The method returns the expression of the requested type or `null` if the conversion isn't possible
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private LuaXExpression CastToCompatible(LuaXExpression expression, LuaXTypeDefinition targetType)
        {
            if (expression.ReturnType.IsTheSame(targetType))
                return expression;

            if ((targetType.IsObject() || targetType.Array) && expression is LuaXConstantExpression c && c.Value.IsNil)
                return expression;

            if (expression.ReturnType.IsNumeric() && targetType.IsNumeric())
                return expression.CastTo(targetType);
            else if (targetType.IsString() && !expression.ReturnType.Array)
                return expression.CastTo(LuaXTypeDefinition.String);
            else if (targetType.IsObject() && expression.ReturnType.IsObject() && Metadata.IsKindOf(expression.ReturnType.Class, targetType.Class))
                return expression.CastTo(targetType);

            return FindCustomCast(expression, targetType);
        }

        private static bool IsMethodACastCandidate(LuaXMethod method, LuaXTypeDefinition targetType, LuaXTypeDefinition sourceType)
        {
            return method.Visibility != LuaXVisibility.Private &&
                   method.Static &&
                   method.ReturnType.IsTheSame(targetType) &&
                   method.Arguments.Count == 1 &&
                   method.Arguments[0].LuaType.IsTheSame(sourceType);
        }

        private LuaXExpression FindCustomCast(LuaXExpression expression, LuaXTypeDefinition targetType)
        {
            LuaXClass castClass = null;
            LuaXMethod castMethod = null;

            //try to find custom cast
            foreach (var @class in Metadata)
            {
                if (!@class.Attributes.Any(attribute => attribute.Name == "Cast"))
                    continue;
                castMethod = @class.Methods.FirstOrDefault(method => IsMethodACastCandidate(method, targetType, expression.ReturnType));
                if (castMethod != null)
                {
                    castClass = @class;
                    break;
                }
            }

            if (castClass != null)
            {
                var expr = new LuaXStaticCallExpression(targetType, castClass.Name, castMethod.Name, expression.Location);
                expr.Arguments.Add(expression);
                return expr;
            }

            if (expression.ReturnType.IsObject())
                return FindCustomCastForObject(expression, targetType);

            return null;
        }

        private LuaXExpression FindCustomCastForObject(LuaXExpression expression, LuaXTypeDefinition targetType)
        {
            LuaXClass parent = null;
            if (expression is LuaXConstantExpression c && c.Value.IsNil)
                parent = LuaXClass.Object;
            else
            {
                if (Metadata.Search(expression.ReturnType.Class, out var @class))
                    parent = @class.ParentClass;
            }
            if (parent != null)
                return FindCustomCastForParent(expression, parent, targetType);

            return null;
        }

        private LuaXExpression FindCustomCastForParent(LuaXExpression expression, LuaXClass targetClass, LuaXTypeDefinition targetType)
        {
            if (targetClass == null)
                return null;

            var sourceType = new LuaXTypeDefinition()
            {
                TypeId = LuaXType.Object,
                Class = targetClass.Name
            };

            LuaXClass castClass = null;
            LuaXMethod castMethod = null;

            //try to find custom cast
            foreach (var @class in Metadata)
            {
                if (!@class.Attributes.Any(attribute => attribute.Name == "Cast"))
                    continue;

                castMethod = @class.Methods.FirstOrDefault(method => IsMethodACastCandidate(method, targetType, sourceType));
                if (castMethod != null)
                {
                    castClass = @class;
                    break;
                }
            }

            if (castClass != null)
            {
                var expr = new LuaXStaticCallExpression(targetType, castClass.Name, castMethod.Name, expression.Location);
                expr.Arguments.Add(expression);
                return expr;
            }

            return FindCustomCastForParent(expression, targetClass.ParentClass, targetType);
        }

        /// <summary>
        /// Processes the class statement
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        /// <param name="method"></param>
        private void ProcessCallStatement(IAstNode node, LuaXClass @class, LuaXMethod method, LuaXStatementCollection statements)
        {
            if (node.Children.Count == 0 || node.Children[0].Symbol != "CALL")
                throw new LuaXAstGeneratorException(Name, node, "A call expression is expected here");
            var expr = ProcessExpression(node.Children[0], @class, method);
            statements.Add(new LuaXCallStatement(expr, new LuaXElementLocation(Name, node)));
        }

        /// <summary>
        /// Processes one if clause
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        /// <param name="method"></param>
        /// <param name="stmt"></param>
        private void ProcessesIfClause(IAstNode node, LuaXClass @class, LuaXMethod method, LuaXIfStatement stmt)
        {
            LuaXExpression condition = null;
            IAstNode body = null;
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "IF" || child.Symbol == "ELSEIF" || child.Symbol == "ELSE" || child.Symbol == "THEN")
                    continue;

                if (child.Symbol == "STATEMENTS")
                    body = child;
                if (child.Symbol == "EXPR")
                    condition = ProcessExpression(child, @class, method);
            }

            LuaXStatementCollection target;

            if (condition != null)
            {
                if (!condition.ReturnType.IsBoolean())
                    throw new LuaXAstGeneratorException(Name, new LuaXParserError(condition.Location, "The condition should be a boolean expression"));
                var clause = new LuaXIfClause(condition, new LuaXElementLocation(Name, node));
                target = clause.Statements;
                stmt.Clauses.Add(clause);
            }
            else
                target = stmt.ElseClause;

            if (body != null)
                ProcessStatements(body.Children, @class, method, target);
        }

        /// <summary>
        /// Processes IF statement
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        /// <param name="method"></param>
        /// <param name="statements"></param>
        private void ProcessesIfStatement(IAstNode node, LuaXClass @class, LuaXMethod method, LuaXStatementCollection statements)
        {
            LuaXIfStatement stmt = new LuaXIfStatement(new LuaXElementLocation(Name, node));
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "END")
                    break;
                ProcessesIfClause(child, @class, method, stmt);
            }
            statements.Add(stmt);
        }

        /// <summary>
        /// Processes WHILE statement
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        /// <param name="method"></param>
        /// <param name="statements"></param>
        private void ProcessesWhileStatement(IAstNode node, LuaXClass @class, LuaXMethod method, LuaXStatementCollection statements)
        {
            LuaXElementLocation location = new LuaXElementLocation(Name, node);
            LuaXExpression condition = null;
            IAstNode body = null;

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "END")
                    break;
                if (child.Symbol == "WHILE" || child.Symbol == "DO")
                    continue;
                if (child.Symbol == "STATEMENTS")
                    body = child;
                if (child.Symbol == "EXPR")
                    condition = ProcessExpression(child, @class, method);
            }

            if (condition != null)
            {
                if (!condition.ReturnType.IsBoolean())
                    throw new LuaXAstGeneratorException(Name, new LuaXParserError(condition.Location, "The while condition should be a boolean expression"));
            }
            else
                throw new LuaXAstGeneratorException(Name, new LuaXParserError(location, "The while condition should be in while statement"));

            LuaXWhileStatement stmt = new LuaXWhileStatement(location, condition);

            if (body != null)
            {
                mLoopDepth++;
                ProcessStatements(body.Children, @class, method, stmt.Statements);
                mLoopDepth--;
            }

            statements.Add(stmt);
        }

        /// <summary>
        /// Processes REPEAT statement
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        /// <param name="method"></param>
        /// <param name="statements"></param>
        private void ProcessesRepeatStatement(IAstNode node, LuaXClass @class, LuaXMethod method, LuaXStatementCollection statements)
        {
            LuaXElementLocation location = new LuaXElementLocation(Name, node);
            LuaXExpression condition = null;
            IAstNode body = null;

            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "REPEAT" || child.Symbol == "UNTIL")
                    continue;
                if (child.Symbol == "STATEMENTS")
                    body = child;
                if (child.Symbol == "EXPR")
                    condition = ProcessExpression(child, @class, method);
            }

            if (condition != null)
            {
                if (!condition.ReturnType.IsBoolean())
                    throw new LuaXAstGeneratorException(Name, new LuaXParserError(condition.Location, "The while condition should be a boolean expression"));
            }
            else
                throw new LuaXAstGeneratorException(Name, new LuaXParserError(location, "The while condition should be in while statement"));

            LuaXRepeatStatement stmt = new LuaXRepeatStatement(location, condition);

            if (body != null)
            {
                mLoopDepth++;
                ProcessStatements(body.Children, @class, method, stmt.Statements);
                mLoopDepth--;
            }

            statements.Add(stmt);
        }

        /// <summary>
        /// Processes BREAK statement
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        /// <param name="method"></param>
        /// <param name="statements"></param>
        private void ProcessesBreakStatement(IAstNode node, LuaXStatementCollection statements)
        {
            LuaXBreakStatement stmt = new LuaXBreakStatement(new LuaXElementLocation(Name, node));
            if (mLoopDepth <= 0)
                throw new LuaXAstGeneratorException(Name, new LuaXParserError(stmt.Location, "The break statement is not in a loop"));
            statements.Add(stmt);
        }

        /// <summary>
        /// Processes CONTINUE statement
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        /// <param name="method"></param>
        /// <param name="statements"></param>
        private void ProcessesContinueStatement(IAstNode node, LuaXStatementCollection statements)
        {
            LuaXContinueStatement stmt = new LuaXContinueStatement(new LuaXElementLocation(Name, node));
            if (mLoopDepth <= 0)
                throw new LuaXAstGeneratorException(Name, new LuaXParserError(stmt.Location, "The continue statement is not in a loop"));
            statements.Add(stmt);
        }

        /// <summary>
        /// Processes RETURN statement
        /// </summary>
        /// <param name="node"></param>
        /// <param name="class"></param>
        /// <param name="method"></param>
        /// <param name="statements"></param>
        private void ProcessesReturnStatement(IAstNode node, LuaXClass @class, LuaXMethod method, LuaXStatementCollection statements)
        {
            LuaXExpression expression = null;
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Symbol == "REXPR")
                    expression = ProcessExpression(child, @class, method);
            }
            if (expression != null && method.ReturnType.TypeId == LuaXType.Void)
                throw new LuaXAstGeneratorException(Name, node, "A void method should not return a value");
            if (expression == null && method.ReturnType.TypeId != LuaXType.Void)
                throw new LuaXAstGeneratorException(Name, node, "A non-void method should return a value");
            if (expression?.ReturnType.IsTheSame(method.ReturnType) == false)
            {
                expression = CastToCompatible(expression, method.ReturnType) ??
                    throw new LuaXAstGeneratorException(Name, node, $"The return value type {expression.ReturnType} and the method type {method.ReturnType} are incompatible");
            }
            statements.Add(new LuaXReturnStatement(expression, new LuaXElementLocation(Name, node)));
        }

        public void ProcessConstantDeclarationInMethod(IAstNode node, LuaXMethod method)
        {
            var decl = ProcessConstantDeclaration(node);

            if (method.Constants.Contains(decl.Name))
                throw new LuaXAstGeneratorException(Name, node, "The constant with the name specified is already defined");

            if (method.Arguments.Contains(decl.Name))
                throw new LuaXAstGeneratorException(Name, node, "The argument with the name specified is already defined");

            if (method.Variables.Contains(decl.Name))
                throw new LuaXAstGeneratorException(Name, node, "The variable with the name specified is already defined");

            method.Constants.Add(decl);
        }
    }
}
