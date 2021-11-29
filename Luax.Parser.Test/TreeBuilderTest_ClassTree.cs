using System;
using System.Reflection.Metadata;
using FluentAssertions;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Builder;
using Luax.Parser.Test.Tools;
using Xunit;

namespace Luax.Parser.Test
{
    public class TreeBuilderTest_ClassTree
    {
        [Theory]
        [InlineData("[ATTRIBUTE[AT[@(@)]][IDENTIFIER(abcd)][L_ROUND_BRACKET][R_ROUND_BRACKET]]", "abcd")]
        [InlineData("[ATTRIBUTE[AT[@(@)]][IDENTIFIER(abcd)][L_ROUND_BRACKET][CONSTANTS[CONSTANT[INTEGER(5)]]][R_ROUND_BRACKET]]", "abcd", 5)]
        [InlineData("[ATTRIBUTE[AT[@(@)]][IDENTIFIER(abcd)][L_ROUND_BRACKET][CONSTANTS[CONSTANT[INTEGER(5)]][COMMA[,(,)]][CONSTANT[BOOLEAN[BOOLEAN_TRUE[true(true)]]]]][R_ROUND_BRACKET]]", "abcd", 5, true)]
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
            const string tree = "[ATTRIBUTES[ATTRIBUTE[AT[@(@)]][IDENTIFIER(a)][L_ROUND_BRACKET][R_ROUND_BRACKET]][ATTRIBUTE[AT[@(@)]][IDENTIFIER(b)][L_ROUND_BRACKET][R_ROUND_BRACKET]]]";
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
            const string tree = "[CLASS_DECLARATION[CLASS[class(class)]][IDENTIFIER(a)][END[end(end)]]]";
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var c = processor.ProcessClass(node);
            c.Name.Should().Be("a");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("object");
            c.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void ParseClass2_Tree()
        {
            const string tree = "[CLASS_DECLARATION[CLASS[class(class)]][IDENTIFIER(a)][PARENT_CLASS[COLON[:(:)]][IDENTIFIER(b)]][END[end(end)]]]";
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
            const string tree = "[CLASS_DECLARATION[ATTRIBUTES[ATTRIBUTE[AT[@(@)]][IDENTIFIER(abc)][L_ROUND_BRACKET][R_ROUND_BRACKET]]][CLASS[class(class)]][IDENTIFIER(a)][PARENT_CLASS[COLON[:(:)]][IDENTIFIER(b)]][END[end(end)]]]";
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
        [InlineData("[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]", "i", LuaXType.Integer, false, null)]
        [InlineData("[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]][ARRAY_DECL[L_SQUARE_BRACKET][R_SQUARE_BRACKET]]]]", "i", LuaXType.Integer, true, null)]
        [InlineData("[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_REAL[real(real)]]]]]", "i", LuaXType.Real, false, null)]
        [InlineData("[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_DATETIME[datetime(datetime)]]]]]", "i", LuaXType.Datetime, false, null)]
        [InlineData("[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_BOOLEAN[boolean(boolean)]]]]]", "i", LuaXType.Boolean, false, null)]
        [InlineData("[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]", "i", LuaXType.String, false, null)]
        [InlineData("[DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[IDENTIFIER(a)]][ARRAY_DECL[L_SQUARE_BRACKET][R_SQUARE_BRACKET]]]]", "x", LuaXType.Object, true, "a")]
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
            var node = AstNodeExtensions.Parse("[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_VOID[void(void)]]]]]");
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.ProcessDeclaration<LuaXVariable>(node, new LuaXVariableFactory<LuaXVariable>()))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Theory]
        [InlineData("[PROPERTY[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]", "i", LuaXType.Integer, null, false, LuaXVisibility.Private, false)]
        [InlineData("[PROPERTY[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]][ARRAY_DECL[L_SQUARE_BRACKET][R_SQUARE_BRACKET]]]]][EOS[;(;)]]]]", "i", LuaXType.Integer, null, true, LuaXVisibility.Private, false)]
        [InlineData("[PROPERTY[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[IDENTIFIER(object)]]]]][EOS[;(;)]]]]", "i", LuaXType.Object, "object", false, LuaXVisibility.Private, false)]
        [InlineData("[PROPERTY[STATIC[static(static)]][DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]", "i", LuaXType.Integer, null, false, LuaXVisibility.Private, true)]
        [InlineData("[PROPERTY[VISIBILITY[VISIBILITY_PRIVATE[private(private)]]][STATIC[static(static)]][DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]", "i", LuaXType.Integer, null, false, LuaXVisibility.Private, true)]
        [InlineData("[PROPERTY[VISIBILITY[VISIBILITY_INTERNAL[internal(internal)]]][DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]", "i", LuaXType.Integer, null, false, LuaXVisibility.Internal, false)]
        [InlineData("[PROPERTY[VISIBILITY[VISIBILITY_PUBLIC]][STATIC][DECLARATION[DECL_LIST[DECL[IDENTIFIER(i)][TYPE_DECL[TYPE_NAME[TYPE_INT]]]]]]]", "i", LuaXType.Integer, null, false, LuaXVisibility.Public, true)]

        public void ParseProperty(string tree, string propertyName, LuaXType type, string className, bool isArray, LuaXVisibility visibility, bool isStatic)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a");
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
            property.Visibility.Should().Be(visibility);
            property.Static.Should().Be(isStatic);
        }

        [Fact]
        public void ParseConstDeclaration_Success()
        {
            var node = AstNodeExtensions.Parse("[CLASS_CONST_DECLARATION[CONST_DECLARATION[CONST_KW[const(const)]][IDENTIFIER(y)][ASSIGN[=(=)]][CONSTANT[INTEGER(10)]][EOS[;(;)]]]]");
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a");
            processor.ProcessConstantDeclarationInClass(node, @class);

            @class.Constants.Should().HaveCount(1);
            var c = @class.Constants[0];
            c.Name.Should().Be("y");
            c.Value.Value.Should().Be(10);
        }

        [Fact]
        public void ParseConstDeclaration_Fail_AlreadyExists()
        {
            var node = AstNodeExtensions.Parse("[CLASS_CONST_DECLARATION[CONST_DECLARATION[CONST_KW[const(const)]][IDENTIFIER(y)][ASSIGN[=(=)]][CONSTANT[INTEGER(10)]][EOS[;(;)]]]]");
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a");
            @class.Constants.Add(new LuaXConstantVariable() { Name = "y" });
            ((Action)(() => processor.ProcessConstantDeclarationInClass(node, @class))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ParseConstDeclaration_Fail_PropertyExists()
        {
            var node = AstNodeExtensions.Parse("[CLASS_CONST_DECLARATION[CONST_DECLARATION[CONST_KW[const(const)]][IDENTIFIER(y)][ASSIGN[=(=)]][CONSTANT[INTEGER(10)]][EOS[;(;)]]]]");
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a");
            @class.Properties.Add(new LuaXProperty() { Name = "y" });
            ((Action)(() => processor.ProcessConstantDeclarationInClass(node, @class))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Theory]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]", "a", LuaXVisibility.Private, false, null)]
        [InlineData("[FUNCTION_DECLARATION[VISIBILITY[VISIBILITY_PRIVATE[private(private)]]][FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]", "a", LuaXVisibility.Private, false, null)]
        [InlineData("[FUNCTION_DECLARATION[VISIBILITY[VISIBILITY_PUBLIC[public(public)]]][FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]", "a", LuaXVisibility.Public, false, null)]
        [InlineData("[FUNCTION_DECLARATION[VISIBILITY[VISIBILITY_INTERNAL[internal(internal)]]][FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]", "a", LuaXVisibility.Internal, false, null)]
        [InlineData("[FUNCTION_DECLARATION[STATIC[static(static)]][FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]", "a", LuaXVisibility.Private, true, null)]
        [InlineData("[FUNCTION_DECLARATION[VISIBILITY[VISIBILITY_PUBLIC[public(public)]]][STATIC[static(static)]][FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]", "a", LuaXVisibility.Public, true, null)]
        [InlineData("[FUNCTION_DECLARATION[ATTRIBUTES[ATTRIBUTE[AT[@(@)]][IDENTIFIER(attr)][L_ROUND_BRACKET][R_ROUND_BRACKET]]][VISIBILITY[VISIBILITY_PUBLIC[public(public)]]][STATIC[static(static)]][FUNCTION[function(function)]][IDENTIFIER(fn)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]", "fn", LuaXVisibility.Public, true, "attr")]
        public void ParseFunction_Name_And_Modifiers(string tree, string name, LuaXVisibility visibility, bool @static, string attributeName)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a");
            processor.ProcessFunction(node, @class);

            @class.Methods.Should().HaveCount(1);
            var method = @class.Methods[0];

            method.Name.Should().Be(name);
            method.Visibility.Should().Be(visibility);
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
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_VOID[void(void)]]]]]", LuaXType.Void, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]", LuaXType.Integer, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_DATETIME[datetime(datetime)]]]]]", LuaXType.Datetime, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_REAL[real(real)]]]]]", LuaXType.Real, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_BOOLEAN[boolean(boolean)]]]]]", LuaXType.Boolean, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]", LuaXType.String, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]][ARRAY_DECL[L_SQUARE_BRACKET][R_SQUARE_BRACKET]]]]", LuaXType.Integer, null, true)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[IDENTIFIER(object)]]]]", LuaXType.Object, "object", false)]
        public void ParseFunction_ReturnValue(string tree, LuaXType type, string className, bool array)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a");
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
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][DECL_LIST[DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_VOID[void(void)]]]]]", "x", LuaXType.Integer, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][DECL_LIST[DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_REAL[real(real)]]]]]][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_VOID[void(void)]]]]]", "x", LuaXType.Real, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][DECL_LIST[DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_DATETIME[datetime(datetime)]]]]]][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_VOID[void(void)]]]]]", "x", LuaXType.Datetime, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][DECL_LIST[DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_STRING[string(string)]]]]]][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_VOID[void(void)]]]]]", "x", LuaXType.String, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][DECL_LIST[DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_BOOLEAN[boolean(boolean)]]]]]][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_VOID[void(void)]]]]]", "x", LuaXType.Boolean, null, false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][DECL_LIST[DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[IDENTIFIER(tuple)]]]]][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_VOID[void(void)]]]]]", "x", LuaXType.Object, "tuple", false)]
        [InlineData("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][DECL_LIST[DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]][ARRAY_DECL[L_SQUARE_BRACKET][R_SQUARE_BRACKET]]]]][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_VOID[void(void)]]]]]", "x", LuaXType.Integer, null, true)]
        public void ParseFunction_ArgumentType(string tree, string name, LuaXType type, string className, bool array)
        {
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a");
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
            var node = AstNodeExtensions.Parse("[ROOT[CLASS_DECLARATION[CLASS[class(class)]][IDENTIFIER(a)][END[end(end)]]][CLASS_DECLARATION[CLASS[class(class)]][IDENTIFIER(a)][END[end(end)]]]]");
            var processor = new LuaXAstTreeCreator("");
            ((Action)(() => processor.Create(node))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]

        public void Error_MethodOrPropertyWithSameName()
        {
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a");
            @class.Properties.Add(new LuaXProperty() { Name = "a" });
            @class.Methods.Add(new LuaXMethod(@class) { Name = "a" });

            var node = AstNodeExtensions.Parse("[PROPERTY[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(a)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]");
            ((Action)(() => processor.ProcessProperty(node, @class))).Should().Throw<LuaXAstGeneratorException>();

            node = AstNodeExtensions.Parse("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]");
            ((Action)(() => processor.ProcessFunction(node, @class))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]

        public void Error_TwoArgsWithSameName()
        {
            var processor = new LuaXAstTreeCreator("");
            var @class = new LuaXClass("a");

            var node = AstNodeExtensions.Parse("[FUNCTION_DECLARATION[FUNCTION[function(function)]][IDENTIFIER(a)][FUNCTION_DECLARATION_ARGS[L_ROUND_BRACKET][DECL_LIST[DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]][COMMA[,(,)]][DECL[IDENTIFIER(x)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_REAL[real(real)]]]]]][R_ROUND_BRACKET]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]");
            ((Action)(() => processor.ProcessFunction(node, @class))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void VariableDeclaration_Success()
        {
            var @class = new LuaXClass("");
            var method = new LuaXMethod(@class);
            var processor = new LuaXAstTreeCreator("");
            var node = AstNodeExtensions.Parse("[STATEMENTS[STATEMENT[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]]");

            processor.ProcessBody(node, @class, method);
            method.Variables.Should().HaveCount(1);
            var v = method.Variables[0];
            v.Name.Should().Be("i");
            v.LuaType.TypeId.Should().Be(LuaXType.Integer);

            node = AstNodeExtensions.Parse("[STATEMENTS[STATEMENT[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(j)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]]");
            processor.ProcessBody(node, @class, method);
            method.Variables.Should().HaveCount(2);
            v = method.Variables[1];
            v.Name.Should().Be("j");
            v.LuaType.TypeId.Should().Be(LuaXType.Integer);
        }

        [Fact]
        public void VariableDeclaration_Fail_SameNameAsVar()
        {
            var processor = new LuaXAstTreeCreator("");
            var node = AstNodeExtensions.Parse("[STATEMENTS[STATEMENT[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]]");
            var @class = new LuaXClass("");
            var method = new LuaXMethod(@class);
            processor.ProcessBody(node, @class, method);
            method.Variables.Add(new LuaXVariable() { Name = "i" });
            ((Action)(() => processor.ProcessBody(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void VariableDeclaration_Fail_SameNameAsArg()
        {
            var processor = new LuaXAstTreeCreator("");
            var node = AstNodeExtensions.Parse("[STATEMENTS[STATEMENT[DECLARATION[VAR[var(var)]][DECL_LIST[DECL[IDENTIFIER(i)][COLON[:(:)]][TYPE_DECL[TYPE_NAME[TYPE_INT[int(int)]]]]]][EOS[;(;)]]]]]");
            var @class = new LuaXClass("");
            var method = new LuaXMethod(@class);
            processor.ProcessBody(node, @class, method);
            method.Arguments.Add(new LuaXVariable() { Name = "i" });
            ((Action)(() => processor.ProcessBody(node, @class, method))).Should().Throw<LuaXAstGeneratorException>();
        }

        // a -> c
        // b -> d -> e -> f
        //           | -> i
        [Theory]
        [InlineData("a", "a", true)]
        [InlineData("c", "a", true)]
        [InlineData("b", "b", true)]
        [InlineData("d", "b", true)]
        [InlineData("e", "d", true)]
        [InlineData("e", "b", true)]
        [InlineData("i", "b", true)]
        [InlineData("f", "d", true)]

        [InlineData("a", "b", false)]
        [InlineData("a", "c", false)]
        [InlineData("c", "d", false)]
        [InlineData("b", "d", false)]
        [InlineData("b", "f", false)]
        [InlineData("i", "f", false)]
        public void IsKindOf(string source, string target, bool compatible)
        {
            var metadata = new LuaXClassCollection
            {
                new LuaXClass("a"),
                new LuaXClass("b"),
                new LuaXClass("c", "a", null),
                new LuaXClass("d", "b", null),
                new LuaXClass("e", "d", null),
                new LuaXClass("f", "e", null),
                new LuaXClass("i", "e", null)
            };

            metadata.IsKindOf(source, target).Should().Be(compatible);
        }

        [Fact]
        public void SearchProperty()
        {
            var metadata = new LuaXClassCollection();
            var a = new LuaXClass("a");
            a.Properties.Add(new LuaXProperty() { Name = "pa" });
            metadata.Add(a);
            var b = new LuaXClass("b", "a", null);
            metadata.Add(b);
            b.Properties.Add(new LuaXProperty() { Name = "pb" });

            metadata.Search("a", out _);    //force index to be build

            b.SearchProperty("pb", out LuaXProperty p, out string _).Should().Be(true);
            p.Name.Should().Be("pb");
            b.SearchProperty("pa", out p, out string _).Should().Be(true);
            p.Name.Should().Be("pa");

            a.SearchProperty("pb", out _, out string _).Should().Be(false);
            a.SearchProperty("pa", out p, out string _).Should().Be(true);
            p.Name.Should().Be("pa");
        }

        [Fact]
        public void SearchConstant()
        {
            var metadata = new LuaXClassCollection();
            var a = new LuaXClass("a");
            a.Constants.Add(new LuaXConstantVariable() { Name = "pa" });
            metadata.Add(a);
            var b = new LuaXClass("b", "a", null);
            metadata.Add(b);
            b.Constants.Add(new LuaXConstantVariable() { Name = "pb" });

            metadata.Search("a", out _);    //force index to be build

            b.SearchConstant("pb", out var p).Should().Be(true);
            p.Name.Should().Be("pb");
            b.SearchConstant("pa", out p).Should().Be(true);
            p.Name.Should().Be("pa");

            a.SearchConstant("pb", out _).Should().Be(false);
            a.SearchConstant("pa", out p).Should().Be(true);
            p.Name.Should().Be("pa");
        }

        [Fact]
        public void SearchMethod()
        {
            var metadata = new LuaXClassCollection();
            var a = new LuaXClass("a");
            a.Methods.Add(new LuaXMethod(a) { Name = "pa" });
            metadata.Add(a);
            var b = new LuaXClass("b", "a", null);
            metadata.Add(b);
            b.Methods.Add(new LuaXMethod(b) { Name = "pb" });

            metadata.Search("a", out _);    //force index to be build

            b.SearchMethod("pb", out LuaXMethod p).Should().Be(true);
            p.Name.Should().Be("pb");
            b.SearchMethod("pa", out p).Should().Be(true);
            p.Name.Should().Be("pa");

            a.SearchMethod("pb", out _).Should().Be(false);
            a.SearchMethod("pa", out p).Should().Be(true);
            p.Name.Should().Be("pa");
        }

        [Fact]
        public void ParseConstantDeclaration()
        {
            const string tree = "[CONST_DECLARATION[CONST_KW[const(const)]][IDENTIFIER(y)][ASSIGN[=(=)]][CONSTANT[INTEGER(10)]][EOS[;(;)]]]";
            var node = AstNodeExtensions.Parse(tree);
            var processor = new LuaXAstTreeCreator("");
            var decl = processor.ProcessConstantDeclaration(node);
            decl.Name.Should().Be("y");
            decl.Value.ConstantType.Should().Be(LuaXType.Integer);
            decl.Value.Value.Should().Be(10);
        }

        [Fact]
        public void CyclicReference_Self_OK()
        {
            var app = new LuaXApplication();
            app.Classes.Add(new LuaXClass("a", "object", new LuaXElementLocation("", 0, 0)));
            ((Action)(() => app.Pass2())).Should().NotThrow<LuaXAstGeneratorException>();
        }

        [Fact]
        public void CyclicReference_Self_Fail()
        {
            var app = new LuaXApplication();
            app.Classes.Add(new LuaXClass("a", "a", new LuaXElementLocation("", 0, 0)));
            ((Action)(() => app.Pass2())).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void CyclicReference_InChain_Self_Fail()
        {
            var app = new LuaXApplication();
            app.Classes.Add(new LuaXClass("a", "d", new LuaXElementLocation("", 0, 0)));
            app.Classes.Add(new LuaXClass("b", "a", new LuaXElementLocation("", 0, 0)));
            app.Classes.Add(new LuaXClass("c", "b", new LuaXElementLocation("", 0, 0)));
            app.Classes.Add(new LuaXClass("d", "a", new LuaXElementLocation("", 0, 0)));
            ((Action)(() => app.Pass2())).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ParentClass_Fail()
        {
            var app = new LuaXApplication();
            app.Classes.Add(new LuaXClass("a", "d", new LuaXElementLocation("", 0, 0)));
            ((Action)(() => app.Pass2())).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void ParentClass_Success()
        {
            var app = new LuaXApplication();
            app.Classes.Add(new LuaXClass("a", "d", new LuaXElementLocation("", 0, 0)));
            app.Classes.Add(new LuaXClass("d"));
            ((Action)(() => app.Pass2())).Should().NotThrow<LuaXAstGeneratorException>();
        }
    }
}
