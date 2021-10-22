using System;
using System.Reflection.Metadata;
using FluentAssertions;
using Luax.Parser.Ast;
using Luax.Parser.Test.Tools;
using Xunit;

namespace Luax.Parser.Test
{
    public class TreeBuilderTest
    {
        [Theory]
        [InlineData("[CONSTANT[NIL]]", LuaXType.Class, null)]
        [InlineData("[CONSTANT[INTEGER(5)]]", LuaXType.Integer, 5)]
        [InlineData("[CONSTANT[INTEGER(1_234)]]", LuaXType.Integer, 1234)]
        [InlineData("[CONSTANT[HEX_INTEGER(0x1bc)]]", LuaXType.Integer, 0x1bc)]
        [InlineData("[CONSTANT[HEX_INTEGER(0x1_bc)]]", LuaXType.Integer, 0x1bc)]
        [InlineData("[CONSTANT[NEGATIVE_INTEGER[INTEGER(5)]]]", LuaXType.Integer, -5)]
        [InlineData("[CONSTANT[REAL(5.1)]]", LuaXType.Real, 5.1)]
        [InlineData("[CONSTANT[REAL(1234_5678.1e22)]]", LuaXType.Real, 12345678.1e22)]
        [InlineData("[CONSTANT[NEGATIVE_REAL[REAL(5.1)]]]", LuaXType.Real, -5.1)]
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

        [Theory]
        [InlineData("[ATTRIBUTE[IDENTIFIER(abcd)]]", "abcd")]
        [InlineData("[ATTRIBUTE[IDENTIFIER(abcd)][CONSTANTS[CONSTANT[INTEGER(5)]]]]]", "abcd", 5)]
        [InlineData("[ATTRIBUTE[IDENTIFIER(abcd)][CONSTANTS[CONSTANT[INTEGER(5)]][CONSTANT[BOOLEAN[BOOLEAN_TRUE]]]]]]", "abcd", 5, true)]
        public void ParseAttribute(string tree, string name, params object[] parameters)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var attribute = processor.ProcessAttribute(node);

            attribute.Name.Should().Be(name);
            attribute.Parameters.Should().HaveCount(parameters.Length);
            for (int i = 0; i < parameters.Length; i++)
                attribute.Parameters[i].Value.Should().Be(parameters[i]);
        }

        [Fact]
        public void ParseAttributes()
        {
            const string tree = "[ATTRIBUTES[ATTRIBUTE[IDENTIFIER(a)]][ATTRIBUTE[IDENTIFIER(b)]]]";
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var target = new LuaXAttributeCollection();
            processor.ProcessAttributes(node.Children, target);
            target.Should().HaveCount(2);
            target[0].Name.Should().Be("a");
            target[1].Name.Should().Be("b");
        }

        [Fact]
        public void ParseClass1_Tree()
        {
            const string tree = "[CLASS[IDENTIFIER(a)]]";
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var c = processor.ProcessClass(node);
            c.Name.Should().Be("a");
            c.HasParent.Should().BeFalse();
            c.Parent.Should().BeNullOrEmpty();
            c.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void ParseClass2_Tree()
        {
            const string tree = "[CLASS[IDENTIFIER(a)][PARENT_CLASS[IDENTIFIER(b)]]]";
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var c = processor.ProcessClass(node);
            c.Name.Should().Be("a");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("b");
            c.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void ParseClass3_Tree()
        {
            const string tree = "[CLASS[ATTRIBUTES[ATTRIBUTE[IDENTIFIER(abc)]]][IDENTIFIER(a)][PARENT_CLASS[IDENTIFIER(b)]]]";
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var c = processor.ProcessClass(node);
            c.Name.Should().Be("a");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("b");
            c.Attributes.Should().HaveCount(1);
            c.Attributes[0].Name.Should().Be("abc");
        }

        [Theory]
        [InlineData("[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]", "i", LuaXType.Integer, false, null)]
        [InlineData("[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_INT]][ARRAY_DECL]]]", "i", LuaXType.Integer, true, null)]
        [InlineData("[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_REAL]]]]", "i", LuaXType.Real, false, null)]
        [InlineData("[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_DATETIME]]]]", "i", LuaXType.Datetime, false, null)]
        [InlineData("[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_BOOLEAN]]]]", "i", LuaXType.Boolean, false, null)]
        [InlineData("[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_STRING]]]]", "i", LuaXType.String, false, null)]
        [InlineData("[DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[IDENTIFIER(a)]][ARRAY_DECL]]]", "x", LuaXType.Class, true, "a")]
        public void ParseDeclaration(string tree, string expectedName, LuaXType expectedType, bool expectedArray, string expectedClassName)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var declaration = processor.ProcessDeclaration<LuaXVariable>(node, new LuaXVariableFactory<LuaXVariable>());

            declaration.Should().NotBeNull();
            declaration.Name.Should().Be(expectedName);
            declaration.LuaType.TypeId.Should().Be(expectedType);
            declaration.LuaType.Array.Should().Be(expectedArray);
            if (string.IsNullOrEmpty(expectedClassName))
                declaration.LuaType.Class.Should().BeNullOrEmpty();
            else
                declaration.LuaType.Class.Should().Be(expectedClassName);
        }

        [Fact]
        public void ParseDeclaration_VoidType()
        {
            var node = AstNodeExtensions.Parse("[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]");
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.ProcessDeclaration<LuaXVariable>(node, new LuaXVariableFactory<LuaXVariable>()))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Theory]
        [InlineData("[PROPERTY[DECLARATION[DECL_LIST[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]]]]", "i", LuaXType.Integer, null, false, true, false)]
        [InlineData("[PROPERTY[DECLARATION[DECL_LIST[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_INT]][ARRAY_DECL]]]]]]", "i", LuaXType.Integer, null, true, true, false)]
        [InlineData("[PROPERTY[DECLARATION[DECL_LIST[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[IDENTIFIER(object)]]]]]]]", "i", LuaXType.Class, "object", false, true, false)]
        [InlineData("[PROPERTY[STATIC][DECLARATION[DECL_LIST[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]]]]", "i", LuaXType.Integer, null, false, true, true)]
        [InlineData("[PROPERTY[STATIC][DECLARATION[DECL_LIST[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_DATETIME]]]]]]]", "i", LuaXType.Datetime, null, false, true, true)]
        [InlineData("[PROPERTY[VISIBILITY[VISIBILITY_PRIVATE]][STATIC][DECLARATION[DECL_LIST[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]]]]", "i", LuaXType.Integer, null, false, false, true)]
        [InlineData("[PROPERTY[VISIBILITY[VISIBILITY_PRIVATE]][DECLARATION[DECL_LIST[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]]]]", "i", LuaXType.Integer, null, false, false, false)]
        [InlineData("[PROPERTY[VISIBILITY[VISIBILITY_PUBLIC]][STATIC][DECLARATION[DECL_LIST[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]]]]", "i", LuaXType.Integer, null, false, true, true)]

        public void ParseProperty(string tree, string propertyName, LuaXType type, string className, bool isArray, bool isPublic, bool isStatic)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            LuaXClass @class = new LuaXClass("a", null);
            processor.ProcessProperty(node, @class);

            @class.Properties.Should().HaveCount(1);
            var property = @class.Properties[0];
            property.Name.Should().Be(propertyName);
            property.LuaType.TypeId.Should().Be(type);
            property.LuaType.Array.Should().Be(isArray);
            if (string.IsNullOrEmpty(className))
                property.LuaType.Class.Should().BeNullOrEmpty();
            else
                property.LuaType.Class.Should().Be(className);
            property.Public.Should().Be(isPublic);
            property.Static.Should().Be(isStatic);
        }

        [Theory]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_STRING]]]]", "a", true, false, null)]
        [InlineData("[FUNCTION_DECLARATION[VISIBILITY[VISIBILITY_PRIVATE]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_STRING]]]]", "a", false, false, null)]
        [InlineData("[FUNCTION_DECLARATION[VISIBILITY[VISIBILITY_PUBLIC]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_STRING]]]]", "a", true, false, null)]
        [InlineData("[FUNCTION_DECLARATION[STATIC][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_STRING]]]]", "a", true, true, null)]
        [InlineData("[FUNCTION_DECLARATION[VISIBILITY[VISIBILITY_PRIVATE]][STATIC][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_STRING]]]]", "a", false, true, null)]
        [InlineData("[FUNCTION_DECLARATION[ATTRIBUTES[ATTRIBUTE[IDENTIFIER(attr)]]][VISIBILITY[VISIBILITY_PRIVATE]][STATIC][IDENTIFIER(fn)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]", "fn", false, true, "attr")]
        public void ParseFunction_Name_And_Modifiers(string tree, string name, bool @public, bool @static, string attributeName)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            LuaXClass @class = new LuaXClass("a", null);
            processor.ProcessFunction(node, @class);

            @class.Methods.Should().HaveCount(1);
            var method = @class.Methods[0];

            method.Name.Should().Be(name);
            method.Public.Should().Be(@public);
            method.Static.Should().Be(@static);

            method.Arguments.Count.Should().Be(0);

            if (string.IsNullOrEmpty(attributeName))
                method.Attributes.Should().BeEmpty();
            else
            {
                method.Attributes.Should().HaveCount(1);
                var attr = method.Attributes[0];
                attr.Name.Should().Be(attributeName);
            }
        }

        [Theory]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]", LuaXType.Void, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]", LuaXType.Integer, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_DATETIME]]]]", LuaXType.Datetime, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_REAL]]]]", LuaXType.Real, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_BOOLEAN]]]]", LuaXType.Boolean, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_STRING]]]]", LuaXType.String, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_INT]][ARRAY_DECL]]]", LuaXType.Integer, null, true)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[IDENTIFIER(object)]]]]", LuaXType.Class, "object", false)]
        public void ParseFunction_ReturnValue(string tree, LuaXType type, string className, bool array)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            LuaXClass @class = new LuaXClass("a", null);
            processor.ProcessFunction(node, @class);

            @class.Methods.Should().HaveCount(1);
            var method = @class.Methods[0];

            method.ReturnType.Should().NotBeNull();
            method.ReturnType.TypeId.Should().Be(type);
            method.ReturnType.Array.Should().Be(array);
            if (string.IsNullOrEmpty(className))
                method.ReturnType.Class.Should().BeNullOrEmpty();
            else
                method.ReturnType.Class.Should().Be(className);
        }

        [Theory]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[DECL_LIST[DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]]][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]", "x", LuaXType.Integer, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[DECL_LIST[DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[TYPE_REAL]]]]]][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]", "x", LuaXType.Real, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[DECL_LIST[DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[TYPE_DATETIME]]]]]][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]", "x", LuaXType.Datetime, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[DECL_LIST[DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[TYPE_STRING]]]]]][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]", "x", LuaXType.String, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[DECL_LIST[DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[TYPE_BOOLEAN]]]]]][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]", "x", LuaXType.Boolean, null, false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[DECL_LIST[DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[IDENTIFIER(tuple)]]]]]][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]", "x", LuaXType.Class, "tuple", false)]
        [InlineData("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[DECL_LIST[DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[TYPE_INT]][ARRAY_DECL]]]]][TYPE_DECL[TYPE_NAME[TYPE_VOID]]]]", "x", LuaXType.Integer, null, true)]
        public void ParseFunction_ArgumentType(string tree, string name, LuaXType type, string className, bool array)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            LuaXClass @class = new LuaXClass("a", null);
            processor.ProcessFunction(node, @class);

            var method = @class.Methods[0];
            method.Arguments.Should().HaveCount(1);
            var arg = method.Arguments[0];

            arg.Name.Should().Be(name);
            arg.LuaType.TypeId.Should().Be(type);
            arg.LuaType.Array.Should().Be(array);
            if (string.IsNullOrEmpty(className))
                arg.LuaType.Class.Should().BeNullOrEmpty();
            else
                arg.LuaType.Class.Should().Be(className);
        }

        [Fact]
        public void Error_ClassWithSameName()
        {
            var node = AstNodeExtensions.Parse("[ROOT[CLASS_DECLARATION[IDENTIFIER(a)]][CLASS_DECLARATION[IDENTIFIER(a)]]]");
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.Create(node))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]

        public void Error_MethodOrPropertyWithSameName()
        {
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a", null);
            @class.Properties.Add(new LuaXProperty() { Name = "a" });
            @class.Methods.Add(new LuaXMethod() { Name = "a" });

            var node = AstNodeExtensions.Parse("[PROPERTY[DECLARATION[DECL_LIST[DECL[IDENTIFIER(a)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]]]]");
            ((Action)(() => processor.ProcessProperty(node, @class))).Should().Throw<LuaXAstGeneratorException>();

            node = AstNodeExtensions.Parse("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]");
            ((Action)(() => processor.ProcessFunction(node, @class))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]

        public void Error_TwoArgsWithSameName()
        {
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a", null);

            var node = AstNodeExtensions.Parse("[FUNCTION_DECLARATION[IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[DECL_LIST[DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]][DECL[IDENTIFIER(x)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]]][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]");
            ((Action)(() => processor.ProcessFunction(node, @class))).Should().Throw<LuaXAstGeneratorException>();
        }
    }
}
