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
        public void ParseClassesInPackage()
        {
            var root = Compiler.CompileResource("ParseClassesInPackage");

            root.Classes.Should().HaveCount(6);
            root.Packages.Should().HaveCount(2);

            var p = root.Packages[0];
            p.Name.Should().Be("myPackage");
            p.Attributes.Should().HaveCount(2);
            p.Location.Line.Should().Be(2);
            p.Location.Column.Should().Be(1);
            var q = p.Attributes[0];
            q.Name.Should().Be("s");
            q.Parameters.Should().HaveCount(1);
            q.Parameters[0].Value.Should().Be("my package");
            q = p.Attributes[1];
            q.Name.Should().Be("u");
            q.Parameters.Should().HaveCount(1);
            q.Parameters[0].Value.Should().Be("qqq");

            p = root.Packages[1];
            p.Name.Should().Be("anotherPackage");
            p.Attributes.Should().BeEmpty();

            var c = root.Classes[0];
            c.Name.Should().Be("a");
            c.PackageName.Should().Be("myPackage");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("object");
            c.Attributes.Should().BeEmpty();
            c.Location.Source.Should().Be("ParseClassesInPackage");
            c.Location.Line.Should().Be(3);
            c.Location.Column.Should().Be(3);

            c = root.Classes[1];
            c.Name.Should().Be("b");
            c.PackageName.Should().Be("myPackage");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("a");
            c.Attributes.Should().BeEmpty();
            c.Location.Source.Should().Be("ParseClassesInPackage");
            c.Location.Line.Should().Be(6);
            c.Location.Column.Should().Be(3);

            c = root.Classes[2];
            c.Name.Should().Be("c");
            c.PackageName.Should().Be("myPackage");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("b");
            c.Location.Source.Should().Be("ParseClassesInPackage");
            c.Location.Line.Should().Be(10);
            c.Location.Column.Should().Be(3);

            c.Attributes.Should().HaveCount(1);
            var a = c.Attributes[0];
            a.Name.Should().Be("x");
            c.PackageName.Should().Be("myPackage");
            a.Parameters.Should().BeEmpty();
            a.Location.Source.Should().Be("ParseClassesInPackage");
            a.Location.Line.Should().Be(9);
            a.Location.Column.Should().Be(3);

            c = root.Classes[3];
            c.Name.Should().Be("d");
            c.PackageName.Should().Be("myPackage");
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

            c = root.Classes[4];
            c.Name.Should().Be("h");
            c.PackageName.Should().Be("anotherPackage");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("object");
            c.Attributes.Should().BeEmpty();

            c = root.Classes[5];
            c.Name.Should().Be("q");
            c.PackageName.Should().Be("");
            c.HasParent.Should().BeTrue();
            c.Parent.Should().Be("object");
            c.Attributes.Should().BeEmpty();
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
        public void RepeatBreakContinue()
        {
            var app = new LuaXApplication();
            app.CompileResource("Repeat");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();
            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(3);
            method.Statements[0].Should().BeOfType<LuaXAssignVariableStatement>();
            method.Statements[1].Should().BeOfType<LuaXRepeatStatement>();
            method.Statements[2].Should().BeOfType<LuaXReturnStatement>();

            var @repeat = method.Statements[1].As<LuaXRepeatStatement>();
            @repeat.UntilCondition.ToString().Should().Be("(var:i Greater const:int:0)");
            @repeat.Statements.Should().HaveCount(4);
            @repeat.Statements[0].Should().BeOfType<LuaXIfStatement>();

            var @if = @repeat.Statements[0].As<LuaXIfStatement>();
            @if.Clauses.Should().HaveCount(1);
            @if.Clauses[0].Condition.ToString().Should().Be("(arg:arg1 Equal var:i)");
            @if.Clauses[0].Statements.Should().HaveCount(1);
            @if.Clauses[0].Statements[0].Should().BeOfType<LuaXBreakStatement>();

            @repeat.Statements[1].Should().BeOfType<LuaXAssignVariableStatement>();
            var assign = @repeat.Statements[1].As<LuaXAssignVariableStatement>();
            assign.VariableName.Should().Be("i");
            assign.Expression.ToString().Should().Be("(var:i Subtract const:int:1)");

            @repeat.Statements[2].Should().BeOfType<LuaXContinueStatement>();

            @repeat.Statements[3].Should().BeOfType<LuaXAssignVariableStatement>();
            var assign2 = @repeat.Statements[3].As<LuaXAssignVariableStatement>();
            assign2.VariableName.Should().Be("i");
            assign2.Expression.ToString().Should().Be("(var:i Subtract const:int:1000)");
        }

        [Fact]
        public void BreakOutOfLoop1()
        {
            var app = new LuaXApplication();
            app.CompileResource("Break1");
            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("The break statement is not in a loop"));
        }

        [Fact]
        public void BreakOutOfLoop2()
        {
            var app = new LuaXApplication();
            app.CompileResource("Break2");
            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("The break statement is not in a loop"));
        }

        [Fact]
        public void ContinueOutOfLoop1()
        {
            var app = new LuaXApplication();
            app.CompileResource("Continue1");
            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("The continue statement is not in a loop"));
        }

        [Fact]
        public void ContinueOutOfLoop2()
        {
            var app = new LuaXApplication();
            app.CompileResource("Continue2");
            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("The continue statement is not in a loop"));
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
        public void Try1()
        {
            var app = new LuaXApplication();
            app.CompileResource("TryCatch1");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();
            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Variables.Should().HaveCount(1);
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
        public void Try2()
        {
            var app = new LuaXApplication();
            app.CompileResource("TryCatch2");
            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("Identifier of declared variable of type exception is expected here"));
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
            method.Statements.Should().HaveCount(6);
            app.Classes.Search("complexClass.innerClass", out var innerClass).Should().BeTrue();
            innerClass.Constructor.Should().NotBeNull();
            innerClass.Parent.Should().Be("complexClass.parent");
            innerClass.SearchMethod("method", out var anotherMethod).Should().BeTrue();
            anotherMethod.ReturnType.TypeId.Should().Be(LuaXType.Integer);
            anotherMethod.Statements.Should().HaveCount(1);
        }

        [Fact]
        public void InnerClassValidOwnerClass()
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass1");
            app.Pass2();
            app.Classes.Search("complexClass", out var @class).Should().BeTrue();
            @class.Constructor.Should().BeNull();
            app.Classes.Search("complexClass.parent", out var innerClass).Should().BeTrue();
            innerClass.SearchMethod("parentMethod", out var parentMethod).Should().BeTrue();
            parentMethod.Statements.Should().HaveCount(1);
            parentMethod.Statements[0].Should().BeOfType<LuaXReturnStatement>();
            var returnStatement = (LuaXReturnStatement)parentMethod.Statements[0];
            returnStatement.Expression.Should().BeOfType<LuaXBinaryOperatorExpression>();
            var expression = (LuaXBinaryOperatorExpression)returnStatement.Expression;

            expression.LeftArgument.Should().BeOfType<LuaXInstancePropertyExpression>();
            var inst1 = (LuaXInstancePropertyExpression)expression.LeftArgument;
            inst1.PropertyName.Should().Be("property");
            inst1.Object.Should().BeOfType<LuaXVariableExpression>();
            var var1 = (LuaXVariableExpression)inst1.Object;
            var1.VariableName.Should().Be("this");
            var1.ReturnType.Class.Should().Be("complexClass");

            expression.RightArgument.Should().BeOfType<LuaXInstanceCallExpression>();
            var inst2 = (LuaXInstanceCallExpression)expression.RightArgument;
            inst2.MethodName.Should().Be("privateFunc");
            inst2.Object.Should().BeOfType<LuaXVariableExpression>();
            var var2 = (LuaXVariableExpression)inst1.Object;
            var2.VariableName.Should().Be("this");
            var2.ReturnType.Class.Should().Be("complexClass");
        }

        [Fact]
        public void InnerClassInvalid1()
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass2");
            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("is a private method"));
        }

        [Fact]
        public void OtherClassMethodCallValid1()
        {
            var app = new LuaXApplication();
            app.CompileResource("OtherClassMethodCall1");
            app.Pass2();
            app.Classes.Search("a", out var @classA).Should().BeTrue();
            app.Classes.Search("b", out var @classB).Should().BeTrue();
            @classB.Parent.Should().Be("a");
            @classA.SearchMethod("method", out _).Should().BeTrue();
            @classB.SearchMethod("dummy", out _).Should().BeTrue();
        }

        [Fact]
        public void OtherClassMethodCallInvalid1()
        {
            var app = new LuaXApplication();
            app.CompileResource("OtherClassMethodCall2");
            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("is a private method"));
        }

        [Fact]
        public void OtherClassMethodCallInvalid2()
        {
            var app = new LuaXApplication();
            app.CompileResource("OtherClassMethodCall4");
            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("is a private method"));
        }

        [Fact]
        public void OtherClassMethodCallValid2()
        {
            var app = new LuaXApplication();
            app.CompileResource("OtherClassMethodCall3");
            app.Pass2();
            app.Classes.Search("a", out var @classA).Should().BeTrue();
            app.Classes.Search("b", out var @classB).Should().BeTrue();
            @classA.SearchMethod("method", out _).Should().BeTrue();
            @classB.SearchMethod("dummy", out _).Should().BeTrue();
        }

        [Fact]
        public void OtherClassMethodCallValid3()
        {
            var app = new LuaXApplication();
            app.CompileResource("OtherClassMethodCall5");
            app.Pass2();
            app.Classes.Search("a", out var @classA).Should().BeTrue();
            app.Classes.Search("b", out var @classB).Should().BeTrue();
            @classA.SearchMethod("method", out var method).Should().BeTrue();
            method.Static.Should().BeTrue();
            @classB.SearchMethod("dummy", out _).Should().BeTrue();
        }

        [Fact]
        public void ForBreakContinueInt()
        {
            var app = new LuaXApplication();
            app.CompileResource("ForBreakContinue");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();

            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(3);

            method.Statements[0].Should().BeOfType<LuaXAssignVariableStatement>();
            method.Statements[1].Should().BeOfType<LuaXForStatement>();

            var @for = method.Statements[1].As<LuaXForStatement>();
            @for.ForLoopDescription.Start.ToString().Should().Be("const:int:0");
            @for.ForLoopDescription.Limit.ToString().Should().Be("const:int:10");
            @for.ForLoopDescription.Step.ToString().Should().Be("const:int:1");
            @for.ForLoopDescription.Variable.Name.Should().Be("i");

            @for.Statements.Should().HaveCount(3);

            var ifContinue = @for.Statements[0].As<LuaXIfStatement>();
            ifContinue.Clauses.Should().HaveCount(1);
            ifContinue.Clauses[0].Condition.ToString().Should().Be("(var:i Equal const:int:0)");
            ifContinue.Clauses[0].Statements.Should().HaveCount(1);
            ifContinue.Clauses[0].Statements[0].Should().BeOfType<LuaXContinueStatement>();

            @for.Statements[1].Should().BeOfType<LuaXAssignVariableStatement>();
            var assign = @for.Statements[1].As<LuaXAssignVariableStatement>();
            assign.Expression.ToString().Should().Be("(var:i Add const:int:1)");

            var ifBreak = @for.Statements[2].As<LuaXIfStatement>();
            ifBreak.Clauses.Should().HaveCount(1);
            ifBreak.Clauses[0].Condition.ToString().Should().Be("(var:j Equal var:i)");
            ifBreak.Clauses[0].Statements.Should().HaveCount(1);
            ifBreak.Clauses[0].Statements[0].Should().BeOfType<LuaXBreakStatement>();
        }

        [Fact]
        public void ForBreakContinueReal()
        {
            var app = new LuaXApplication();
            app.CompileResource("ForBreakContinue");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();

            @class.SearchMethod("test2", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(3);

            method.Statements[0].Should().BeOfType<LuaXAssignVariableStatement>();
            method.Statements[1].Should().BeOfType<LuaXForStatement>();

            var @for = method.Statements[1].As<LuaXForStatement>();
            @for.ForLoopDescription.Start.ToString().Should().Be("const:real:0");
            @for.ForLoopDescription.Limit.ToString().Should().Be("const:real:10");
            @for.ForLoopDescription.Step.ToString().Should().Be("const:real:1");
            @for.ForLoopDescription.Variable.Name.Should().Be("i");

            @for.Statements.Should().HaveCount(3);

            var ifContinue = @for.Statements[0].As<LuaXIfStatement>();
            ifContinue.Clauses.Should().HaveCount(1);
            ifContinue.Clauses[0].Condition.ToString().Should().Be("(var:i Equal const:real:0)");
            ifContinue.Clauses[0].Statements.Should().HaveCount(1);
            ifContinue.Clauses[0].Statements[0].Should().BeOfType<LuaXContinueStatement>();

            @for.Statements[1].Should().BeOfType<LuaXAssignVariableStatement>();
            var assign = @for.Statements[1].As<LuaXAssignVariableStatement>();
            assign.Expression.ToString().Should().Be("(var:i Add const:real:1)");

            var ifBreak = @for.Statements[2].As<LuaXIfStatement>();
            ifBreak.Clauses.Should().HaveCount(1);
            ifBreak.Clauses[0].Condition.ToString().Should().Be("(var:j Equal var:i)");
            ifBreak.Clauses[0].Statements.Should().HaveCount(1);
            ifBreak.Clauses[0].Statements[0].Should().BeOfType<LuaXBreakStatement>();
        }

        [Fact]
        public void ForReverseBreakContinueInt()
        {
            var app = new LuaXApplication();
            app.CompileResource("ForReverseBreakContinue");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();

            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(3);

            method.Statements[0].Should().BeOfType<LuaXAssignVariableStatement>();
            method.Statements[1].Should().BeOfType<LuaXForStatement>();

            var @for = method.Statements[1].As<LuaXForStatement>();
            @for.ForLoopDescription.Start.ToString().Should().Be("const:int:10");
            @for.ForLoopDescription.Limit.ToString().Should().Be("const:int:0");
            @for.ForLoopDescription.Step.ToString().Should().Be("const:int:-1");
            @for.ForLoopDescription.Variable.Name.Should().Be("i");

            @for.Statements.Should().HaveCount(3);

            var ifContinue = @for.Statements[0].As<LuaXIfStatement>();
            ifContinue.Clauses.Should().HaveCount(1);
            ifContinue.Clauses[0].Condition.ToString().Should().Be("(var:i Equal const:int:0)");
            ifContinue.Clauses[0].Statements.Should().HaveCount(1);
            ifContinue.Clauses[0].Statements[0].Should().BeOfType<LuaXContinueStatement>();

            @for.Statements[1].Should().BeOfType<LuaXAssignVariableStatement>();
            var assign = @for.Statements[1].As<LuaXAssignVariableStatement>();
            assign.Expression.ToString().Should().Be("(var:i Add const:int:1)");

            var ifBreak = @for.Statements[2].As<LuaXIfStatement>();
            ifBreak.Clauses.Should().HaveCount(1);
            ifBreak.Clauses[0].Condition.ToString().Should().Be("(var:j Equal var:i)");
            ifBreak.Clauses[0].Statements.Should().HaveCount(1);
            ifBreak.Clauses[0].Statements[0].Should().BeOfType<LuaXBreakStatement>();
        }

        [Fact]
        public void ForReverseBreakContinueReal()
        {
            var app = new LuaXApplication();
            app.CompileResource("ForReverseBreakContinue");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();

            @class.SearchMethod("test2", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(3);

            method.Statements[0].Should().BeOfType<LuaXAssignVariableStatement>();
            method.Statements[1].Should().BeOfType<LuaXForStatement>();

            var @for = method.Statements[1].As<LuaXForStatement>();
            @for.ForLoopDescription.Start.ToString().Should().Be("const:real:10");
            @for.ForLoopDescription.Limit.ToString().Should().Be("const:real:0");
            @for.ForLoopDescription.Step.ToString().Should().Be("const:real:-1");
            @for.ForLoopDescription.Variable.Name.Should().Be("i");

            @for.Statements.Should().HaveCount(3);

            var ifContinue = @for.Statements[0].As<LuaXIfStatement>();
            ifContinue.Clauses.Should().HaveCount(1);
            ifContinue.Clauses[0].Condition.ToString().Should().Be("(var:i Equal const:real:0)");
            ifContinue.Clauses[0].Statements.Should().HaveCount(1);
            ifContinue.Clauses[0].Statements[0].Should().BeOfType<LuaXContinueStatement>();

            @for.Statements[1].Should().BeOfType<LuaXAssignVariableStatement>();
            var assign = @for.Statements[1].As<LuaXAssignVariableStatement>();
            assign.Expression.ToString().Should().Be("(var:i Add const:real:1)");

            var ifBreak = @for.Statements[2].As<LuaXIfStatement>();
            ifBreak.Clauses.Should().HaveCount(1);
            ifBreak.Clauses[0].Condition.ToString().Should().Be("(var:j Equal var:i)");
            ifBreak.Clauses[0].Statements.Should().HaveCount(1);
            ifBreak.Clauses[0].Statements[0].Should().BeOfType<LuaXBreakStatement>();
        }

        [Fact]
        public void ForWithoutIteratorVar()
        {
            var app = new LuaXApplication();
            app.CompileResource("ForWithoutIteratorExp");
            app.Pass2();
            app.Classes.Search("test", out var @class).Should().BeTrue();

            @class.SearchMethod("test1", out var method).Should().BeTrue();
            method.Statements.Should().HaveCount(3);

            method.Statements[0].Should().BeOfType<LuaXAssignVariableStatement>();
            method.Statements[1].Should().BeOfType<LuaXForStatement>();

            var @for = method.Statements[1].As<LuaXForStatement>();
            @for.ForLoopDescription.Start.ToString().Should().Be("const:int:0");
            @for.ForLoopDescription.Limit.ToString().Should().Be("const:int:10");
            @for.ForLoopDescription.Step.ToString().Should().Be("const:int:1");
            @for.ForLoopDescription.Variable.Name.Should().Be("i");

            @for.Statements.Should().HaveCount(1);

            @for.Statements[0].Should().BeOfType<LuaXAssignVariableStatement>();
            var assign = @for.Statements[0].As<LuaXAssignVariableStatement>();
            assign.Expression.ToString().Should().Be("(var:j Add const:int:1)");
        }

        [Fact]
        public void ForInitExprNonNumberType()
        {
            var app = new LuaXApplication();
            app.CompileResource("ForInitExprNonNumberType");

            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("Initialization part of for statement should be declaration and be int type"));
        }

        [Fact]
        public void ForLoopPartsWithDifferentType()
        {
            var app = new LuaXApplication();
            app.CompileResource("ForLoopPartsWithDifferentType");

            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("Initialization, condition and iteration parts of for statement should all be of the same type"));
        }

        [Fact]
        public void Override_Correct()
        {
            var app = new LuaXApplication();
            app.CompileResource("OverridingOK");

            ((Action)(() => app.Pass2())).Should().NotThrow<LuaXAstGeneratorException>();
        }

        [Theory]
        [InlineData("OverridingFail1")]
        [InlineData("OverridingFail2")]
        [InlineData("OverridingFail3")]
        [InlineData("OverridingFail4")]
        public void Override_Incorrect(string variant)
        {
            var app = new LuaXApplication();
            app.CompileResource(variant);

            ((Action)(() => app.Pass2())).Should().Throw<LuaXAstGeneratorException>();
        }

        [Fact]
        public void IncorrectFunctionDefinition()
        {
            var app = new LuaXApplication();
            app.CompileResource("IncorrectFunctionDefinition");
            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Errors[0].Line == 8 && e.Errors[0].Column == 4);
        }

        [Fact]
        public void HttpCommunicator()
        {
            var app = new LuaXApplication();
            app.Pass2();
            app.Classes.Search("httpCommunicator", out var httpCommunicator).Should().BeTrue();
            httpCommunicator.Constructor.Should().NotBeNull();
            httpCommunicator.Methods.Should().HaveCount(5);

            httpCommunicator.SearchMethod("get", out var method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeTrue();
            method.Arguments.Should().HaveCount(2);
            method.Arguments[0].LuaType.IsString().Should().BeTrue();
            method.Arguments[1].LuaType.IsObject().Should().BeTrue();
            method.Arguments[1].LuaType.Class.Should().Be("httpResponseCallback");
            method.ReturnType.IsVoid().Should().BeTrue();

            httpCommunicator.SearchMethod("post", out method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeTrue();
            method.Arguments.Should().HaveCount(3);
            method.Arguments[0].LuaType.IsString().Should().BeTrue();
            method.Arguments[1].LuaType.IsString().Should().BeTrue();
            method.Arguments[2].LuaType.IsObject().Should().BeTrue();
            method.Arguments[2].LuaType.Class.Should().Be("httpResponseCallback");
            method.ReturnType.IsVoid().Should().BeTrue();

            httpCommunicator.SearchMethod("setRequestHeader", out method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeTrue();
            method.Arguments.Should().HaveCount(2);
            method.Arguments[0].LuaType.IsString().Should().BeTrue();
            method.Arguments[1].LuaType.IsString().Should().BeTrue();
            method.ReturnType.IsVoid().Should().BeTrue();

            httpCommunicator.SearchMethod("cancel", out method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeTrue();
            method.Arguments.Should().HaveCount(0);
            method.ReturnType.IsVoid().Should().BeTrue();
        }

        [Fact]
        public void HttpResponseCallback()
        {
            var app = new LuaXApplication();
            app.Pass2();
            app.Classes.Search("httpResponseCallback", out var httpResponseCallback).Should().BeTrue();
            httpResponseCallback.Constructor.Should().BeNull();
            httpResponseCallback.Methods.Should().HaveCount(3);

            httpResponseCallback.SearchMethod("onComplete", out var method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeFalse();
            method.Arguments.Should().HaveCount(2);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.Arguments[1].LuaType.IsString().Should().BeTrue();
            method.ReturnType.IsVoid().Should().BeTrue();

            httpResponseCallback.SearchMethod("onError", out method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeFalse();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsString().Should().BeTrue();
            method.ReturnType.IsVoid().Should().BeTrue();

            httpResponseCallback.SearchMethod("onStateChange", out method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeFalse();
            method.Arguments.Should().HaveCount(1);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.ReturnType.IsVoid().Should().BeTrue();
        }

        [Fact]
        public void Scheduler()
        {
            var app = new LuaXApplication();
            app.Pass2();
            app.Classes.Search("scheduler", out var scheduler).Should().BeTrue();
            scheduler.Constructor.Should().BeNull();
            scheduler.Methods.Should().HaveCount(4);

            scheduler.SearchMethod("startImmediately", out var method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeTrue();
            method.Arguments.Should().HaveCount(0);
            method.ReturnType.IsVoid().Should().BeTrue();

            scheduler.SearchMethod("startWithDelay", out method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeTrue();
            method.Arguments.Should().HaveCount(0);
            method.ReturnType.IsVoid().Should().BeTrue();

            scheduler.SearchMethod("stop", out method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeFalse();
            method.Extern.Should().BeTrue();
            method.Arguments.Should().HaveCount(0);
            method.ReturnType.IsVoid().Should().BeTrue();

            scheduler.SearchMethod("create", out method).Should().BeTrue();
            method.Visibility.Should().Be(LuaXVisibility.Public);
            method.Static.Should().BeTrue();
            method.Extern.Should().BeTrue();
            method.Arguments.Should().HaveCount(2);
            method.Arguments[0].LuaType.IsInteger().Should().BeTrue();
            method.Arguments[1].LuaType.IsObject().Should().BeTrue();
            method.Arguments[1].LuaType.Class.Should().Be("action");
            method.ReturnType.IsObject().Should().BeTrue();
            method.ReturnType.Class.Should().Be("scheduler");
        }

        [Fact]
        public void UndefinedVarType()
        {
            var app = new LuaXApplication();
            app.CompileResource("UndefinedVarType");

            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("Variable type someUnknownClass is not defined"));
        }

        [Fact]
        public void UndefinedPropertyType()
        {
            var app = new LuaXApplication();
            app.CompileResource("UndefinedPropertyType");

            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("Property type someUnknownClass is not defined"));
        }

        [Fact]
        public void OwnerMethod()
        {
            var app = new LuaXApplication();
            app.CompileResource("OwnerMethod");

            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("Method test.testInner.test2 is not found"));
        }

        [Fact]
        public void OwnerProperty1()
        {
            var app = new LuaXApplication();
            app.CompileResource("OwnerProperty1");

            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("Class test.testInner does not contain property prop1"));
        }

        [Fact]
        public void OwnerProperty2()
        {
            var app = new LuaXApplication();
            app.CompileResource("OwnerProperty2");

            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("Class test.testInner does not contain property prop1"));
        }

        [Fact]
        public void SameNameMethodAndProperty()
        {
            var app = new LuaXApplication();
            app.CompileResource("SameNameMethodAndProperty");

            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("The class already have property/method with same name"));
        }

        [Fact]
        public void OverrideStaticMethod()
        {
            var app = new LuaXApplication();
            app.CompileResource("OverrideStaticMethod");

            Action act = () => app.Pass2();
            act.Should().Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("is static and can not be overriden"));
        }

        [Theory]
        [InlineData("x1", "a", null, "a1")]
        [InlineData("x2", "a", null, "a1")]
        [InlineData("x3", "b", null, "b1")]
        [InlineData("x4", "a", "x4", "l1")]
        [InlineData("x5", null, null, null)]

        public void ConstInExprReferenceToClass(string targetMethod, string className, string methodName, string constName)
        {
            var app = new LuaXApplication();
            app.CompileResource("ConstInExpr");
            app.Pass2();
            app.Classes.Search("a", out var @class).Should().BeTrue();
            @class.Methods.Search(targetMethod, out var @method).Should().BeTrue();
            
            method.Statements.Should().HaveCount(1);
            
            method.Statements[0]
                .Should()
                .BeOfType<LuaXReturnStatement>();
            
            method.Statements[0]
                .As<LuaXReturnStatement>()
                .Expression
                .Should()
                .BeOfType<LuaXConstantExpression>();
            
            var expr = method.Statements[0].As<LuaXReturnStatement>().Expression.As<LuaXConstantExpression>();
            
            expr.Should().BeOfType<LuaXConstantExpression>();

            if (className == null)
                expr.Source.Should().BeNull();
            else
            {
                expr.Source.Should().NotBeNull();
                expr.Source.Class.Name.Should().Be(className);
                expr.Source.Constant.Name.Should().Be(constName);

                if (methodName == null)
                    expr.Source.Method.Should().BeNull();
                else
                    expr.Source.Method.Name.Should().Be(methodName);
            }
        }

        [Fact]
        public void CallInstanceAsStatic()
        {
            var app = new LuaXApplication();
            app.CompileResource("CallInstanceAsStatic");
            ((Action)(() => app.Pass2())).Should()
                .Throw<LuaXAstGeneratorException>()
                .Where(e => e.Message.Contains("is not a static method"));
        }
    }
}
