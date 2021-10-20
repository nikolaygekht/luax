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
    }  
}
