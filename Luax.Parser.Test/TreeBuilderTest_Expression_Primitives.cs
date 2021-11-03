using System;
using System.Linq.Expressions;
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
        [InlineData("[CONSTANT[NIL[nil(nil)]]]", LuaXType.Object, null)]
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

        private static void StageVariableAndProperty(out LuaXClassCollection metadata, out LuaXClass @class, out LuaXMethod method, LuaXTypeDefinition methodArgType = null)
        {
            metadata = new LuaXClassCollection();

            var lib = new LuaXClass("lib");

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

            lib.Constants.Add(new LuaXConstantVariable()
            {
                Name = "pi",
                Value = new LuaXConstant(3.14, new LuaXElementLocation("", 0, 0))
            });

            @class = new LuaXClass("class1");

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

            method = new LuaXMethod(@class)
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
                LuaType = methodArgType ?? LuaXTypeDefinition.Integer
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

            var method1 = new LuaXMethod(@class)
            {
                Name = "staticMethod",
                Static = true,
                ReturnType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Void
                }
            };
            @class.Methods.Add(method1);

            var method2 = new LuaXMethod(@class)
            {
                Name = "instanceMethod",
                ReturnType = new LuaXTypeDefinition()
                {
                    TypeId = LuaXType.Void
                }
            };
            @class.Methods.Add(method2);
            metadata.Search("xxx", out _);  //force index creation
        }

        [Fact]
        public void Variable_AsArgument_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(arg1)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXArgumentExpression>();
            var e = expression.As<LuaXArgumentExpression>();
            e.ArgumentName.Should().Be("arg1");
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
            e.VariableName.Should().Be("v2");
            e.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            e.ReturnType.Array.Should().BeTrue();
        }

        [Fact]
        public void Variable_AsMethodConstant_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            method.Constants.Add(new LuaXConstantVariable() { Name = "c2", Value = new LuaXConstant(10, new LuaXElementLocation("", 0, 0)) });
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(c2)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXConstantExpression>();
            var e = expression.As<LuaXConstantExpression>();
            e.Value.ConstantType.Should().Be(LuaXType.Integer);
            e.Value.Value.Should().Be(10);
        }

        [Fact]
        public void Variable_AsClassConstant_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            @class.Constants.Add(new LuaXConstantVariable() { Name = "c2", Value = new LuaXConstant(10, new LuaXElementLocation("", 0, 0)) });
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(c2)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXConstantExpression>();
            var e = expression.As<LuaXConstantExpression>();
            e.Value.ConstantType.Should().Be(LuaXType.Integer);
            e.Value.Value.Should().Be(10);
        }

        [Fact]
        public void Variable_AsSuperClassConstant_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            @class.Constants.Add(new LuaXConstantVariable() { Name = "c2", Value = new LuaXConstant(10, new LuaXElementLocation("", 0, 0)) });
            var @class1 = new LuaXClass("z", @class.Name, new LuaXElementLocation("", 0, 0));
            class1.ParentClass = @class;
            var method1 = new LuaXMethod(class1);

            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(c2)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class1, method1);
            expression.Should().BeOfType<LuaXConstantExpression>();
            var e = expression.As<LuaXConstantExpression>();
            e.Value.ConstantType.Should().Be(LuaXType.Integer);
            e.Value.Value.Should().Be(10);
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
            e.VariableName.Should().Be("this");
            e.ReturnType.TypeId.Should().Be(LuaXType.Object);
            e.ReturnType.Class.Should().Be(@class.Name);
            e.ReturnType.Array.Should().BeFalse();
        }

        [Fact]
        public void Variable_AsVariable_Super_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var _);
            var class1 = new LuaXClass("test2", @class.Name, null);
            var method1 = new LuaXMethod(@class1) { Name = "test" };
            class1.Methods.Add(method1);
            metadata.Add(class1);
            metadata.Search("xxx", out _);  //force index creation

            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(super)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, class1, method1);
            expression.Should().BeOfType<LuaXVariableExpression>();
            var e = expression.As<LuaXVariableExpression>();
            e.VariableName.Should().Be("super");
            e.ReturnType.TypeId.Should().Be(LuaXType.Object);
            e.ReturnType.Class.Should().Be(@class.Name);
            e.ReturnType.Array.Should().BeFalse();
        }

        [Fact]
        public void Variable_AsVariable_Super_FromStatic_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var class1 = new LuaXClass("test2", @class.Name, null);
            var method1 = new LuaXMethod(@class1) { Name = "test", Static = true };
            class1.Methods.Add(method1);
            metadata.Add(class1);
            metadata.Search("xxx", out _);  //force index creation

            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(super)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method1))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Variable_AsVariable_Super_NoParent_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var class1 = LuaXClass.Object;
            var method1 = new LuaXMethod(@class1) { Name = "test", };
            class1.Methods.Add(method1);
            metadata.Add(class1);
            metadata.Search("xxx", out _);  //force index creation

            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(super)]]]]]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, class1, method1))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void Variable_AsVariable_This_FromStaticMethod_Failed()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var method1 = new LuaXMethod(@class)
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
            expression.Should().BeOfType<LuaXInstancePropertyExpression>();
            var e = expression.As<LuaXInstancePropertyExpression>();
            e.PropertyName.Should().Be("privateProperty");
            e.ReturnType.TypeId.Should().Be(LuaXType.Integer);

            e.Object.Should().BeOfType<LuaXVariableExpression>();
            e.Object.ReturnType.TypeId.Should().Be(LuaXType.Object);
            e.Object.ReturnType.Class.Should().Be(@class.Name);
            e.Object.As<LuaXVariableExpression>().VariableName.Should().Be("this");
        }

        [Fact]

        public void Variable_AsInstanceProperty_OwnClass_FromStaticMethod_Failed()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);

            var method1 = new LuaXMethod(@class) { Static = true };

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
            expression.Should().BeOfType<LuaXInstancePropertyExpression>();
            var e = expression.As<LuaXInstancePropertyExpression>();
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
            expression.Should().BeOfType<LuaXInstancePropertyExpression>();
            var e = expression.As<LuaXInstancePropertyExpression>();
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
        public void Property_Constant_OtherClass_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse(PropertyAccessTree("lib", "pi"));
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXConstantExpression>();
            var e = expression.As<LuaXConstantExpression>();
            e.Value.Value.Should().Be(3.14);
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
            e.ArrayExpression.As<LuaXVariableExpression>().VariableName.Should().Be("v2");
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
            arg.VariableName.Should().Be("v1");
        }

        [Fact]
        public void Typename_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[TYPENAME_OP[TYPENAME[typename(typename)]][L_ROUND_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v1)]]]]]]]]]]]]]]][R_ROUND_BRACKET]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXTypeNameOperatorExpression>();
            var cast = expression.As<LuaXTypeNameOperatorExpression>();
            cast.ReturnType.TypeId.Should().Be(LuaXType.String);
            cast.ReturnType.Class.Should().BeNullOrEmpty();
            cast.ReturnType.Array.Should().BeFalse();
            cast.Argument.Should().BeOfType<LuaXVariableExpression>();
            var arg = cast.Argument.As<LuaXVariableExpression>();
            arg.VariableName.Should().Be("v1");
        }

        [Fact]
        public void Cast_ToClass_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[CAST_OP[CAST[cast(cast)]][L_SHARP_BRACKET[<(<)]][TYPE_NAME[IDENTIFIER(lib)]][R_SHARP_BRACKET[>(>)]][L_ROUND_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v1)]]]]]]]]]]]]]]][R_ROUND_BRACKET]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXCastOperatorExpression>();
            var cast = expression.As<LuaXCastOperatorExpression>();
            cast.ReturnType.TypeId.Should().Be(LuaXType.Object);
            cast.ReturnType.Class.Should().Be("lib");
            cast.ReturnType.Array.Should().BeFalse();
            cast.Argument.Should().BeOfType<LuaXVariableExpression>();
            var arg = cast.Argument.As<LuaXVariableExpression>();
            arg.VariableName.Should().Be("v1");
        }

        [Fact]
        public void Cast_ToClass_Failed()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[CAST_OP[CAST[cast(cast)]][L_SHARP_BRACKET[<(<)]][TYPE_NAME[IDENTIFIER(tuple)]][R_SHARP_BRACKET[>(>)]][L_ROUND_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(v1)]]]]]]]]]]]]]]][R_ROUND_BRACKET]]]]]]]]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
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

        [Fact]
        public void NewArray_IntegerArg_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[NEW_ARRAY_EXPR[NEW[new(new)]][TYPE_NAME[TYPE_INT[int(int)]]][L_SQUARE_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT[INTEGER(10)]]]]]]]]]]]]][R_SQUARE_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXNewArrayExpression>();
            var array = expression.As<LuaXNewArrayExpression>();
            array.ElementType.TypeId.Should().Be(LuaXType.Integer);
            array.ElementType.Array.Should().BeFalse();

            array.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            array.ReturnType.Array.Should().BeTrue();

            array.SizeExpression.Should().BeOfType<LuaXConstantExpression>();
            array.SizeExpression.As<LuaXConstantExpression>().Value.Value.Should().Be(10);
        }

        [Fact]
        public void NewArray_RealArg_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[NEW_ARRAY_EXPR[NEW[new(new)]][TYPE_NAME[TYPE_INT[int(int)]]][L_SQUARE_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT[REAL(10.0)]]]]]]]]]]]]][R_SQUARE_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXNewArrayExpression>();
            var array = expression.As<LuaXNewArrayExpression>();
            array.ElementType.TypeId.Should().Be(LuaXType.Integer);
            array.ElementType.Array.Should().BeFalse();

            array.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            array.ReturnType.Array.Should().BeTrue();

            array.SizeExpression.Should().BeOfType<LuaXCastOperatorExpression>();
            var cast = array.SizeExpression.As<LuaXCastOperatorExpression>();
            cast.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            cast.ReturnType.Array.Should().BeFalse();
            cast.Argument.Should().BeOfType<LuaXConstantExpression>();
            cast.Argument.As<LuaXConstantExpression>().Value.Value.Should().Be(10.0);
        }

        [Fact]
        public void NewArray_BooleanArg_Fail()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[NEW_ARRAY_EXPR[NEW[new(new)]][TYPE_NAME[TYPE_INT[int(int)]]][L_SQUARE_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT[BOOLEAN[BOOLEAN_TRUE]]]]]]]]]]]]]][R_SQUARE_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void NewArray_OfObjects_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[NEW_ARRAY_EXPR[NEW[new(new)]][TYPE_NAME[IDENTIFIER(lib)]][L_SQUARE_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT[INTEGER(10)]]]]]]]]]]]]][R_SQUARE_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXNewArrayExpression>();
            var array = expression.As<LuaXNewArrayExpression>();
            array.ElementType.TypeId.Should().Be(LuaXType.Object);
            array.ElementType.Class.Should().Be("lib");
            array.ElementType.Array.Should().BeFalse();

            array.ReturnType.TypeId.Should().Be(LuaXType.Object);
            array.ReturnType.Class.Should().Be("lib");
            array.ReturnType.Array.Should().BeTrue();
        }

        [Fact]
        public void NewArray_OfObjects_UnknownClass_Fail()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[NEW_ARRAY_EXPR[NEW[new(new)]][TYPE_NAME[IDENTIFIER(tuple)]][L_SQUARE_BRACKET][REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT[INTEGER(10)]]]]]]]]]]]]][R_SQUARE_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void NewObject_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[NEW_TABLE_EXPR[NEW[new(new)]][IDENTIFIER(lib)][L_ROUND_BRACKET][R_ROUND_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXNewObjectExpression>();
            var newObject = expression.As<LuaXNewObjectExpression>();
            newObject.ClassName.Should().Be("lib");
            newObject.ReturnType.TypeId.Should().Be(LuaXType.Object);
            newObject.ReturnType.Class.Should().Be("lib");
            newObject.ReturnType.Array.Should().BeFalse();
        }

        [Fact]
        public void NewObject_Failure()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[REXPR[NEW_TABLE_EXPR[NEW[new(new)]][IDENTIFIER(tuple)][L_ROUND_BRACKET][R_ROUND_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void LocalCall_Static_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[CALL[LOCAL_CALL[IDENTIFIER(staticMethod)][CALL_BRACKET[L_ROUND_BRACKET][R_ROUND_BRACKET]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXStaticCallExpression>();
            var call = expression.As<LuaXStaticCallExpression>();
            call.ClassName.Should().Be("class1");
            call.MethodName.Should().Be("staticMethod");
        }

        [Fact]
        public void LocalCall_Instance_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[CALL[LOCAL_CALL[IDENTIFIER(instanceMethod)][CALL_BRACKET[L_ROUND_BRACKET][R_ROUND_BRACKET]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXInstanceCallExpression>();
            var call = expression.As<LuaXInstanceCallExpression>();
            call.Object.Should().BeOfType<LuaXVariableExpression>();
            call.Object.ReturnType.TypeId.Should().Be(LuaXType.Object);
            call.Object.ReturnType.Class.Should().Be("class1");
            call.MethodName.Should().Be("instanceMethod");
        }

        [Fact]
        public void LocalCall_UnknownMethod_Fail()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[CALL[LOCAL_CALL[IDENTIFIER(unknownMethod)][CALL_BRACKET[L_ROUND_BRACKET][R_ROUND_BRACKET]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void LocalCall_WrongArgCount_Fail()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[CALL[LOCAL_CALL[IDENTIFIER(test)][CALL_BRACKET[L_ROUND_BRACKET][R_ROUND_BRACKET]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void MethodCall_Static_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[CALL[METHOD_CALL[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(class1)]]]]]]]]]]]]]]][PROPERTY_ACCESS[.(.)]][IDENTIFIER(staticMethod)][CALL_BRACKET[L_ROUND_BRACKET][R_ROUND_BRACKET]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXStaticCallExpression>();
            var call = expression.As<LuaXStaticCallExpression>();
            call.ClassName.Should().Be("class1");
            call.MethodName.Should().Be("staticMethod");
        }

        [Fact]
        public void MethodCall_Static_Fail()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[CALL[METHOD_CALL[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(class1)]]]]]]]]]]]]]]][PROPERTY_ACCESS[.(.)]][IDENTIFIER(instanceMethod)][CALL_BRACKET[L_ROUND_BRACKET][R_ROUND_BRACKET]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void MethodCall_Instance_Success()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[CALL[METHOD_CALL[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(this)]]]]]]]]]]]]]]][PROPERTY_ACCESS[.(.)]][IDENTIFIER(instanceMethod)][CALL_BRACKET[L_ROUND_BRACKET][R_ROUND_BRACKET]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXInstanceCallExpression>();
            var call = expression.As<LuaXInstanceCallExpression>();
            call.Object.Should().BeOfType<LuaXVariableExpression>();
            call.Object.ReturnType.TypeId.Should().Be(LuaXType.Object);
            call.Object.ReturnType.Class.Should().Be("class1");
            call.MethodName.Should().Be("instanceMethod");
        }

        [Fact]
        public void MethodCall_Instance_Fail()
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method);
            var node = AstNodeExtensions.Parse("[CALL[METHOD_CALL[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CALLABLE_EXPR[ASSIGN_TARGET[VARIABLE[IDENTIFIER(this)]]]]]]]]]]]]]]][PROPERTY_ACCESS[.(.)]][IDENTIFIER(staticMethod)][CALL_BRACKET[L_ROUND_BRACKET][R_ROUND_BRACKET]]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Theory]
        [InlineData(LuaXType.Integer, "[INTEGER(10)]", 10)]
        [InlineData(LuaXType.Real, "[REAL(10.0)]", 10.0)]
        [InlineData(LuaXType.Boolean, "[BOOLEAN[BOOLEAN_TRUE]]", true)]
        [InlineData(LuaXType.String, "[STRING[STRINGDQ(\"a\")]]", "a")]
        public void MethodCall_Arguments_Match_Success(LuaXType argType, string constantText, object constantValue)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method, new LuaXTypeDefinition() { TypeId = argType });
            var node = AstNodeExtensions.Parse($"[LOCAL_CALL[IDENTIFIER(test)][CALL_BRACKET[L_ROUND_BRACKET][CALL_ARGS[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT{constantText}]]]]]]]]]]]]][R_ROUND_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXInstanceCallExpression>();
            var call = expression.As<LuaXInstanceCallExpression>();
            call.Arguments.Should().HaveCount(1);
            call.Arguments[0].Should().BeOfType<LuaXConstantExpression>();
            call.Arguments[0].As<LuaXConstantExpression>().Value.Value.Should().Be(constantValue);
        }

        [Theory]
        [InlineData(LuaXType.Real, "[INTEGER(10)]")]
        [InlineData(LuaXType.Integer, "[REAL(10.0)]")]
        [InlineData(LuaXType.String, "[REAL(10.0)]")]
        [InlineData(LuaXType.String, "[INTEGER(10)]")]
        [InlineData(LuaXType.String, "[BOOLEAN[BOOLEAN_TRUE]]")]
        public void MethodCall_Arguments_AutoCast_Success(LuaXType argType, string constantText)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method, new LuaXTypeDefinition() { TypeId = argType });
            var node = AstNodeExtensions.Parse($"[LOCAL_CALL[IDENTIFIER(test)][CALL_BRACKET[L_ROUND_BRACKET][CALL_ARGS[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT{constantText}]]]]]]]]]]]]][R_ROUND_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            var expression = processor.ProcessExpression(node, @class, method);
            expression.Should().BeOfType<LuaXInstanceCallExpression>();
            var call = expression.As<LuaXInstanceCallExpression>();
            call.Arguments.Should().HaveCount(1);
            call.Arguments[0].Should().BeOfType<LuaXCastOperatorExpression>();
            call.Arguments[0].As<LuaXCastOperatorExpression>().ReturnType.TypeId.Should().Be(argType);
        }

        [Theory]
        [InlineData(LuaXType.Boolean, "[INTEGER(10)]")]
        [InlineData(LuaXType.Datetime, "[REAL(10.0)]")]
        [InlineData(LuaXType.ClassName, "[REAL(10.0)]")]
        [InlineData(LuaXType.Integer, "[BOOLEAN[BOOLEAN_TRUE]]")]
        [InlineData(LuaXType.Real, "[STRING[STRINGDQ(\"a\")]]")]
        public void MethodCall_Arguments_Incompatible_Failed(LuaXType argType, string constantText)
        {
            StageVariableAndProperty(out var metadata, out var @class, out var method, new LuaXTypeDefinition() { TypeId = argType });
            var node = AstNodeExtensions.Parse($"[LOCAL_CALL[IDENTIFIER(test)][CALL_BRACKET[L_ROUND_BRACKET][CALL_ARGS[REXPR[EXPR[OR_BOOL_EXPR[AND_BOOL_EXPR[UX_BOOL_EXPR[REL_EXPR[ADD_EXPR[MUL_EXPR[POWER_EXPR[UNARY_EXPR[SIMPLE_EXPR[CONSTANT{constantText}]]]]]]]]]]]]][R_ROUND_BRACKET]]]");
            var processor = new LuaXAstTreeCreator("", metadata);
            ((Action)(() => processor.ProcessExpression(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }
    }
}
