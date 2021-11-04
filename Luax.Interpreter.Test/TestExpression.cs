using System;
using System.Data;
using System.Reflection;
using FluentAssertions;
using Luax.Interpreter.Expression;
using Luax.Interpreter.Infrastructure;
using Luax.Parser;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Statement;
using Luax.Parser.Test.Utils;
using Xunit;

namespace Luax.Interpreter.Test
{
    public class TestExpression
    {
        private static void ValidateObject(object v)
        {
            v.Should().BeOfType<LuaXObjectInstance>();
            var obj = v as LuaXObjectInstance;
            obj.Class.LuaType.Name.Should().Be("a");
        }

        private static void ValidateArray(object v)
        {
            v.Should().BeOfType<LuaXVariableInstanceArray>();
            var arr = v as LuaXVariableInstanceArray;
            arr.ArrayType.IsArrayOf(LuaXType.Integer).Should().BeTrue();
            arr.ElementType.IsInteger().Should().BeTrue();
            arr.Length.Should().Be(32);
        }

        [Theory]
        //
        //constants
        //
        [InlineData("int[]", "nil", null, typeof(object))]
        [InlineData("string", "nil", null, typeof(string))]
        [InlineData("b", "nil", null, typeof(object))]
        [InlineData("int", "0", 0, typeof(int))]
        [InlineData("int", "123", 123, typeof(int))]
        [InlineData("real", "0", 0.0, typeof(double))]
        [InlineData("real", "0.0", 0.0, typeof(double))]
        [InlineData("real", "1.234", 1.234, typeof(double))]
        [InlineData("boolean", "true", true, typeof(bool))]
        [InlineData("boolean", "false", false, typeof(bool))]
        //
        //new 
        //
        [InlineData("int[]", "new int[32]", nameof(ValidateArray), typeof(Delegate))]
        [InlineData("a", "new a()", nameof(ValidateObject), typeof(Delegate))]
        //
        //array access
        //
        [InlineData("int", "arr[0]", 10, typeof(int))]
        [InlineData("int", "arr[1]", 20, typeof(int))]
        [InlineData("int", "arr[2]", 30, typeof(int))]
        [InlineData("int", "arr[-2]", 90, typeof(int))]
        [InlineData("int", "arr[-1]", 100, typeof(int))]
        [InlineData("int", "arr.length", 10, typeof(int))]
        //
        //variable & properties access
        //
        [InlineData("string", "x", "abc", typeof(string))]
        [InlineData("int", "s", 1, typeof(int))]
        [InlineData("int", "p", 2, typeof(int))]
        [InlineData("int", "i", 3, typeof(int))]
        //
        //unary operations
        //
        [InlineData("int", "-3", -3, typeof(int))]
        [InlineData("int", "-i", -3, typeof(int))]
        [InlineData("real", "-cast<real>(i) / 2", -1.5, typeof(double))]
        [InlineData("boolean", "not true", false, typeof(bool))]
        [InlineData("boolean", "not false", true, typeof(bool))]
        //math
        [InlineData("int", "1 + 1", 2, typeof(int))]
        [InlineData("int", "2 - 1", 1, typeof(int))]
        [InlineData("int", "2 * 2", 4, typeof(int))]
        [InlineData("int", "6 / 2", 3, typeof(int))]
        [InlineData("int", "5 / 2", 2, typeof(int))]
        [InlineData("int", "5 % 2", 1, typeof(int))]
        [InlineData("int", "5 ^ 2", 25, typeof(int))]
        [InlineData("real", "1.0 + 1.0", 2.0, typeof(double))]
        [InlineData("real", "2.0 - 1.0", 1.0, typeof(double))]
        [InlineData("real", "2.0 * 2.0", 4.0, typeof(double))]
        [InlineData("real", "6.0 / 2.0", 3.0, typeof(double))]
        [InlineData("real", "5.0 / 2.0", 2.5, typeof(double))]
        [InlineData("real", "5.0 % 2.0", 0.0, typeof(double))]
        [InlineData("real", "5.0 ^ 2.0", 25.0, typeof(double))]
        [InlineData("real", "1 + 1.0", 2.0, typeof(double))]
        [InlineData("real", "2.0 - 1", 1.0, typeof(double))]
        [InlineData("real", "2 + 1 / 2.0 ^ 2", 2.25, typeof(double))]
        //logical
        [InlineData("boolean", "true and true", true, typeof(bool))]
        [InlineData("boolean", "false and true", false, typeof(bool))]
        [InlineData("boolean", "true and false", false, typeof(bool))]
        [InlineData("boolean", "false and false", false, typeof(bool))]
        [InlineData("boolean", "true or true", true, typeof(bool))]
        [InlineData("boolean", "false or true", true, typeof(bool))]
        [InlineData("boolean", "true or false", true, typeof(bool))]
        [InlineData("boolean", "false or false", false, typeof(bool))]
        //concat
        [InlineData("string", "\"a\" .. \"b\"", "ab", typeof(string))]
        [InlineData("string", "\"a\" .. 1", "a1", typeof(string))]
        [InlineData("string", "1.0 .. \"b\"", "1b", typeof(string))]
        [InlineData("string", "true .. false", "truefalse", typeof(string))]
        //comparison - numeric
        [InlineData("boolean", "1 == 1", true, typeof(bool))]
        [InlineData("boolean", "1 == 0", false, typeof(bool))]
        [InlineData("boolean", "1 ~= 1", false, typeof(bool))]
        [InlineData("boolean", "1 ~= 0", true, typeof(bool))]
        [InlineData("boolean", "1.0 == 1.0", true, typeof(bool))]
        [InlineData("boolean", "1.0 == 0.0", false, typeof(bool))]
        [InlineData("boolean", "1.0 ~= 1.0", false, typeof(bool))]
        [InlineData("boolean", "1.0 ~= 0.0", true, typeof(bool))]
        [InlineData("boolean", "1 == 1.0", true, typeof(bool))]
        [InlineData("boolean", "1.0 == 0", false, typeof(bool))]
        [InlineData("boolean", "2 > 1", true, typeof(bool))]
        [InlineData("boolean", "2 > 2.0", false, typeof(bool))]
        [InlineData("boolean", "2 >= 1", true, typeof(bool))]
        [InlineData("boolean", "2 >= 2", true, typeof(bool))]
        [InlineData("boolean", "1 >= 2", false, typeof(bool))]
        [InlineData("boolean", "2 < 2", false, typeof(bool))]
        [InlineData("boolean", "1.0 < 2", true, typeof(bool))]
        [InlineData("boolean", "1 <= 2", true, typeof(bool))]
        [InlineData("boolean", "2 <= 2", true, typeof(bool))]
        [InlineData("boolean", "3 <= 2", false, typeof(bool))]
        [InlineData("boolean", "3.0 <= 2", false, typeof(bool))]
        //comparison string
        [InlineData("boolean", "\"a\" == \"a\"", true, typeof(bool))]
        [InlineData("boolean", "\"a\" ~= \"a\"", false, typeof(bool))]
        [InlineData("boolean", "\"a\" == \"b\"", false, typeof(bool))]
        [InlineData("boolean", "\"a\" ~= \"b\"", true, typeof(bool))]
        [InlineData("boolean", "\"b\" > \"a\"", true, typeof(bool))]
        [InlineData("boolean", "\"a\" > \"b\"", false, typeof(bool))]
        [InlineData("boolean", "\"aa\" > \"a\"", true, typeof(bool))]
        [InlineData("boolean", "\"b\" >= \"a\"", true, typeof(bool))]
        [InlineData("boolean", "\"b\" >= \"b\"", true, typeof(bool))]
        [InlineData("boolean", "\"b\" >= \"c\"", false, typeof(bool))]
        [InlineData("boolean", "\"b\" < \"a\"", false, typeof(bool))]
        [InlineData("boolean", "\"a\" < \"b\"", true, typeof(bool))]
        [InlineData("boolean", "\"aa\" < \"a\"", false, typeof(bool))]
        [InlineData("boolean", "\"a\" < \"aa\"", true, typeof(bool))]
        [InlineData("boolean", "\"a\" > \"A\"", true, typeof(bool))]
        [InlineData("boolean", "\"b\" <= \"a\"", false, typeof(bool))]
        [InlineData("boolean", "\"b\" <= \"b\"", true, typeof(bool))]
        [InlineData("boolean", "\"b\" <= \"c\"", true, typeof(bool))]
        //compare boolean
        [InlineData("boolean", "true == true", true, typeof(bool))]
        [InlineData("boolean", "false == false", true, typeof(bool))]
        [InlineData("boolean", "true ~= true", false, typeof(bool))]
        [InlineData("boolean", "false ~= false", false, typeof(bool))]
        [InlineData("boolean", "true == false", false, typeof(bool))]
        [InlineData("boolean", "false ~= true", true, typeof(bool))]
        //compare date
        [InlineData("boolean", "cast<datetime>(\"2021-11-22\") == cast<datetime>(\"2021-11-22\")", true, typeof(bool))]
        [InlineData("boolean", "cast<datetime>(\"2021-11-22\") == cast<datetime>(\"2021-11-22 01:00:00\")", false, typeof(bool))]
        [InlineData("boolean", "cast<datetime>(\"2021-11-22\") ~= cast<datetime>(\"2021-11-22\")", false, typeof(bool))]
        [InlineData("boolean", "cast<datetime>(\"2021-11-22\") ~= cast<datetime>(\"2021-11-22 01:00:00\")", true, typeof(bool))]
        [InlineData("boolean", "cast<datetime>(\"2021-11-22\") < cast<datetime>(\"2021-11-22 01:00:00\")", true, typeof(bool))]
        [InlineData("boolean", "cast<datetime>(\"2021-11-22\") > cast<datetime>(\"2021-11-22 01:00:00\")", false, typeof(bool))]
        //typename
        [InlineData("string", "typename(1)", "int", typeof(string))]
        [InlineData("string", "typename(1.0)", "real", typeof(string))]
        [InlineData("string", "typename(true)", "boolean", typeof(string))]
        [InlineData("string", "typename(nil)", "nil", typeof(string))]
        [InlineData("string", "typename(cast<datetime>(\"2021-11-22\"))", "datetime", typeof(string))]
        [InlineData("string", "typename(arr)", "int[]", typeof(string))]
        [InlineData("string", "typename(new a())", "a", typeof(string))]
        [InlineData("string", "typename(cast<object>(new a()))", "a", typeof(string))]

        public void TestExpression_SimpleData_Success(string returnType, string expression, object expectedValue, Type expectedType)
        {
            var app = new LuaXApplication();
            app.CompileResource("ExpressionTest", new[] {
                new Tuple<string, string>("$type", returnType),
                new Tuple<string, string>( "$expression", expression )
            });
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            expectedValue = TestValue.Translate(expectedType, expectedValue);
            typelib.SearchClass("b", out var @class);
            @class.Should().NotBeNull();
            @class.StaticProperties["s"].Value = 1;

            var instance = @class.New(typelib);
            instance.Properties["p"].Value = 2;
            instance.Properties["i"].Value = 3;

            var variables = new LuaXVariableInstanceSet();
            variables.Add(@class.LuaType.TypeOf(), "this", instance);
            variables.Add(LuaXTypeDefinition.String, "x", "abc");

            var arr = new LuaXVariableInstanceArray(LuaXTypeDefinition.Integer.ArrayOf(), 10);
            for (int i = 0; i < 10; i++)
                arr[i].Value = (i + 1) * 10;
            variables.Add(LuaXTypeDefinition.Integer.ArrayOf(), "arr", arr);

            @class.LuaType.SearchMethod("f", out var luaMethod);
            luaMethod.Should().NotBeNull();
            var luaExpression = luaMethod.Statements[0].As<LuaXReturnStatement>().Expression;

            var result = LuaXExpressionEvaluator.Evaluate(luaExpression, typelib, @class, variables);

            if (expectedValue == null)
                result.Should().BeNull();
            else if (expectedValue is string methodName && expectedType == typeof(Delegate))
            {
                var method = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
                method.Should().NotBeNull();
                method.IsStatic.Should().BeTrue();
                method.GetParameters()
                    .Should().HaveCount(1);
                method.GetParameters()[0].ParameterType.Should().Be(typeof(object));
                method.Invoke(null, new object[] { result });
            }
            else
            {
                result.Should().BeOfType(expectedType);
                result.Should().Be(expectedValue);
            }
        }

        [Theory]
        [InlineData("f1", true, "Cast to parent class")]
        [InlineData("f2", false, "Cast to child class")]
        [InlineData("f3", false, "Cast to class not in inheritance chain")]
        [InlineData("f4", true, "Save any class to object")]
        [InlineData("f5", true, "Save any class to object and cast to parent")]
        [InlineData("f6", false, "Cast to class not in inheritance chain")]
        [InlineData("f7", true, "Cast nil to any class")]
        [InlineData("f8", true, "Cast nil to any class")]
        [InlineData("f9", true, "Cast nil to string")]
        [InlineData("f10", false, "Cast nil to integer")]
        public void TestCastCompatibility(string function, bool success, string cause)
        {
            var app = new LuaXApplication();
            app.CompileResource("CastTest");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var @class);
            @class.Should().NotBeNull();
            @class.LuaType.SearchMethod(function, out var luaMethod);
            luaMethod.Should().NotBeNull();
            luaMethod.Statements.Should().HaveCount(2);
            luaMethod.Statements[0].Should().BeAssignableTo<LuaXAssignStatement>();
            luaMethod.Statements[1].Should().BeOfType<LuaXReturnStatement>();

            var variables = new LuaXVariableInstanceSet(luaMethod);
            variables["x"].Value = LuaXExpressionEvaluator.Evaluate(luaMethod.Statements[0].As<LuaXAssignStatement>().Expression,
                typelib, @class, variables);

            var action = (Action)(() => LuaXExpressionEvaluator.Evaluate(luaMethod.Statements[1].As<LuaXReturnStatement>().Expression,
                typelib, @class, variables));

            if (success)
                action.Should().NotThrow($"{cause} should be possible");
            else
                action.Should().Throw<LuaXExecutionException>($"{cause} should not be possible");
        }

        [Theory]
        //date/time functions
        [InlineData("datetime", "stdlib.mkdate(2021, 10, 15)", "2021-10-15", typeof(DateTime))]
        [InlineData("datetime", "stdlib.mkdatetime(2021, 10, 15, 22, 15, 47, 125)", "2021-10-15 22:15:47.125", typeof(DateTime))]
        [InlineData("int", "stdlib.day(stdlib.mkdatetime(2021, 10, 15, 22, 15, 47, 125))", 15, typeof(int))]
        [InlineData("int", "stdlib.month(stdlib.mkdatetime(2021, 10, 15, 22, 15, 47, 125))", 10, typeof(int))]
        [InlineData("int", "stdlib.year(stdlib.mkdatetime(2021, 10, 15, 22, 15, 47, 125))", 2021, typeof(int))]
        [InlineData("boolean", "stdlib.leapYear(stdlib.mkdate(2021, 10, 15))", false, typeof(bool))]
        [InlineData("boolean", "stdlib.leapYear(stdlib.mkdate(2012, 10, 15))", true, typeof(bool))]
        [InlineData("boolean", "stdlib.leapYear(stdlib.mkdate(2000, 10, 15))", true, typeof(bool))]
        [InlineData("boolean", "stdlib.leapYear(stdlib.mkdate(1900, 10, 15))", false, typeof(bool))]
        [InlineData("int", "stdlib.hour(stdlib.mkdatetime(2021, 10, 15, 22, 15, 47, 125))", 22, typeof(int))]
        [InlineData("int", "stdlib.minute(stdlib.mkdatetime(2021, 10, 15, 22, 15, 47, 125))", 15, typeof(int))]
        [InlineData("int", "stdlib.second(stdlib.mkdatetime(2021, 10, 15, 22, 15, 47, 125))", 47, typeof(int))]
        [InlineData("real", "stdlib.seconds(stdlib.mkdatetime(2021, 10, 15, 22, 15, 47, 125))", 47.125, typeof(double))]
        [InlineData("real", "stdlib.toJdn(stdlib.mkdatetime(2021, 10, 29, 13, 07, 35, 245))", 2459517.546935706018518, typeof(double))]
        [InlineData("datetime", "stdlib.fromJdn(2459517.5)", "2021-10-29 12:00:00", typeof(DateTime))]
        public void TestStdlib(string @return, string expr, object expectedValue, Type expectedType)
        {
            expectedValue = TestValue.Translate(expectedType, expectedValue);

            var app = new LuaXApplication();
            app.CompileResource("Stdlibtest", new Tuple<string, string>[]
            {
                new Tuple<string, string>("$return", @return),
                new Tuple<string, string>("$expr", expr)
            });
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var @class);
            @class.Should().NotBeNull();
            @class.SearchMethod("f", null, out var method);

            LuaXMethodExecutor.Execute(method, typelib, null, Array.Empty<object>(), out var r);
            r.Should().BeEquivalentTo(expectedValue);
        }

        [Fact]
        public void TestStdlib_nowlocal()
        {
            var app = new LuaXApplication();
            app.CompileResource("Stdlibtest", new Tuple<string, string>[]
            {
                new Tuple<string, string>("$return", "datetime"),
                new Tuple<string, string>("$expr", "stdlib.nowlocal()")
            });
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var @class);
            @class.Should().NotBeNull();
            @class.SearchMethod("f", null, out var method);

            LuaXMethodExecutor.Execute(method, typelib, null, Array.Empty<object>(), out var r);
            r.As<DateTime>().Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public void TestStdlib_nowutc()
        {
            var app = new LuaXApplication();
            app.CompileResource("Stdlibtest", new Tuple<string, string>[]
            {
                new Tuple<string, string>("$return", "datetime"),
                new Tuple<string, string>("$expr", "stdlib.nowutc()")
            });
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("test", out var @class);
            @class.Should().NotBeNull();
            @class.SearchMethod("f", null, out var method);

            LuaXMethodExecutor.Execute(method, typelib, null, Array.Empty<object>(), out var r);
            r.As<DateTime>().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
        }

        [Theory]
        [InlineData("b.a1", 1)]
        [InlineData("b.b1", 2)]
        [InlineData("c1", 3)]
        [InlineData("v1", 4)]
        public void TestConstants(string constantName, int expectedValue)
        {
            var app = new LuaXApplication();
            app.CompileResource("ConstTest", new Tuple<string, string>[]
            {
                new Tuple<string, string>("$const", constantName)
            });

            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            typelib.SearchClass("c", out var @class);
            @class.Should().NotBeNull();
            @class.SearchMethod("x", null, out var method);

            LuaXMethodExecutor.Execute(method, typelib, null, Array.Empty<object>(), out var r);
            r.Should().BeOfType<int>();
            r.Should().Be(expectedValue);
        }
    }
}



