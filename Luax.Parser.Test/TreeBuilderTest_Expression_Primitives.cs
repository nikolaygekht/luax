using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using FluentAssertions;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Builder;
using Luax.Parser.Ast.LuaExpression;
using Luax.Parser.Test.Tools;
using Xunit;

namespace Luax.Parser.Test
{
    public class TreeBuilderTest_Expression_Primitives
    {
        [Theory]
        [InlineData("[CONSTANT[NIL]]", LuaXType.Object, null)]
        [InlineData("[CONSTANT[INTEGER(5)]]", LuaXType.Integer, 5)]
        [InlineData("[CONSTANT[INTEGER(1_234)]]", LuaXType.Integer, 1234)]
        [InlineData("[CONSTANT[HEX_INTEGER(0x1bc)]]", LuaXType.Integer, 0x1bc)]
        [InlineData("[CONSTANT[HEX_INTEGER(0x1_bc)]]", LuaXType.Integer, 0x1bc)]
        [InlineData("[CONSTANT[NEGATIVE_INTEGER[MINUS_OP[-(-)]][INTEGER(5)]]]", LuaXType.Integer, -5)]
        [InlineData("[CONSTANT[REAL(5.1)]]", LuaXType.Real, 5.1)]
        [InlineData("[CONSTANT[REAL(1234_5678.1e22)]]", LuaXType.Real, 12345678.1e22)]
        [InlineData("[CONSTANT[NEGATIVE_REAL[MINUS_OP[-(-)]][REAL(5.1)]]]", LuaXType.Real, -5.1)]
        [InlineData("[CONSTANT[BOOLEAN[BOOLEAN_TRUE]]]", LuaXType.Boolean, true)]
        [InlineData("[CONSTANT[BOOLEAN[BOOLEAN_FALSE]]]", LuaXType.Boolean, false)]
        [InlineData("[CONSTANT[STRING[STRINGDQ(\"abcd\")]]]", LuaXType.String, "abcd")]
        [InlineData("[CONSTANT[STRING[STRINGDQ(\"\")]]]", LuaXType.String, "")]
        [InlineData("[CONSTANT[STRING[STRINGDQ(\"\\\\x\")]]]", LuaXType.String, "\\x")]
        [InlineData("[CONSTANT[STRING[STRINGDQ(\"\\r\")]]]", LuaXType.String, "\r")]
        [InlineData("[CONSTANT[STRING[STRINGDQ(\"\\n\")]]]", LuaXType.String, "\n")]
        [InlineData("[CONSTANT[STRING[STRINGDQ(\"\\t\")]]]", LuaXType.String, "\t")]
        public void ParseConstant_Successful(string tree, LuaXType type, object value)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");

            var constant = processor.ProcessConstant(node);
            constant.Should().NotBeNull();
            constant.ConstantType.Should().Be(type);
            if (value != null)
            {
                constant.Value.Should().Be(value);
                constant.IsNil.Should().BeFalse();
            }
            else
            {
                constant.Value.Should().BeNull();
                constant.IsNil.Should().BeTrue();
            }
        }

        [Fact]
        public void ConstantExpression()
        {
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT[INTEGER(1)]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("");
            var expr = processor.ProcessExpression(node, null, null);
            expr.Should().BeOfType<LuaXConstantExpression>();
            var ce = expr.As<LuaXConstantExpression>();
            ce.Value.Value.Should().Be(1);
            ce.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            ce.ReturnType.Class.Should().BeNullOrEmpty();
            ce.ReturnType.Array.Should().BeFalse();
        }

        private static void StageVariableAndProperty(out LuaXClassCollection metadata, out LuaXClass @class, out LuaXMethod method)
        {
            metadata = new LuaXClassCollection();

            var lib = new LuaXClass("lib", null);

            @lib.Properties.Add(new LuaXProperty()
            {
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Integer
                },
                Name = "publicStaticProperty",
                Static = true,
                Visibility = LuaXVisibility.Public,
            });

            @lib.Properties.Add(new LuaXProperty()
            {
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Integer
                },
                Name = "privateStaticProperty",
                Static = true,
                Visibility = LuaXVisibility.Private,
            });

            @lib.Properties.Add(new LuaXProperty()
            {
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Integer
                },
                Name = "publicProperty",
                Static = false,
                Visibility = LuaXVisibility.Public,
            });

            @lib.Properties.Add(new LuaXProperty()
            {
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Integer
                },
                Name = "privateProperty",
                Static = false,
                Visibility = LuaXVisibility.Private,
            });

            @class = new LuaXClass("class1", null);

            @class.Properties.Add(new LuaXProperty()
            {
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Integer
                },
                Name = "publicStaticProperty",
                Static = true,
                Visibility = LuaXVisibility.Public,
            });

            @class.Properties.Add(new LuaXProperty()
            {
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Integer
                },
                Name = "privateStaticProperty",
                Static = true,
                Visibility = LuaXVisibility.Private,
            });

            @class.Properties.Add(new LuaXProperty()
            {
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Integer
                },
                Name = "publicProperty",
                Static = false,
                Visibility = LuaXVisibility.Public,
            });

            @class.Properties.Add(new LuaXProperty()
            {
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Integer
                },
                Name = "privateProperty",
                Static = false,
                Visibility = LuaXVisibility.Private,
            });

            @class.Properties.Add(new LuaXProperty()
            {
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Object,
                    Class = "class1",
                    Array = false,
                },
                Name = "selfReference",
                Static = false,
                Visibility = LuaXVisibility.Private,
            });

            metadata.Add(lib);
            metadata.Add(@class);

            method = new LuaXMethod()
            {
                Name = "test",
                ReturnType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Void
                }
            };

            method.Arguments.Add(new LuaXVariable()
            {
                Name = "arg1",
                LuaType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Integer
                },
            });

            method.Variables.Add(
                new LuaXVariable()
                {
                    Name = "v1",
                    LuaType = new LuaXTypeDefinition()
                    {
                        TypeId = LuaXType.Integer
                    },
                });

            method.Variables.Add(
               new LuaXVariable()
               {
                   Name = "v2",
                   LuaType = new LuaXTypeDefinition()
                   {
                       TypeId = LuaXType.Integer,
                       Array = true,
                   },
               });

            method.Variables.Add(
               new LuaXVariable()
               {
                   Name = "v3",
                   LuaType = new LuaXTypeDefinition()
                   {
                       TypeId = LuaXType.Real
                   },
               });

            method.Variables.Add(
               new LuaXVariable()
               {
                   Name = "v4",
                   LuaType = new LuaXTypeDefinition()
                   {
                       TypeId = LuaXType.String
                   },
               });

            method.Variables.Add(
               new LuaXVariable()
               {
                   Name = "v5",
                   LuaType = new LuaXTypeDefinition()
                   {
                       TypeId = LuaXType.Object,
                       Class = "class1",
                   },
               });

            method.Variables.Add(
               new LuaXVariable()
               {
                   Name = "v6",
                   LuaType = new LuaXTypeDefinition()
                   {
                       TypeId = LuaXType.Object,
                       Class = "lib",
                   },
               });

            @class.Methods.Add(method);
        }

        [Fact]
        public void Variable_AsArgument_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(arg1)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXVariableExpression>();
            var e = expression.As<LuaXVariableExpression>();
            e.Name.Should().Be("arg1");
            e.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            e.ReturnType.Array.Should().BeFalse();
        }

        [Fact]
        public void Variable_AsVariable_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v2)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXVariableExpression>();
            var e = expression.As<LuaXVariableExpression>();
            e.Name.Should().Be("v2");
            e.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            e.ReturnType.Array.Should().BeTrue();
        }

        [Fact]
        public void Variable_AsVariable_This_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(this)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXVariableExpression>();
            var e = expression.As<LuaXVariableExpression>();
            e.Name.Should().Be("this");
            e.ReturnType.TypeId.Should().Be(LuaXType.Object);
            e.ReturnType.Class.Should().Be(@class.Name);
            e.ReturnType.Array.Should().BeFalse();
        }

        [Fact]
        public void Variable_AsVariable_This_FromStaticMethod_Failed()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var method1 = new LuaXMethod()
            {
                Static = true
            };
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(this)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method1))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Variable_AsInstanceProperty_OwnClass_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(privateProperty)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXPropertyExpression>();
            var e = expression.As<LuaXPropertyExpression>();
            e.PropertyName.Should().Be("privateProperty");
            e.ReturnType.TypeId.Should().Be(LuaXType.Integer);

            e.Object.Should().BeOfType<LuaXVariableExpression>();
            e.Object.ReturnType.TypeId.Should().Be(LuaXType.Object);
            e.Object.ReturnType.Class.Should().Be(@class.Name);
            e.Object.As<LuaXVariableExpression>().Name.Should().Be("this");
        }

        [Fact]

        public void Variable_AsInstanceProperty_OwnClass_FromStaticMethod_Failed()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);

            var method1 = new LuaXMethod() { Static = true };

            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(privateProperty)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method1))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Variable_AsStaticProperty_OwnClass_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(privateStaticProperty)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXStaticPropertyExpression>();
            var e = expression.As<LuaXStaticPropertyExpression>();
            e.ClassName.Should().Be(@class.Name);
            e.PropertyName.Should().Be("privateStaticProperty");
        }

        [Fact]
        public void Variable_AsClassName_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(lib)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXClassNameExpression>();
            var e = expression.As<LuaXClassNameExpression>();
            e.Name.Should().Be("lib");
            e.ReturnType.TypeId.Should().Be(LuaXType.ClassName);
            e.ReturnType.Class.Should().Be("lib");
        }

        [Fact]
        public void Variable_Failure_NotFound()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(unknown)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        private static string PropertyAccessTree(string leftSideId, string rightSideId)
            => $"[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[PROPERTY[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER({leftSideId})]]]][PROPERTY_ACCESS[.(.)]][IDENTIFIER({rightSideId})]]]]]]]]]]]]]]]";

        [Fact]
        public void Property_Instance_Private_OwnClass_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("this", "privateProperty"));
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXPropertyExpression>();
            var e = expression.As<LuaXPropertyExpression>();
            e.PropertyName.Should().Be("privateProperty");
            e.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            e.Object.ReturnType.TypeId.Should().Be(LuaXType.Object);
            e.Object.ReturnType.Class.Should().Be("class1");
        }

        [Fact]
        public void Property_Instance_Public_OtherClass_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("v6", "publicProperty"));
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXPropertyExpression>();
            var e = expression.As<LuaXPropertyExpression>();
            e.PropertyName.Should().Be("publicProperty");
            e.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            e.Object.ReturnType.TypeId.Should().Be(LuaXType.Object);
            e.Object.ReturnType.Class.Should().Be("lib");
        }

        [Fact]
        public void Property_Instance_Private_OtherClass_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("v6", "privateProperty"));
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Property_Static_Private_OwnClass_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("class1", "privateStaticProperty"));
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXStaticPropertyExpression>();
            var e = expression.As<LuaXStaticPropertyExpression>();
            e.PropertyName.Should().Be("privateStaticProperty");
            e.ClassName.Should().Be("class1");
        }

        [Fact]
        public void Property_Static_NotExist_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("class1", "notExist"));
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Property_Static_AsInstance_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("v6", "publicStaticProperty"));
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Property_Instance_AsStatic_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("lib", "publicProperty"));
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Property_Instance_NotExist_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("v6", "notExist"));
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Property_Static_Public_OtherClass_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("lib", "publicStaticProperty"));
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXStaticPropertyExpression>();
            var e = expression.As<LuaXStaticPropertyExpression>();
            e.PropertyName.Should().Be("publicStaticProperty");
            e.ClassName.Should().Be("lib");
        }

        [Fact]
        public void Property_Static_Private_OtherClass_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("lib", "privateStaticProperty"));
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Property_LengthOfArray_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("v2", "length"));
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXArrayLengthExpression>();
            var e = expression.As<LuaXArrayLengthExpression>();
            e.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            e.ArrayExpression.Should().BeOfType<LuaXVariableExpression>();
            e.ArrayExpression.As<LuaXVariableExpression>().Name.Should().Be("v2");
        }

        [Fact]
        public void Property_OfArray_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("v2", "count"));
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Cast_ToString_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[CAST_OP[CAST[cast(cast)]][L_SHARP_BRACKET[<(<)]][TYPE_NAME[TYPE_STRING[string(string)]]][R_SHARP_BRACKET[>(>)]][L_ROUND_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v1)]]]]]]]]]]]]]]][R_ROUND_BRACKET]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXCastOperatorExpression>();
            var cast = expression.As<LuaXCastOperatorExpression>();
            cast.ReturnType.TypeId.Should().Be(LuaXType.String);
            cast.ReturnType.Class.Should().BeNullOrEmpty();
            cast.ReturnType.Array.Should().BeFalse();
            cast.Argument.Should().BeOfType<LuaXVariableExpression>();
            var arg = cast.Argument.As<LuaXVariableExpression>();
            arg.Name.Should().Be("v1");
        }

        [Fact]
        public void Cast_ToClass_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[CAST_OP[CAST[cast(cast)]][L_SHARP_BRACKET[<(<)]][TYPE_NAME[IDENTIFIER(tuple)]][R_SHARP_BRACKET[>(>)]][L_ROUND_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v1)]]]]]]]]]]]]]]][R_ROUND_BRACKET]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXCastOperatorExpression>();
            var cast = expression.As<LuaXCastOperatorExpression>();
            cast.ReturnType.TypeId.Should().Be(LuaXType.Object);
            cast.ReturnType.Class.Should().Be("tuple");
            cast.ReturnType.Array.Should().BeFalse();
            cast.Argument.Should().BeOfType<LuaXVariableExpression>();
            var arg = cast.Argument.As<LuaXVariableExpression>();
            arg.Name.Should().Be("v1");
        }

        [Fact]
        public void Cast_ToVoid_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[CAST_OP[CAST[cast(cast)]][L_SHARP_BRACKET[<(<)]][TYPE_NAME[TYPE_VOID[void(void)]]][R_SHARP_BRACKET[>(>)]][L_ROUND_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v1)]]]]]]]]]]]]]]][R_ROUND_BRACKET]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ArrayIndex_Integer_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[ARRAY_ACCESS[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v2)]]]]]]]]]]]]]]][L_SQUARE_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT[INTEGER(1)]]]]]]]]]]]]][R_SQUARE_BRACKET]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXArrayAccessExpression>();
            var access = expression.As<LuaXArrayAccessExpression>();

            access.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            access.ReturnType.Array.Should().BeFalse();

            access.IndexExpression.Should().BeOfType<LuaXConstantExpression>();
            access.IndexExpression.As<LuaXConstantExpression>().Value.Value.Should().Be(1);
        }

        [Fact]
        public void ArrayIndex_Real_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[ARRAY_ACCESS[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v2)]]]]]]]]]]]]]]][L_SQUARE_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT[REAL(1.1)]]]]]]]]]]]]][R_SQUARE_BRACKET]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXArrayAccessExpression>();
            var access = expression.As<LuaXArrayAccessExpression>();

            access.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            access.ReturnType.Array.Should().BeFalse();

            access.IndexExpression.Should().BeOfType<LuaXCastOperatorExpression>();
            var cast = access.IndexExpression.As<LuaXCastOperatorExpression>();
            cast.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            cast.ReturnType.Array.Should().BeFalse();
            cast.Argument.Should().BeOfType<LuaXConstantExpression>();
            cast.Argument.As<LuaXConstantExpression>().Value.Value.Should().Be(1.1);
        }

        [Fact]
        public void ArrayIndex_NotNumericIndex_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[ARRAY_ACCESS[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v2)]]]]]]]]]]]]]]][L_SQUARE_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v4)]]]]]]]]]]]]]]][R_SQUARE_BRACKET]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ArrayIndex_IndexingNotArray_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[ARRAY_ACCESS[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v1)]]]]]]]]]]]]]]][L_SQUARE_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT[INTEGER(1)]]]]]]]]]]]]][R_SQUARE_BRACKET]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }
    }
}
