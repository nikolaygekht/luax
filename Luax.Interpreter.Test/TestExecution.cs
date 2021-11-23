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

            typelib.SearchClass("Program", out var program).Should().BeTrue();
            program.SearchMethod("Main", null, out var method).Should().BeTrue();
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
                e.LuaXStackTrace.Should().HaveCount(5);
                e.LuaXStackTrace[0].Location.Source.Should().Be("stdlib.luax");
                e.LuaXStackTrace[0].CallSite.Class.Name.Should().Be("assert");
                e.LuaXStackTrace[0].CallSite.Name.Should().Be("isTrue");
                e.LuaXStackTrace[1].Location.Source.Should().Be("ExceptionStackTraceTest");
                e.LuaXStackTrace[1].Location.Line.Should().Be(15);
                e.LuaXStackTrace[1].CallSite.Class.Name.Should().Be("Program");
                e.LuaXStackTrace[1].CallSite.Name.Should().Be("z");
                e.LuaXStackTrace[2].Location.Source.Should().Be("ExceptionStackTraceTest");
                e.LuaXStackTrace[2].Location.Line.Should().Be(11);
                e.LuaXStackTrace[2].CallSite.Class.Name.Should().Be("Program");
                e.LuaXStackTrace[2].CallSite.Name.Should().Be("y");
                e.LuaXStackTrace[3].Location.Source.Should().Be("ExceptionStackTraceTest");
                e.LuaXStackTrace[3].Location.Line.Should().Be(7);
                e.LuaXStackTrace[3].CallSite.Class.Name.Should().Be("Program");
                e.LuaXStackTrace[3].CallSite.Name.Should().Be("x");
                e.LuaXStackTrace[4].Location.Source.Should().Be("ExceptionStackTraceTest");
                e.LuaXStackTrace[4].Location.Line.Should().Be(3);
                e.LuaXStackTrace[4].CallSite.Class.Name.Should().Be("Program");
                e.LuaXStackTrace[4].CallSite.Name.Should().Be("Main");
                thrown = true;
            }
            thrown.Should().BeTrue();
        }

        [Fact]
        public void LuaXStackTraceFrames()
        {
            var app = new LuaXApplication();
            app.CompileResource("LuaXStackTraceFramesTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("Program", out var program).Should().BeTrue();
            program.SearchMethod("Main", null, out var method).Should().BeTrue();
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
                e.InnerException.Should().BeOfType<DivideByZeroException>();
                e.LuaXStackTrace.Should().HaveCount(4);
                e.LuaXStackTrace[0].Location.Source.Should().Be("LuaXStackTraceFramesTest");
                e.LuaXStackTrace[0].Location.Line.Should().Be(19);
                e.LuaXStackTrace[0].CallSite.Class.Name.Should().Be("Program");
                e.LuaXStackTrace[0].CallSite.Name.Should().Be("f3");
                e.LuaXStackTrace[1].Location.Source.Should().Be("LuaXStackTraceFramesTest");
                e.LuaXStackTrace[1].Location.Line.Should().Be(13);
                e.LuaXStackTrace[1].CallSite.Class.Name.Should().Be("Program");
                e.LuaXStackTrace[1].CallSite.Name.Should().Be("f2");
                e.LuaXStackTrace[2].Location.Source.Should().Be("LuaXStackTraceFramesTest");
                e.LuaXStackTrace[2].Location.Line.Should().Be(7);
                e.LuaXStackTrace[2].CallSite.Class.Name.Should().Be("Program");
                e.LuaXStackTrace[2].CallSite.Name.Should().Be("f1");
                e.LuaXStackTrace[3].Location.Source.Should().Be("LuaXStackTraceFramesTest");
                e.LuaXStackTrace[3].Location.Line.Should().Be(3);
                e.LuaXStackTrace[3].CallSite.Class.Name.Should().Be("Program");
                e.LuaXStackTrace[3].CallSite.Name.Should().Be("Main");

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
        [InlineData(3, 75)]
        [InlineData(2, 45)]
        [InlineData(1, 25)]
        [InlineData(0, 15)]
        public void TestInnerClasses1(int param, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass1");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("complexClass", out var program).Should().BeTrue();
            program.SearchMethod("dummy", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsInteger().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { param }, out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1, "a", "a")]
        [InlineData(5, "b", "bbbbb")]
        [InlineData(3, "ab", "ababab")]
        public void TestInnerClasses2(int param1, string param2, string expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass2");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("program", out var program).Should().BeTrue();
            program.SearchMethod("main", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(2);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.Arguments[1].LuaType.IsString().Should().BeTrue();
            method.ReturnType.IsString().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { param1, param2 }, out var r);
            r.Should().BeOfType<string>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 4)]
        [InlineData(3, 27)]
        [InlineData(4, 256)]
        [InlineData(5, 3125)]
        public void TestInnerClasses3(int param1, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass3");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("program", out var program).Should().BeTrue();
            program.SearchMethod("main", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsInteger().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { param1 }, out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 15)]
        [InlineData(4, 172)]
        [InlineData(5, 2345)]
        public void TestInnerClasses4(int param1, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass4");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("program", out var program).Should().BeTrue();
            program.SearchMethod("main", null, out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsInteger().Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { param1 }, out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }
    }
}



