using System;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using FluentAssertions;
using Luax.Parser.Ast;
using Luax.Parser.Ast.LuaExpression;
using Luax.Parser.Ast.Statement;
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
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("object");
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
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("object");
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
            property.Location.Line.Should().Be(7);

            property.Attributes.Count.Should().Be(1);
            property.Attributes[0].Name.Should().Be("attr");
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
        public void ParseClassConstants()
        {
            var root = Compiler.CompileResource("ParseClassConstants");

            root.Classes.Should().HaveCount(1);
            var @class = root.Classes[0];

            @class.Constants.Should().HaveCount(4);

            var property = @class.Constants[0];
            property.Name.Should().Be("i");
            property.Value.ConstantType.Should().Be(LuaXType.Integer);
            property.Value.Value.Should().Be(10);

            property = @class.Constants[1];
            property.Name.Should().Be("pi");
            property.Value.ConstantType.Should().Be(LuaXType.Real);
            property.Value.Value.Should().Be(3.1415);

            property = @class.Constants[2];
            property.Name.Should().Be("name");
            property.Value.ConstantType.Should().Be(LuaXType.String);
            property.Value.Value.Should().Be("a");

            property = @class.Constants[3];
            property.Name.Should().Be("success");
            property.Value.ConstantType.Should().Be(LuaXType.Boolean);
            property.Value.Value.Should().Be(true);

            property.Attributes.Should().HaveCount(1);
            property.Attributes[0].Name.Should().Be("attr");
        }

        [Fact]
        public void Stdlib()
        {
            var body = StdlibHeader.ReadStdlib();

            body.Classes.Search("csvParser", out var csvParser).Should().BeTrue();
            csvParser.Should().NotBeNull();

            csvParser.SearchMethod("splitLine", out var m).Should().BeTrue();
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

        [Fact]
        public void Expression()
        {
            var app = new LuaXApplication();
            app.CompileResource("Expression1");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();
            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(2);
            method.Statements[1].Should().BeOfType<LuaXReturnStatement>();
            var expr = method.Statements[1].As<LuaXReturnStatement>().Expression;
            expr.Should().NotBeNull();

            expr.Should().BeOfType<LuaXBinaryOperatorExpression>();
            var expr1 = expr.As<LuaXBinaryOperatorExpression>();
            expr1.Location.Line.Should().Be(5);
            expr1.Location.Column.Should().Be(40);
            expr1.ToString().Should().Be("(((call:stdlib::abs(arg:arg) Subtract var:r) Multiply const:int:2) Add (const:int:5 Multiply ((arg:arg Divide const:int:2) Power const:int:2)))");
        }

        [Fact]
        public void If()
        {
            var app = new LuaXApplication();
            app.CompileResource("If1");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();
            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(2);
            method.Statements[0].Should().BeOfType<LuaXIfStatement>();
            var @if = method.Statements[0].As<LuaXIfStatement>();
            @if.Clauses.Should().HaveCount(2);
            @if.Clauses[0].Condition.ToString().Should().Be("(arg:arg1 Greater const:int:0)");
            @if.Clauses[0].Statements.Should().HaveCount(1);
            @if.Clauses[0].Statements[0].Should().BeOfType<LuaXReturnStatement>();
            @if.Clauses[0].Statements[0].As<LuaXReturnStatement>().Expression.ToString().Should().Be("const:int:1");

            @if.Clauses[1].Condition.ToString().Should().Be("(arg:arg1 Less const:int:0)");
            @if.Clauses[1].Statements.Should().HaveCount(1);
            @if.Clauses[1].Statements[0].Should().BeOfType<LuaXReturnStatement>();
            @if.Clauses[1].Statements[0].As<LuaXReturnStatement>().Expression.ToString().Should().Be("const:int:-1");

            @if.ElseClause.Should().HaveCount(1);
            @if.ElseClause[0].Should().BeOfType<LuaXReturnStatement>();
            @if.ElseClause[0].As<LuaXReturnStatement>().Expression.ToString().Should().Be("cast<int>(const:real:0)");

            @if = method.Statements[1].As<LuaXIfStatement>();
            @if.Clauses.Should().HaveCount(1);
            @if.ElseClause.Should().BeEmpty();
        }

        [Fact]
        public void WhileBreakContinue()
        {
            var app = new LuaXApplication();
            app.CompileResource("While1");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();
            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(2);
            method.Statements[0].Should().BeOfType<LuaXAssignVariableStatement>();
            method.Statements[1].Should().BeOfType<LuaXWhileStatement>();

            var @while = method.Statements[1].As<LuaXWhileStatement>();
            @while.WhileCondition.ToString().Should().Be("(var:i Greater const:int:0)");
            @while.Statements.Should().HaveCount(3);
            @while.Statements[0].Should().BeOfType<LuaXIfStatement>();

            var @if = @while.Statements[0].As<LuaXIfStatement>();
            @if.Clauses.Should().HaveCount(1);
            @if.Clauses[0].Condition.ToString().Should().Be("(arg:arg1 Equal var:i)");
            @if.Clauses[0].Statements.Should().HaveCount(1);
            @if.Clauses[0].Statements[0].Should().BeOfType<LuaXBreakStatement>();

            @while.Statements[1].Should().BeOfType<LuaXAssignVariableStatement>();
            var assign = @while.Statements[1].As<LuaXAssignVariableStatement>();
            assign.VariableName.Should().Be("i");
            assign.Expression.ToString().Should().Be("(var:i Subtract const:int:1)");
            @while.Statements[2].Should().BeOfType<LuaXContinueStatement>();
        }

        [Fact]
        public void BreakOutOfLoop1()
        {
            var app = new LuaXApplication();
            app.CompileResource("Break1");
            var ex = Assert.Throws<LuaXAstGeneratorException>(() => app.Pass2());
            Assert.Contains("The break statement is not in a loop", ex.Message);
        }

        [Fact]
        public void BreakOutOfLoop2()
        {
            var app = new LuaXApplication();
            app.CompileResource("Break2");
            var ex = Assert.Throws<LuaXAstGeneratorException>(() => app.Pass2());
            Assert.Contains("The break statement is not in a loop", ex.Message);
        }

        [Fact]
        public void ContinueOutOfLoop1()
        {
            var app = new LuaXApplication();
            app.CompileResource("Continue1");
            var ex = Assert.Throws<LuaXAstGeneratorException>(() => app.Pass2());
            Assert.Contains("The continue statement is not in a loop", ex.Message);
        }

        [Fact]
        public void ContinueOutOfLoop2()
        {
            var app = new LuaXApplication();
            app.CompileResource("Continue2");
            var ex = Assert.Throws<LuaXAstGeneratorException>(() => app.Pass2());
            Assert.Contains("The continue statement is not in a loop", ex.Message);
        }

        [Fact]
        public void ClassAsArray_Success()
        {
            var app = new LuaXApplication();
            app.CompileResource("ClassAsArray");
            ((Action)(() => app.Pass2())).Should().NotThrow<LuaXAstGeneratorException>();
            app.Classes.Search("test", out var @class);
            @class.Methods.Search("test", out var @method);
            method.Statements[0].Should().BeOfType<LuaXAssignVariableStatement>();
            var stmt1 = method.Statements[0].As<LuaXAssignVariableStatement>();
            stmt1.Expression.Should().BeOfType<LuaXInstanceCallExpression>();
            var expr1 = stmt1.Expression.As<LuaXInstanceCallExpression>();
            expr1.ReturnType.IsReal().Should().BeTrue();
            expr1.Object.ReturnType.Class.Should().Be("test");
            expr1.MethodName.Should().Be("get");
            expr1.Arguments.Should().HaveCount(1);
            expr1.Arguments[0].Should().BeOfType<LuaXConstantExpression>();
            expr1.Arguments[0].As<LuaXConstantExpression>().Value.Value.Should().Be("a");

            var stmt2 = method.Statements[1].As<LuaXCallStatement>();
            stmt2.CallExpression.Should().BeOfType<LuaXInstanceCallExpression>();
            var expr2 = stmt2.CallExpression.As<LuaXInstanceCallExpression>();
            expr2.ReturnType.IsVoid().Should().BeTrue();
            expr2.Object.ReturnType.Class.Should().Be("test");
            expr2.MethodName.Should().Be("set");
            expr2.Arguments.Should().HaveCount(2);
            expr2.Arguments[0].Should().BeOfType<LuaXConstantExpression>();
            expr2.Arguments[0].As<LuaXConstantExpression>().Value.Value.Should().Be("b");
            expr2.Arguments[1].Should().BeOfType<LuaXVariableExpression>();
            expr2.Arguments[1].As<LuaXVariableExpression>().VariableName.Should().Be("x");
        }

        [Fact]
        public void Try()
        {
            var app = new LuaXApplication();
            app.CompileResource("TryCatch1");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();
            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(1);
            method.Statements[0].Should().BeOfType<LuaXTryStatement>();
            var @try = method.Statements[0].As<LuaXTryStatement>();

            @try.TryStatements.Should().HaveCount(2);

            @try.TryStatements[0].Should().BeOfType<LuaXIfStatement>();
            @try.TryStatements[0].As<LuaXIfStatement>().Clauses.Count.Should().Be(1);
            @try.TryStatements[0].As<LuaXIfStatement>().ElseClause.Count.Should().Be(0);

            @try.TryStatements[1].Should().BeOfType<LuaXReturnStatement>();
            @try.TryStatements[1].As<LuaXReturnStatement>().Expression.ToString().Should().Be("(arg:arg Greater const:int:0)");

            @try.CatchClause.Should().BeOfType<LuaXCatchClause>();
            var @catch = @try.CatchClause;
            @catch.CatchIdentifier.Should().Be("ex");

            @catch.CatchStatements.Should().HaveCount(1);
            @catch.CatchStatements[0].Should().BeOfType<LuaXReturnStatement>();
            @catch.CatchStatements[0].As<LuaXReturnStatement>().Expression.ToString().Should().Be("const:boolean:False");
        }

        [Fact]
        public void Throw()
        {
            var app = new LuaXApplication();
            app.CompileResource("Throw1");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();
            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(2);
            method.Statements[0].Should().BeOfType<LuaXThrowStatement>();
            var throw1 = method.Statements[0].As<LuaXThrowStatement>();
            @throw1.ThrowExpression.Should().BeOfType<LuaXStaticCallExpression>();
            var throw1Expr = throw1.ThrowExpression.As<LuaXStaticCallExpression>();
            throw1Expr.ReturnType.IsObject().Should().BeTrue();
            throw1Expr.ReturnType.Class.Should().Be("exception");
            throw1Expr.Arguments.Count.Should().Be(2);
            var throwArg1 = throw1.ThrowExpression.As<LuaXStaticCallExpression>().Arguments[0];
            throwArg1.Should().BeOfType(typeof(LuaXConstantExpression));
            throwArg1.ReturnType.IsInteger().Should().BeTrue();
            throwArg1.ToString().Should().Be("const:int:0");
            var throwArg2 = throw1.ThrowExpression.As<LuaXStaticCallExpression>().Arguments[1];
            throwArg2.Should().BeOfType(typeof(LuaXConstantExpression));
            throwArg2.ReturnType.IsString().Should().BeTrue();
            throwArg2.ToString().Should().Be("const:string:exception1");

            var @throw2 = method.Statements[1].As<LuaXThrowStatement>();
            @throw2.ThrowExpression.Should().BeOfType<LuaXStaticCallExpression>();
            @throw2.ThrowExpression.As<LuaXStaticCallExpression>().ReturnType.IsObject().Should().BeTrue();
            @throw2.ThrowExpression.As<LuaXStaticCallExpression>().ReturnType.Class.Should().Be("exception");
            @throw2.ThrowExpression.As<LuaXStaticCallExpression>().ToString().Should().Be("call:test::getError()");
        }

        [Fact]
        public void InnerClassValid()
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass1");
            app.Pass2();
            app.Classes.Search("complexClass", out var @class).Should().BeTrue();
            @class.Constructor.Should().BeNull();
            @class.SearchMethod("dummy", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(5);
            @class.Classes.Should().HaveCount(2);
            @class.Classes.Search("innerClass", out var innerClass).Should().BeTrue();
            innerClass.Constructor.Should().NotBeNull();
            innerClass.SearchMethod("method", out var anotherMethod).Should().BeTrue();
            anotherMethod.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            anotherMethod.Statements.Should().HaveCount(1);
        }
    }
}
