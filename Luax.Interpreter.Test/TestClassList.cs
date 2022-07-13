using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Luax.Interpreter.Infrastructure;
using Luax.Interpreter.Infrastructure.Stdlib;
using Luax.Parser;
using Luax.Parser.Ast;
using Luax.Parser.Test.Utils;
using Xunit;

namespace Luax.Interpreter.Test
{
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
            var jdn1 = StdlibDate.DateToJDN(date);
            jdn1.Should().BeApproximately(jdn, 0.000000115740740740741);
            var date1 = StdlibDate.JdnToDate(jdn);
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
            LuaXObjectInstance instance = @class.New(null);
            foreach (var property in properties)
                instance.Properties[property].Should().NotBeNull();
        }

        [Theory]
        [InlineData("a")]
        [InlineData("b")]
        [InlineData("c")]
        [InlineData("d")]
        public void NotHaveOwner(string sourceClassName)
        {
            mTypeLib.SearchClass(sourceClassName, out var @class).Should().BeTrue();
            LuaXObjectInstance instance = @class.New(null);
            instance.OwnerObjectInstance.Should().BeNull();
        }

        [Fact]
        public void HaveOwner()
        {
            mTypeLib.SearchClass("a", out var @classA);
            LuaXObjectInstance instanceA = @classA.New(null);
            mTypeLib.SearchClass("b", out var @classB);
            LuaXObjectInstance instanceB = @classB.New(null, instanceA);
            instanceB.OwnerObjectInstance.Should().BeSameAs(instanceA);
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
                instance = @class.New(null);
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
                instance = @class.New(null);
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

            LuaXObjectInstance instance = @class.New(mTypeLib);

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


