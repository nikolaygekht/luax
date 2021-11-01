using System;
using FluentAssertions;
using Luax.Interpreter.Expression;
using Luax.Interpreter.Infrastructure;
using Luax.Parser;
using Luax.Parser.Test.Utils;
using Xunit;

namespace Luax.Interpreter.Test
{
    public class TestExecution
    {
        [Fact]
        public void Assign()
        {
            var app = new LuaXApplication();
            app.CompileResource("AssignTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("a", out var a);
            typelib.SearchClass("b", out var b);
            a.Should().NotBeNull();
            b.Should().NotBeNull();
            b.SearchMethod("test1", null, out var method);

            var @this = b.New(typelib);

            LuaXMethodExecutor.Execute(method, typelib, @this, new object[] { 10 }, out var r);
            r.Should().Be(10 + 8 + (10 + 4) * 2);
            a.StaticProperties["s"].Value.Should().Be(10 + 1);
            b.StaticProperties["s"].Value.Should().Be(10 + 2);
            @this.Properties["p"].Value.Should().Be(10 + 3);

            a.StaticProperties["arr"].Value
                .Should().NotBeNull()
                .And.BeOfType<LuaXVariableInstanceArray>();

            var arr = a.StaticProperties["arr"].Value.As<LuaXVariableInstanceArray>();
            arr[0].Value.Should().Be(10 + 5);
            arr[1].Value.Should().Be(10 + 6);
            arr[2].Value.Should().Be(10 + 7);
        }

        [Fact]
        public void Constructor()
        {
            var app = new LuaXApplication();
            app.CompileResource("Constructor");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("a", out var a);
            typelib.SearchClass("b", out var b);
            a.Should().NotBeNull();
            b.Should().NotBeNull();

            var pa = a.New(typelib);
            pa.Properties["a"].Value.Should().Be(0);
            pa.Properties["b"].Value.Should().Be(0.0);
            pa.Properties["c"].Value.Should().Be(null);
            pa.Properties["d"].Value.Should().Be(false);
            pa.Properties["e"].Value.Should().Be(new DateTime(1900, 1, 1));
            pa.Properties["f"].Value.Should().Be(null);

            var pb = b.New(typelib);
            pb.Properties["a"].Value.Should().Be(1);
            pb.Properties["b"].Value.Should().Be(2.0);
            pb.Properties["c"].Value.Should().Be("hello");
            pb.Properties["d"].Value.Should().Be(true);
            pb.Properties["e"].Value.As<DateTime>().Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(0.1));
            pb.Properties["f"].Value.Should().BeOfType<LuaXVariableInstanceArray>();
            var arr = pb.Properties["f"].Value.As<LuaXVariableInstanceArray>();
            arr.Length.Should().Be(2);
            arr[0].Value.Should().Be(10);
            arr[1].Value.Should().Be(20);
        }
    }
}



