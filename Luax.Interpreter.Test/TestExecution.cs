using System;
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
        [InlineData(6, 21)]
        [InlineData(5, 15)]
        [InlineData(4, 10)]
        [InlineData(3, 6)]
        [InlineData(2, 3)]
        [InlineData(1, 1)]
        public void TestWhile(int argument, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("WhileTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod("dummy", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsInteger().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { argument }, out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("test1", -1, "exception from the factory")]
        [InlineData("test2", -2, "exception from the static call")]
        [InlineData("test3", -3, "exception from the variable")]
        public void TestThrow(string methodName, int expectedCode, string expectedMessage)
        {
            var app = new LuaXApplication();
            app.CompileResource("ThrowTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod(methodName, null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(0);
            method.ReturnType.IsVoid().Should().BeTrue();

            try
            {
                LuaXMethodExecutor.Execute(method, typelib, null, Array.Empty<object>(), out var r);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<LuaXExecutionException>();
                ex.As<LuaXExecutionException>().Properties["code"].Value.Should().Be(expectedCode);
                ex.As<LuaXExecutionException>().Message.Should().Be(expectedMessage);
            }
        }

        [Theory]
        [InlineData(6, 15)]
        [InlineData(5, 0)]
        [InlineData(4, 5)]
        [InlineData(3, 9)]
        [InlineData(2, 12)]
        [InlineData(1, 14)]
        public void TestWhileBreak(int argument, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("WhileBreakTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod("dummy", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsInteger().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { argument }, out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(-1, 1, "value is lower, code is")]
        [InlineData(0, 2, "value is equal, code is")]
        [InlineData(1, 0, "value is greater")]
        public void TestTryCatch(int arg, int expectedCode, string expectedMessage)
        {
            var app = new LuaXApplication();
            app.CompileResource("TryCatchTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod("testTry", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsString().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { arg }, out var r);

            r.Should().BeOfType<string>();
            r.Should().Be(expectedCode != 0 ? $"{expectedMessage} {expectedCode}" : expectedMessage);
        }

        [Theory]
        [InlineData(6, 16)]
        [InlineData(5, 10)]
        [InlineData(4, 10)]
        [InlineData(3, 6)]
        [InlineData(2, 3)]
        [InlineData(1, 1)]
        public void TestWhileContinue(int argument, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("WhileContinueTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod("dummy", null, out var method).Should().BeTrue();
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
        public void TestWhileNested(string str, int strRep, int rep, string expectedValue)
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

        [Theory]
        [InlineData(0, 2, 1, 3)]
        [InlineData(1, 2, 1, 2)]
        [InlineData(2, 0, -1, 3)]
        [InlineData(5, 2, -1, 4)]
        public void TestFor(int start, int limit, int iterator, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("ForTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod("basicTest", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(3);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.Arguments[1].LuaType.IsInteger().Should().BeTrue();
            method.Arguments[2].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsInteger().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { start, limit, iterator }, out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(0, 2, 3)]
        [InlineData(1, 2, 2)]
        public void TestForWithoutIteratorExpr(int start, int limit, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("ForTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod("withoutIteratorExprTest", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(2);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.Arguments[1].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsInteger().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { start, limit }, out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(6, 21)]
        [InlineData(5, 15)]
        [InlineData(4, 10)]
        [InlineData(3, 6)]
        [InlineData(2, 3)]
        [InlineData(1, 1)]
        public void TestForChangeValueOfInitExpr(int limit, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("ForTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod("changeValueOfInitExprTest", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsInteger().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { limit }, out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("a", 3, 1, "aaa")]
        [InlineData("b", 4, 2, "bbbb_bbbb")]
        [InlineData("b", 4, 3, "bbbb_bbbb_bbbb")]
        public void TestForNested(string str, int strRep, int rep, string expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("ForTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var program).Should().BeTrue();
            program.SearchMethod("nestedLoopTest", null, out var method).Should().BeTrue();
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



