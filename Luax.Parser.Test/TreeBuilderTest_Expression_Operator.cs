using System;
using FluentAssertions;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Builder;
using Luax.Parser.Ast.LuaExpression;
using Luax.Parser.Test.Tools;
using Xunit;

namespace Luax.Parser.Test
{
    public class TreeBuilderTest_Expression_Operator
    {
        private static void StageVariableAndProperty(out LuaXClassCollection metadata, out LuaXClass @class, out LuaXMethod method)
        {
            metadata = new LuaXClassCollection();
            @class = new LuaXClass("class1");
            metadata.Add(@class);

            method = new LuaXMethod()
            {
                Name = "test",
                ReturnType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Void
                }
            };

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vi1",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Integer
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vi2",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Integer
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vr1",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Real
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vr2",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Real
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vs1",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.String
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vs2",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.String
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vb1",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Boolean
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vb2",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Boolean
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vd1",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Datetime
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "vd2",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Datetime
                    },
                });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "va",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Integer,
                        Array = true,
                    },
                });
            @class.Methods.Add(method);
        }

        [Theory]
        [InlineData("vi1", LuaXType.Integer)]
        [InlineData("vr1", LuaXType.Real)]
        public void Negate_Success(string variable, LuaXType type)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[MINUS_OP[-(-)][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable})]]]]]]]]]]]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            var expr = builder.ProcessExpression(node, @class, method);
            expr.Should().BeOfType<LuaXUnaryOperatorExpression>();
            var op = expr.As<LuaXUnaryOperatorExpression>();
            op.Operator.Should().Be(LuaXUnaryOperator.Minus);
            op.ReturnType.TypeId.Should().Be(type);
            op.Argument.Should().BeOfType<LuaXVariableExpression>();
            op.Argument.As<LuaXVariableExpression>().VariableName.Should().Be(variable);
        }

        [Theory]
        [InlineData("vs1")]
        [InlineData("vb1")]
        [InlineData("vd1")]
        public void Negate_Fail(string variable)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[MINUS_OP[-(-)][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable})]]]]]]]]]]]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => builder.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Theory]
        [InlineData("vb1")]
        public void Not_Success(string variable)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[NOT_OP[not(not)][REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable})]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            var expr = builder.ProcessExpression(node, @class, method);
            expr.Should().BeOfType<LuaXUnaryOperatorExpression>();
            var op = expr.As<LuaXUnaryOperatorExpression>();
            op.Operator.Should().Be(LuaXUnaryOperator.Not);
            op.ReturnType.TypeId.Should().Be(LuaXType.Boolean);
            op.Argument.Should().BeOfType<LuaXVariableExpression>();
            op.Argument.As<LuaXVariableExpression>().VariableName.Should().Be(variable);
        }

        [Theory]
        [InlineData("vi1")]
        [InlineData("vr1")]
        [InlineData("vs1")]
        [InlineData("vd1")]
        [InlineData("va1")]
        [InlineData("va2")]
        public void Not_Fail(string variable)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[NOT_OP[not(not)][REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable})]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => builder.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Theory]
        [InlineData("vs1", "vs2", false, false)]
        [InlineData("vi1", "vi2", true, true)]
        [InlineData("vs1", "vi2", false, true)]
        [InlineData("vr1", "vs2", true, false)]
        public void Concat_Success(string variable1, string variable2, bool cast1, bool cast2)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[CONCAT_OP[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable1})]]]]]]]]][..(..)][MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable2})]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            var expr = builder.ProcessExpression(node, @class, method);
            expr.Should().BeOfType<LuaXBinaryOperatorExpression>();
            var op = expr.As<LuaXBinaryOperatorExpression>();
            op.Operator.Should().Be(LuaXBinaryOperator.Concat);
            op.ReturnType.TypeId.Should().Be(LuaXType.String);

            var a1 = op.LeftArgument;
            var a2 = op.RightArgument;

            if (cast1)
            {
                a1.Should().BeOfType<LuaXCastOperatorExpression>();
                a1 = op.LeftArgument.As<LuaXCastOperatorExpression>().Argument;
            }

            if (cast2)
            {
                a2.Should().BeOfType<LuaXCastOperatorExpression>();
                a2 = op.RightArgument.As<LuaXCastOperatorExpression>().Argument;
            }

            a1.Should().BeOfType<LuaXVariableExpression>();
            a1.As<LuaXVariableExpression>().VariableName.Should().Be(variable1);
            a2.Should().BeOfType<LuaXVariableExpression>();
            a2.As<LuaXVariableExpression>().VariableName.Should().Be(variable2);
        }

        [Theory]
        [InlineData("AND_OP", LuaXBinaryOperator.And)]
        [InlineData("OR_OP", LuaXBinaryOperator.Or)]
        public void LogicalOp_Success(string symbol, LuaXBinaryOperator binary)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[{symbol}[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(vb1)]]]]]]]]]]]][and(and)][UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(vb2)]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            var expr = builder.ProcessExpression(node, @class, method);
            expr.Should().BeOfType<LuaXBinaryOperatorExpression>();
            var op = expr.As<LuaXBinaryOperatorExpression>();
            op.Operator.Should().Be(binary);
            op.ReturnType.TypeId.Should().Be(LuaXType.Boolean);

            var a1 = op.LeftArgument;
            var a2 = op.RightArgument;

            a1.Should().BeOfType<LuaXVariableExpression>();
            a1.As<LuaXVariableExpression>().VariableName.Should().Be("vb1");
            a2.Should().BeOfType<LuaXVariableExpression>();
            a2.As<LuaXVariableExpression>().VariableName.Should().Be("vb2");
        }

        [Theory]
        [InlineData("AND_OP", "vi1", "vi2")]
        [InlineData("AND_OP", "vb1", "vi2")]
        [InlineData("OR_OP", "vr1", "vb2")]
        [InlineData("OR_OP", "vs1", "vd2")]
        public void LogicalOp_Fail(string symbol, string variable1, string variable2)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[{symbol}[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable1})]]]]]]]]]]]][and(and)][UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable2})]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => builder.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Theory]
        [InlineData("EQ_OP", "==", LuaXBinaryOperator.Equal, "vi1", "vi2")]
        [InlineData("EQ_OP", "==", LuaXBinaryOperator.Equal, "vi1", "vr2")]
        [InlineData("EQ_OP", "==", LuaXBinaryOperator.Equal, "vr1", "vi2")]
        [InlineData("EQ_OP", "==", LuaXBinaryOperator.Equal, "vr1", "vr2")]
        [InlineData("EQ_OP", "==", LuaXBinaryOperator.Equal, "vs1", "vs2")]
        [InlineData("EQ_OP", "==", LuaXBinaryOperator.Equal, "vb1", "vb2")]
        [InlineData("EQ_OP", "==", LuaXBinaryOperator.Equal, "vd1", "vd2")]
        [InlineData("NEQ_OP", "~=", LuaXBinaryOperator.NotEqual, "vi1", "vi2")]
        [InlineData("GT_OP", ">", LuaXBinaryOperator.Greater, "vi1", "vi2")]
        [InlineData("GE_OP", ">=", LuaXBinaryOperator.GreaterOrEqual, "vi1", "vi2")]
        [InlineData("LT_OP", "<", LuaXBinaryOperator.Less, "vi1", "vi2")]
        [InlineData("LE_OP", "<=", LuaXBinaryOperator.LessOrEqual, "vi1", "vi2")]
        public void RealitveOp_Success(string symbol, string value, LuaXBinaryOperator binary, string variable1, string variable2)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[{symbol}[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable1})]]]]]]]]]][{value}({value})][ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable2})]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            var expr = builder.ProcessExpression(node, @class, method);
            expr.Should().BeOfType<LuaXBinaryOperatorExpression>();
            var op = expr.As<LuaXBinaryOperatorExpression>();
            op.Operator.Should().Be(binary);
            op.ReturnType.TypeId.Should().Be(LuaXType.Boolean);

            var a1 = op.LeftArgument;
            var a2 = op.RightArgument;

            a1.Should().BeOfType<LuaXVariableExpression>();
            a1.As<LuaXVariableExpression>().VariableName.Should().Be(variable1);
            a2.Should().BeOfType<LuaXVariableExpression>();
            a2.As<LuaXVariableExpression>().VariableName.Should().Be(variable2);
        }

        [Theory]
        [InlineData("EQ_OP", "==", "vi1", "vs2")]
        [InlineData("EQ_OP", "==", "vr1", "vs2")]
        [InlineData("EQ_OP", "==", "vi1", "vb2")]
        [InlineData("EQ_OP", "==", "vb1", "vs2")]
        [InlineData("EQ_OP", "==", "va1", "vi2")]
        [InlineData("EQ_OP", "==", "va1", "va2")]
        public void RealitveOp_Fail(string symbol, string value, string variable1, string variable2)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[{symbol}[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable1})]]]]]]]]]][{value}({value})][ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable2})]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => builder.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Theory]
        [InlineData("PLUS_OP", "+", LuaXBinaryOperator.Add, LuaXType.Integer, "vi1", "vi2")]
        [InlineData("PLUS_OP", "+", LuaXBinaryOperator.Add, LuaXType.Real, "vr1", "vr2")]
        [InlineData("MINUS_OP", "-", LuaXBinaryOperator.Subtract, LuaXType.Real, "vr1", "vr2")]
        [InlineData("MINUS_OP", "-", LuaXBinaryOperator.Subtract, LuaXType.Real, "vi1", "vr2")]
        [InlineData("MUL_OP", "*", LuaXBinaryOperator.Multiply, LuaXType.Real, "vr1", "vr2")]
        [InlineData("DIV_OP", "/", LuaXBinaryOperator.Divide, LuaXType.Real, "vr1", "vr2")]
        [InlineData("REM_OP", "%", LuaXBinaryOperator.Reminder, LuaXType.Real, "vr1", "vr2")]
        [InlineData("POWER_OP", "^", LuaXBinaryOperator.Power, LuaXType.Real, "vr1", "vr2")]

        public void MathOp_Success(string symbol, string value, LuaXBinaryOperator binary, LuaXType resultType, string variable1, string variable2)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[{symbol}[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable1})]]]]]]]]][{value}({value})][MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable2})]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            var expr = builder.ProcessExpression(node, @class, method);
            expr.Should().BeOfType<LuaXBinaryOperatorExpression>();
            var op = expr.As<LuaXBinaryOperatorExpression>();
            op.Operator.Should().Be(binary);
            op.ReturnType.TypeId.Should().Be(resultType);

            var a1 = op.LeftArgument;
            var a2 = op.RightArgument;

            a1.Should().BeOfType<LuaXVariableExpression>();
            a1.As<LuaXVariableExpression>().VariableName.Should().Be(variable1);
            a2.Should().BeOfType<LuaXVariableExpression>();
            a2.As<LuaXVariableExpression>().VariableName.Should().Be(variable2);
        }

        [Theory]
        [InlineData("PLUS_OP", "+", "vs1", "vs2")]
        [InlineData("PLUS_OP", "+", "vd1", "vd2")]
        [InlineData("PLUS_OP", "+", "vb1", "vb2")]
        [InlineData("PLUS_OP", "+", "va1", "va2")]
        [InlineData("PLUS_OP", "+", "vi1", "vs2")]
        [InlineData("PLUS_OP", "+", "vs1", "vr2")]

        public void MathOp_Fail(string symbol, string value, string variable1, string variable2)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse($"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[{symbol}[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable1})]]]]]]]]][{value}({value})][MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({variable2})]]]]]]]]]]]]]]]");
            var builder = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => builder.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }
    }
}

