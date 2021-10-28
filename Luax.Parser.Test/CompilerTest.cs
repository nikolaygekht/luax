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
        [InlineData("nil", LuaXType.Object, null)]
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
            c.Location.Source.Should().Be("ParseClass");
            c.Location.Line.Should().Be(1);
            c.Location.Column.Should().Be(1);

            c = root.Classes[1];
            c.Name.Should().Be("b");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("a");
            c.Attributes.Should().BeEmpty();
            c.Location.Source.Should().Be("ParseClass");
            c.Location.Line.Should().Be(4);
            c.Location.Column.Should().Be(2);

            c = root.Classes[2];
            c.Name.Should().Be("c");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("b");
            c.Location.Source.Should().Be("ParseClass");
            c.Location.Line.Should().Be(8);
            c.Location.Column.Should().Be(1);

            c.Attributes.Should().HaveCount(1);
            var a = c.Attributes[0];
            a.Name.Should().Be("x");
            a.Parameters.Should().BeEmpty();
            a.Location.Source.Should().Be("ParseClass");
            a.Location.Line.Should().Be(7);
            a.Location.Column.Should().Be(1);

            c = root.Classes[3];
            c.Name.Should().Be("d");
            c.HasParent.Should().BeFalse();
            c.Attributes.Should().HaveCount(2);

            a = c.Attributes[0];
            a.Name.Should().Be("y");
            a.Parameters.Should().HaveCount(3);
            a.Parameters[0].Value.Should().Be(1);
            a.Parameters[1].Value.Should().Be(-5);
            a.Parameters[2].IsNil.Should().BeTrue();

            a = c.Attributes[1];
            a.Name.Should().Be("z");
            a.Parameters.Should().HaveCount(2);
            a.Parameters[0].Value.Should().Be(true);
            a.Parameters[1].Value.Should().Be("hello");
        }

        [Fact]
        public void ParseClassProperties()
        {
            var root = Compiler.CompileResource("ParseClassProperties");

            root.Classes.Should().HaveCount(1);
            var @class = root.Classes[0];

            @class.Properties.Should().HaveCount(6);

            var property = @class.Properties[0];
            property.Name.Should().Be("a");
            property.LuaType.TypeId.Should().Be(LuaXType.Integer);
            property.LuaType.Array.Should().BeFalse();
            property.Static.Should().BeFalse();
            property.Visibility.Should().Be(LuaXVisibility.Private);
            property.Location.Line.Should().Be(2);
            property.Location.Column.Should().Be(9);

            property = @class.Properties[1];
            property.Name.Should().Be("b");
            property.LuaType.TypeId.Should().Be(LuaXType.Real);
            property.LuaType.Array.Should().BeFalse();
            property.Static.Should().BeTrue();
            property.Visibility.Should().Be(LuaXVisibility.Private);
            property.Location.Line.Should().Be(3);
            property.Location.Column.Should().Be(16);

            property = @class.Properties[2];
            property.Name.Should().Be("c");
            property.LuaType.TypeId.Should().Be(LuaXType.String);
            property.LuaType.Array.Should().BeTrue();
            property.Static.Should().BeTrue();
            property.Visibility.Should().Be(LuaXVisibility.Private);
            property.Location.Line.Should().Be(3);
            property.Location.Column.Should().Be(26);

            property = @class.Properties[3];
            property.Name.Should().Be("d");
            property.LuaType.TypeId.Should().Be(LuaXType.Boolean);
            property.LuaType.Array.Should().BeFalse();
            property.Static.Should().BeFalse();
            property.Visibility.Should().Be(LuaXVisibility.Public);
            property.Location.Line.Should().Be(4);

            property = @class.Properties[4];
            property.Name.Should().Be("e");
            property.LuaType.TypeId.Should().Be(LuaXType.Object);
            property.LuaType.Class.Should().Be("object");
            property.LuaType.Array.Should().BeFalse();
            property.Static.Should().BeTrue();
            property.Visibility.Should().Be(LuaXVisibility.Private);
            property.Location.Line.Should().Be(5);

            property = @class.Properties[5];
            property.Name.Should().Be("f");
            property.LuaType.TypeId.Should().Be(LuaXType.Datetime);
            property.LuaType.Array.Should().BeFalse();
            property.Static.Should().BeTrue();
            property.Visibility.Should().Be(LuaXVisibility.Internal);
            property.Location.Line.Should().Be(6);
        }

        [Fact]
        public void ParseClassMethods()
        {
            var root = Compiler.CompileResource("ParseClassMethods");

            root.Classes.Should().HaveCount(1);
            var @class = root.Classes[0];

            @class.Methods.Should().HaveCount(4);

            var method = @class.Methods[0];
            method.Attributes.Should().HaveCount(0);
            method.Arguments.Should().HaveCount(0);
            method.Name.Should().Be("f1");
            method.Visibility.Should().Be(LuaXVisibility.Private);
            method.Static.Should().BeFalse();
            method.ReturnType.TypeId.Should().Be(LuaXType.Void);
            method.Location.Line.Should().Be(2);

            method = @class.Methods[1];
            method.Attributes.Should().HaveCount(0);
            method.Arguments.Should().HaveCount(0);
            method.Name.Should().Be("f2");
            method.Visibility.Should().Be(LuaXVisibility.Private);
            method.Static.Should().BeTrue();
            method.ReturnType.TypeId.Should().Be(LuaXType.Void);
            method.Location.Line.Should().Be(5);

            method = @class.Methods[2];
            method.Attributes.Should().HaveCount(0);
            method.Arguments.Should().HaveCount(0);
            method.Name.Should().Be("f3");
            method.Visibility.Should().Be(LuaXVisibility.Internal);
            method.Static.Should().BeFalse();
            method.ReturnType.TypeId.Should().Be(LuaXType.Void);
            method.Location.Line.Should().Be(8);

            method = @class.Methods[3];
            method.Attributes.Should().HaveCount(2);
            method.Arguments.Should().HaveCount(3);
            method.Name.Should().Be("doit");
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeTrue();
            method.ReturnType.TypeId.Should().Be(LuaXType.Object);
            method.ReturnType.Array.Should().BeFalse();
            method.ReturnType.Class.Should().Be("tuple");

            var arg = method.Arguments[0];
            arg.Name.Should().Be("x");
            arg.LuaType.TypeId.Should().Be(LuaXType.Integer);
            arg.LuaType.Array.Should().BeFalse();
            arg.Location.Line.Should().Be(13);
            arg.Location.Column.Should().Be(32);

            arg = method.Arguments[1];
            arg.Name.Should().Be("y");
            arg.LuaType.TypeId.Should().Be(LuaXType.String);
            arg.LuaType.Array.Should().BeTrue();
            arg.Location.Line.Should().Be(13);
            arg.Location.Column.Should().Be(41);

            arg = method.Arguments[2];
            arg.Name.Should().Be("z");
            arg.LuaType.TypeId.Should().Be(LuaXType.Object);
            arg.LuaType.Class.Should().Be("object");
            arg.LuaType.Array.Should().BeFalse();

            var attr = method.Attributes[0];
            attr.Name.Should().Be("attr1");
            attr.Parameters.Should().BeEmpty();

            attr = method.Attributes[1];
            attr.Name.Should().Be("attr2");
            attr.Parameters.Should().HaveCount(1);
            attr.Parameters[0].ConstantType.Should().Be(LuaXType.Integer);
            attr.Parameters[0].Value.Should().Be(5);
        }

        [Fact]
        public void Stdlib()
        {
            var body = StdlibHeader.ReadStdlib();

            body.Classes.Search("csvParser", out var csvParser).Should().BeTrue();
            csvParser.Should().NotBeNull();

            csvParser.Methods.Search("create", out var m).Should().BeTrue();
            m.Should().NotBeNull();
            m.Extern.Should().BeTrue();
            m.Static.Should().BeTrue();
            m.Visibility.Should().Be(LuaXVisibility.Public);
            m.Arguments.Should().BeEmpty();
            m.ReturnType.TypeId.Should().Be(LuaXType.Object);
            m.ReturnType.Class.Should().Be("csvParser");

            csvParser.Methods.Search("splitLine", out m).Should().BeTrue();
            m.Should().NotBeNull();
            m.Extern.Should().BeTrue();
            m.Static.Should().BeFalse();
            m.Visibility.Should().Be(LuaXVisibility.Public);
            m.Arguments.Should().HaveCount(1);
            m.Arguments[0].LuaType.TypeId.Should().Be(LuaXType.String);
            m.Arguments[0].LuaType.Array.Should().Be(false);
            m.ReturnType.TypeId.Should().Be(LuaXType.String);
            m.ReturnType.Array.Should().Be(true);
        }
    }
}
