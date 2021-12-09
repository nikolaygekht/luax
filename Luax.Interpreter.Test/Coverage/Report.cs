using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using FluentAssertions;
using Hime.SDK.Grammars;
using Luax.Interpreter.Execution.Coverage;
using Luax.Interpreter.Infrastructure;
using Luax.Parser;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Statement;
using Luax.Parser.Test.Utils;
using Xunit;

namespace Luax.Interpreter.Test.Coverage
{
    public class Report
    {
        [Fact]
        public void Statement()
        {
            var statement = new LuaXReturnStatement(null, new LuaXElementLocation("file", 1, 2));
            var reportStatement = new CoverageReportStatement(statement);
            reportStatement.Line.Should().Be(1);
            reportStatement.Column.Should().Be(2);
            reportStatement.Count.Should().Be(0);
        }

        [Fact]
        public void Method_Properties()
        {
            var @class = new LuaXClass("name", null, new LuaXElementLocation("file", 1, 2));
            var method1 = new LuaXMethod(@class)
            {
                Name = "method1",
                Location = new LuaXElementLocation("file", 1, 2)
            };
            @class.Methods.Add(method1);

            var methodReport = new CoverageReportMethod(method1);
            methodReport.Line.Should().Be(1);
            methodReport.Column.Should().Be(2);
        }

        [Fact]
        public void Method_StatementList()
        {
            var @class = new LuaXClass("name", null, new LuaXElementLocation("file", 1, 2));
            var method1 = new LuaXMethod(@class)
            {
                Name = "method1",
                Location = new LuaXElementLocation("file", 1, 2)
            };
            @class.Methods.Add(method1);

            var statement1 = new LuaXReturnStatement(null, new LuaXElementLocation("file", 2, 2));
            var statement2 = new LuaXReturnStatement(null, new LuaXElementLocation("file", 2, 5));
            var statement3 = new LuaXReturnStatement(null, new LuaXElementLocation("file", 3, 5));

            method1.Statements.Add(statement1);
            method1.Statements.Add(statement2);
            method1.Statements.Add(statement3);

            var methodReport = new CoverageReportMethod(method1);

            methodReport.Statements.Should()
                .HaveCount(3)
                .And.Contain(e => ReferenceEquals(e.Statement, statement1))
                .And.Contain(e => ReferenceEquals(e.Statement, statement2))
                .And.Contain(e => ReferenceEquals(e.Statement, statement3));

            methodReport.Find(statement1)
                .Should()
                .NotBeNull()
                .And.Match<CoverageReportStatement>(s => ReferenceEquals(s.Statement, statement1));

            methodReport.Find(statement2)
                .Should()
                .NotBeNull()
                .And.Match<CoverageReportStatement>(s => ReferenceEquals(s.Statement, statement2));

            methodReport.Find(statement3)
                .Should()
                .NotBeNull()
                .And.Match<CoverageReportStatement>(s => ReferenceEquals(s.Statement, statement3));
        }

        [Fact]
        public void Class_Properties()
        {
            var @class = new LuaXClass("name", null, new LuaXElementLocation("file", 1, 2));
            var reportClass = new CoverageReportClass(@class);
            reportClass.Line.Should().Be(1);
            reportClass.Column.Should().Be(2);
            reportClass.Class.Should().BeSameAs(@class);
        }

        [Fact]
        public void Class_MethodList()
        {
            var @class = new LuaXClass("name", null, new LuaXElementLocation("file", 1, 2));
            var method1 = new LuaXMethod(@class)
            {
                Name = "method1",
                Location = new LuaXElementLocation("file", 1, 2)
            };
            @class.Methods.Add(method1);

            var method2 = new LuaXMethod(@class)
            {
                Name = "method2",
                Location = new LuaXElementLocation("file", 2, 1)
            };
            @class.Methods.Add(method2);

            var reportClass = new CoverageReportClass(@class);
            reportClass.Methods.Should()
                .HaveCount(2)
                .And.Contain(m => m.Name == "method1")
                .And.Contain(m => m.Name == "method2");

            reportClass.Find("method1")
                .Should()
                .NotBeNull()
                .And.Match<CoverageReportMethod>(m => ReferenceEquals(m.Method, method1));

            reportClass.Find("method2")
                .Should()
                .NotBeNull()
                .And.Match<CoverageReportMethod>(m => ReferenceEquals(m.Method, method2));

            reportClass.Find("method3")
                .Should()
                .BeNull();
        }

        [Fact]
        public void Report_Content()
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass1");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);
            var report = new CoverageReport(typelib);

            report.Classes.Should()
                .HaveCount(3)
                .And.Contain(c => c.Class.Name == "complexClass")
                .And.Contain(c => c.Class.Name == "complexClass.parent")
                .And.Contain(c => c.Class.Name == "complexClass.innerClass");

            typelib.SearchClass("complexClass", out var @class);
            report.Find(@class.LuaType)
                .Should()
                .NotBeNull()
                .And.Match<CoverageReportClass>(c => object.ReferenceEquals(c.Class, @class.LuaType));
        }

        [Fact]
        public void Report_ToXML()
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass1");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            var report = new CoverageReport(typelib);

            typelib.SearchClass("complexClass", out var @class).Should().BeTrue();
            @class.SearchMethod("dummy", null, out var method).Should().BeTrue();
            var stmt = method.Statements[0];

            var stmtReport = report.Find(@class.LuaType).Find(method.Name).Find(stmt);
            stmtReport.Should().NotBeNull();
            stmtReport.Count++;

            XDocument xml;
            using (var ms = new MemoryStream())
            {
                report.SaveTo(ms, Encoding.UTF8);
                ms.Position = 0;
                xml = XDocument.Load(ms, LoadOptions.None);
            }

            xml.Should().NotBeNull();
            xml.Should()
                .HaveRoot("report");

            xml.Root.Should()
                .HaveAttribute("total-statements", "13")
                .And.HaveAttribute("covered-statements", "1")
                .And.HaveAttribute("coverage", "7%");

            xml.XPathSelectElement("/report/class[@name='complexClass']")
                .Should()
                .NotBeNull()
                .And.HaveAttribute("location-file", "InnerClass1")
                .And.HaveAttribute("location-line", "1")
                .And.HaveAttribute("location-column", "1")
                .And.HaveAttribute("total-statements", "9")
                .And.HaveAttribute("covered-statements", "1")
                .And.HaveAttribute("coverage", "11%");

            xml.XPathSelectElement("/report/class[@name='complexClass']/method[@name='dummy']")
                .Should()
                .NotBeNull()
                .And.HaveAttribute("location-line", "24")
                .And.HaveAttribute("location-column", "4")
                .And.HaveAttribute("total-statements", "3")
                .And.HaveAttribute("covered-statements", "1")
                .And.HaveAttribute("coverage", "33%");

            xml.XPathSelectElement("/report/class[@name='complexClass']/method[@name='dummy']/statement[position() = 1]")
                .Should()
                .NotBeNull()
                .And.HaveAttribute("location-line", "26")
                .And.HaveAttribute("location-column", "7")
                .And.HaveAttribute("count", "1");

            xml.XPathSelectElement("/report/class[@name='complexClass']/method[@name='dummy']/statement[position() = 2]")
                .Should()
                .NotBeNull()
                .And.HaveAttribute("location-line", "27")
                .And.HaveAttribute("location-column", "7")
                .And.HaveAttribute("count", "0");
        }
    }
}
