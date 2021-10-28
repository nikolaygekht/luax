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
        /// Processes the method body
        /// </summary>
        /// <param name="node"></param>
        /// <param name="method"></param>
        public void ProcessBody(IAstNode node, LuaXClass @class, LuaXMethod method)
            => ProcessStatements(node.Children, @class, method, method.Statements);

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
                    case "CALL_STMT":
                        ProcessCallStatement(child, @class, method, statements);
                        break;
                    case "ASSIGN_STMT":
                        ProcessAssignStatement(child, @class, method, statements);
                        break;
                    case "IF_STMT":
                        ProcessesIfStatement(child, @class, method, statements);
                        break;
                    case "RETURN_STMT":
                        ProcessesReturnStatement(child, @class, method, statements);
                        break;
                    default:
                        throw new LuaXAstGeneratorException(Name, child, $"Unexpected symbol {child.Symbol}");
                }
            }
        }

        private void ProcessDeclarationStatement(IAstNode node, LuaXMethod method)
        {
            if (node.Children.Count < 2 || node.Children[1].Symbol != "DECL_LIST")
                throw new LuaXAstGeneratorException(Name, node, "One or more DECL is expected here");

            ProcessDeclarationList<LuaXVariable>(node.Children[1], new LuaXVariableFactory<LuaXVariable>(), v =>
            {
                if (method.Arguments.Contains(v.Name) || method.Variables.Contains(v.Name))
                    throw new LuaXAstGeneratorException(Name, node, $"Variable {v.Name} already exists");
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

            if (!target.ReturnType.Equals(source.ReturnType))
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
            else
                throw new LuaXAstGeneratorException(Name, node, "Assigned the target specified is not supported");
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
            if (expression.ReturnType.Equals(targetType))
                return expression;

            if (expression.ReturnType.IsNumeric() && targetType.IsNumeric())
                return expression.CastTo(targetType);
            else if (targetType.IsString() && !expression.ReturnType.Array)
                return expression.CastTo(LuaXTypeDefinition.String);
            else if (targetType.IsObject() && expression.ReturnType.IsObject() && Metadata.IsKindOf(expression.ReturnType.Class, targetType.Class))
                return expression.CastTo(targetType);

            return null;
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
            if (expression != null && !expression.ReturnType.Equals(method.ReturnType))
            {
                expression = CastToCompatible(expression, method.ReturnType) ??
                    throw new LuaXAstGeneratorException(Name, node, $"The return value type {expression.ReturnType} and the method type {method.ReturnType} are incompatible");
            }
            statements.Add(new LuaXReturnStatement(expression, new LuaXElementLocation(Name, node)));
        }
    }
}
