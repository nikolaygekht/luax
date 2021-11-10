﻿using System;
using FluentAssertions;
using Luax.Interpreter.Expression;
using Luax.Interpreter.Infrastructure;
using Luax.Interpreter.Infrastructure.Stdlib;
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

        [Fact]
        public void StackTrace()
        {
            var app = new LuaXApplication();
            app.CompileResource("ExceptionStackTraceTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("program", out var program).Should().BeTrue();
            program.SearchMethod("main", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().BeEmpty();

            bool thrown = false;
            try
            {
                LuaXMethodExecutor.Execute(method, typelib, null, Array.Empty<object>(), out var _);
            }
            catch (LuaXExecutionException e)
            {
                e.InnerException.Should().NotBeNull();
                e.InnerException.Should().BeOfType<LuaXAssertionException>();
                e.Locations.Should().HaveCount(5);
                e.Locations[0].Source.Should().Be("stdlib.luax");
                e.Locations[1].Source.Should().Be("ExceptionStackTraceTest");
                e.Locations[1].Line.Should().Be(15);
                e.Locations[2].Source.Should().Be("ExceptionStackTraceTest");
                e.Locations[2].Line.Should().Be(11);
                e.Locations[3].Source.Should().Be("ExceptionStackTraceTest");
                e.Locations[3].Line.Should().Be(7);
                e.Locations[4].Source.Should().Be("ExceptionStackTraceTest");
                e.Locations[4].Line.Should().Be(3);
                thrown = true;
            }
            thrown.Should().BeTrue();
        }

        [Theory]
        [InlineData("f", 1, 1.0)]
        [InlineData("f", 2, 2.0)]
        [InlineData("f", 10, 3628800)]
        [InlineData("t", 0, 0.0)]
        [InlineData("t", 1, 0.1)]
        [InlineData("t", 2, 0.2)]
        [InlineData("t", 3, 0.3)]
        [InlineData("t", 4, -1)]
        public void TestIf(string methodName, int argument, double expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("IfTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod(methodName, null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsReal().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { argument }, out var r);
            r.Should().BeOfType<double>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("f", 6, 15)]
        [InlineData("f", 5, 0)]
        [InlineData("f", 4, 5)]
        [InlineData("f", 3, 9)]
        [InlineData("f", 2, 12)]
        [InlineData("f", 1, 14)]
        [InlineData("t", 6, 15)]
        [InlineData("t", 5, 0)]
        [InlineData("t", 4, 5)]
        [InlineData("t", 3, 9)]
        [InlineData("t", 2, 12)]
        [InlineData("t", 1, 14)]
        public void TestWhile(string methodName, int argument, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("WhileTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod(methodName, null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsInteger().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { argument }, out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("a", 3, 1, "aaa")]
        [InlineData("b", 4, 2, "bbbb_bbbb")]
        [InlineData("b", 4, 3, "bbbb_bbbb_bbbb")]
        public void NestedTestWhile(string str, int strRep, int rep, string expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("WhileNestedTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod("stringMaker", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(3);
            method.Arguments[0].LuaType.IsString().Should().BeTrue();
            method.Arguments[1].LuaType.IsInteger().Should().BeTrue();
            method.Arguments[2].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsString().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { str, strRep, rep }, out var r);
            r.Should().BeOfType<string>();
            r.Should().Be(expectedValue);
        }
    }
}



