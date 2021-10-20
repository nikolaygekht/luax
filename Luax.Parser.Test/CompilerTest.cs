using System;
using FluentAssertions;
using Luax.Parser.Ast;
using Luax.Parser.Test.Tools;
using Luax.Parser.Test.Utils;
using Xunit;

namespace Luax.Parser.Test
{
    public class CompilerTest
    {
        [Theory]
        [InlineData("true", LuaXType.Boolean, true)]
        [InlineData("false", LuaXType.Boolean, false)]
        [InlineData("nil", LuaXType.Class, null)]
        [InlineData("1", LuaXType.Integer, 1)]
        [InlineData("1_234", LuaXType.Integer, 1234)]
        [InlineData("0xa", LuaXType.Integer, 10)]
        [InlineData("0x12_34_56_78", LuaXType.Integer, 0x12345678)]
        [InlineData("-1", LuaXType.Integer, -1)]
        [InlineData("1.1", LuaXType.Real, 1.1)]
        [InlineData("-1.1", LuaXType.Real, -1.1)]
        [InlineData("1.0e5", LuaXType.Real, 1e5)]
        [InlineData("1_235.000_5", LuaXType.Real, 1235.0005)]
        [InlineData("\"\"", LuaXType.String, "")]
        [InlineData("\"a\"", LuaXType.String, "a")]
        [InlineData("\"a\\\"\"", LuaXType.String, "a\"")]
        public void ParseConstant(string constant, LuaXType type, object value)
        {
            var root = Compiler.CompileResource("ParseConstant", new[] { new Tuple<string, string>("$constant", constant) });
            var c = root.Classes[0]
                        .Attributes[0]
                        .Parameters[0];

            c.ConstantType.Should().Be(type);
            c.Value.Should().Be(value);
        }

        [Fact]
        public void ParseClass()
        {
            var root = Compiler.CompileResource("ParseClass");

            root.Classes.Should().HaveCount(4);

            var c = root.Classes[0];
            c.Name.Should().Be("a");
            c.HasParent.Should().BeFalse();
            c.Parent.Should().BeNullOrEmpty();
            c.Attributes.Should().BeEmpty();

            c = root.Classes[1];
            c.Name.Should().Be("a");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("b");
            c.Attributes.Should().BeEmpty();


            c = root.Classes[2];
            c.Name.Should().Be("a");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("b");
            c.Attributes.Should().HaveCount(1);
            var a = c.Attributes[0];
            a.Name.Should().Be("x");
            a.Parameters.Should().BeEmpty();

            c = root.Classes[3];
            c.Name.Should().Be("a");
            c.HasParent.Should().BeFalse();
            c.Attributes.Should().HaveCount(2);
            
            a = c.Attributes[0];
            a.Name.Should().Be("y");
            a.Parameters.Should().HaveCount(3);
            a.Parameters[0].Value.Should().Be(1);
            a.Parameters[1].Value.Should().Be(-5);
            a.Parameters[2].Should().BeSameAs(LuaXConstant.Nil);

            a = c.Attributes[1];
            a.Name.Should().Be("z");
            a.Parameters.Should().HaveCount(2);
            a.Parameters[0].Value.Should().Be(true);
            a.Parameters[1].Value.Should().Be("hello");
        }
    }
}
