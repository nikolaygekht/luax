using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Luax.Interpreter.Expression;
using Luax.Interpreter.Infrastructure;
using Luax.Parser;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Statement;
using Luax.Parser.Test.Utils;
using Xunit;

#pragma warning disable IDE0038 // Use pattern matching

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

            var instance = @class.New();
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
    }

    public class TestClassList
    {
        private readonly LuaXTypesLibrary mTypeLib;

        public TestClassList()
        {
            var app = new LuaXApplication();
            app.CompileResource("ClassesTest1");
            app.Pass2();
            mTypeLib = new LuaXTypesLibrary(app);
        }

        [Theory]
        [InlineData(2021, 10, 29, 13, 07, 35, 245, 2459517.546935706018518)]
        public void JDN(int year, int month, int day, int hour, int minute, int second, int millisecond, double jdn)
        {
            var date = new DateTime(year, month, day, hour, minute, second, millisecond);
            var jdn1 = LuaXTypesLibrary.DateToJDN(date);
            jdn1.Should().BeApproximately(jdn, 0.000000115740740740741);
            var date1 = LuaXTypesLibrary.JdnToDate(jdn);
            date1.Should().BeCloseTo(date, TimeSpan.FromSeconds(0.01));
        }

        [Theory]
        [InlineData(1, typeof(int), LuaXType.Integer, 1, typeof(int))]
        [InlineData(1, typeof(int), LuaXType.Real, 1.0, typeof(double))]
        [InlineData(2459517, typeof(int), LuaXType.Datetime, "2021-10-29", typeof(DateTime))]
        [InlineData(123, typeof(int), LuaXType.String, "123", typeof(string))]

        [InlineData(1.1, typeof(double), LuaXType.Integer, 1, typeof(int))]
        [InlineData(1.1, typeof(double), LuaXType.Real, 1.1, typeof(double))]
        [InlineData(2459517.5, typeof(double), LuaXType.Datetime, "2021-10-29 12:00:00", typeof(DateTime))]
        [InlineData(1.23, typeof(double), LuaXType.String, "1.23", typeof(string))]

        [InlineData(true, typeof(bool), LuaXType.Boolean, true, typeof(bool))]
        [InlineData(true, typeof(bool), LuaXType.String, "true", typeof(string))]
        [InlineData(false, typeof(bool), LuaXType.String, "false", typeof(string))]
        [InlineData(true, typeof(bool), LuaXType.Integer, 1, typeof(int))]
        [InlineData(false, typeof(bool), LuaXType.Integer, 0, typeof(int))]
        [InlineData(true, typeof(bool), LuaXType.Real, 1, typeof(double))]
        [InlineData(false, typeof(bool), LuaXType.Real, 0, typeof(double))]

        [InlineData("2021-10-29 12:00:00", typeof(DateTime), LuaXType.Integer, 2459517, typeof(int))]
        [InlineData("2021-10-29 12:00:00", typeof(DateTime), LuaXType.Datetime, "2021-10-29 12:00:00", typeof(DateTime))]
        [InlineData("2021-10-29 12:00:00", typeof(DateTime), LuaXType.Real, 2459517.5, typeof(double))]
        [InlineData("2021-10-29 00:00:00", typeof(DateTime), LuaXType.String, "2021-10-29", typeof(string))]
        [InlineData("2021-10-29 00:00:03", typeof(DateTime), LuaXType.String, "2021-10-29 00:00:03", typeof(string))]
        [InlineData("2021-10-29 00:00:00.001", typeof(DateTime), LuaXType.String, "2021-10-29 00:00:00.001", typeof(string))]

        [InlineData("1", typeof(string), LuaXType.Integer, 1, typeof(int))]
        [InlineData("1", typeof(string), LuaXType.String, "1", typeof(string))]
        [InlineData("1.23", typeof(string), LuaXType.Real, 1.23, typeof(double))]
        [InlineData("true", typeof(string), LuaXType.Boolean, true, typeof(bool))]
        [InlineData("false", typeof(string), LuaXType.Boolean, false, typeof(bool))]
        [InlineData("2010-11-15", typeof(string), LuaXType.Datetime, "2010-11-15", typeof(DateTime))]
        [InlineData("2010-11-15 12:22", typeof(string), LuaXType.Datetime, "2010-11-15 12:22:00", typeof(DateTime))]
        [InlineData("2010-11-15 12:22:55", typeof(string), LuaXType.Datetime, "2010-11-15 12:22:55", typeof(DateTime))]
        [InlineData("2010-11-15 12:22:55.123", typeof(string), LuaXType.Datetime, "2010-11-15 12:22:55.123", typeof(DateTime))]

        public void CastPrimitives_Success(object value, Type valueType, LuaXType luaType, object expectedValue, Type expectedValueType)
        {
            value = TestValue.Translate(valueType, value);
            expectedValue = TestValue.Translate(expectedValueType, expectedValue);

            mTypeLib.CastTo(new LuaXTypeDefinition() { TypeId = luaType }, ref value)
                .Should().BeTrue();

            value.Should()
                .NotBeNull()
                .And.BeOfType(expectedValueType)
                .And.Be(expectedValue);
        }

        [Theory]
        [InlineData(1, typeof(int), LuaXType.Boolean)]
        [InlineData(1.1, typeof(double), LuaXType.Boolean)]
        [InlineData("wrong", typeof(string), LuaXType.Boolean)]
        [InlineData("wrong", typeof(string), LuaXType.Integer)]
        [InlineData("wrong", typeof(string), LuaXType.Real)]
        [InlineData("wrong", typeof(string), LuaXType.Datetime)]
        public void CastPrimitives_Fail(object value, Type valueType, LuaXType luaType)
        {
            value = TestValue.Translate(valueType, value);
            mTypeLib.CastTo(new LuaXTypeDefinition() { TypeId = luaType }, ref value)
                .Should().BeFalse();
        }

        [Theory]
        [InlineData("a", true)]
        [InlineData("b", true)]
        [InlineData("c", true)]
        [InlineData("d", true)]
        [InlineData("e", true)]
        [InlineData("f", true)]
        public void TestClass(string className, bool exists)
        {
            mTypeLib.SearchClass(className, out _).Should().Be(exists);
        }

        [Theory]
        [InlineData("a", "a1")]
        [InlineData("b", "a1", "b1")]
        [InlineData("c", "a1", "c1")]
        [InlineData("d", "a1", "b1", "d1")]
        [InlineData("e", "a1", "b1", "d1", "e1")]
        [InlineData("f", "f1")]
        public void TestProperties(string className, params string[] properties)
        {
            mTypeLib.SearchClass(className, out var @class).Should().BeTrue();
            LuaXObjectInstance instance = @class.New();
            foreach (var property in properties)
                instance.Properties[property].Should().NotBeNull();
        }

        [Theory]
        [InlineData("a", "a")]
        [InlineData("b", "a")]
        [InlineData("c", "a")]
        [InlineData("d", "a")]
        [InlineData("d", "b")]
        [InlineData("e", "d")]
        [InlineData(null, "a")]
        [InlineData(null, "f")]
        [InlineData(null, "b")]
        public void CastClasses_Success(string sourceClassName, string targetClassName)
        {
            LuaXObjectInstance instance = null;
            if (!string.IsNullOrEmpty(sourceClassName))
            {
                mTypeLib.SearchClass(sourceClassName, out var @class).Should().BeTrue();
                instance = @class.New();
            }

            object a = instance;

            mTypeLib.CastTo(new LuaXTypeDefinition() { TypeId = LuaXType.Object, Class = targetClassName }, ref a)
                .Should().BeTrue();

            a.Should().BeSameAs(instance);
        }

        [Theory]
        [InlineData("a", "f")]
        [InlineData("f", "a")]
        [InlineData("c", "b")]
        [InlineData("b", "c")]
        [InlineData("a", "b")]
        public void CastClasses_Fail(string sourceClassName, string targetClassName)
        {
            LuaXObjectInstance instance = null;
            if (!string.IsNullOrEmpty(sourceClassName))
            {
                mTypeLib.SearchClass(sourceClassName, out var @class).Should().BeTrue();
                instance = @class.New();
            }

            object a = instance;

            mTypeLib.CastTo(new LuaXTypeDefinition() { TypeId = LuaXType.Object, Class = targetClassName }, ref a)
                .Should().BeFalse();
        }

        [Fact]
        public void DefaultPropertyValues()
        {
            mTypeLib.SearchClass("g", out var @class).Should().BeTrue();

            @class.StaticProperties["s1"]
                .Should()
                .NotBeNull()
                .And.Match<LuaXVariableInstance>(p => p.LuaType.IsInteger() && p.Value is int && (int)p.Value == 0);

            @class.StaticProperties["p1"]
                .Should().BeNull();

            LuaXObjectInstance instance = @class.New();

            instance.Properties["s1"]
                .Should().BeNull();

            instance.Properties["p1"]
                .Should()
                .NotBeNull()
                .And.Match<LuaXVariableInstance>(p => p.LuaType.IsInteger() && p.Value is int && (int)p.Value == 0);

            instance.Properties["p2"]
                .Should()
                .NotBeNull()
                .And.Match<LuaXVariableInstance>(p => p.LuaType.IsReal() && p.Value is double && (double)p.Value == 0);

            instance.Properties["p3"]
                .Should()
                .NotBeNull()
                .And.Match<LuaXVariableInstance>(p => p.LuaType.IsBoolean() && p.Value is bool && !(bool)p.Value);

            instance.Properties["p4"]
                .Should()
                .NotBeNull()
                .And.Match<LuaXVariableInstance>(p => p.LuaType.IsString() && p.Value == null);

            instance.Properties["p5"]
                .Should()
                .NotBeNull()
                .And.Match<LuaXVariableInstance>(p => p.LuaType.IsDate() && p.Value is DateTime && (DateTime)p.Value == new DateTime(1900, 1, 1));

            instance.Properties["p6"]
                .Should()
                .NotBeNull()
                .And.Match<LuaXVariableInstance>(p => p.LuaType.IsObject() && p.LuaType.Class == "a" && p.Value == null);

            instance.Properties["p7"]
                .Should()
                .NotBeNull()
                .And.Match<LuaXVariableInstance>(p => p.LuaType.IsArrayOf(LuaXType.Integer, "") && p.Value == null);
        }
    }
}

