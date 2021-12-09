using FluentAssertions;
using Luax.Interpreter.Execution.Coverage;
using Luax.Interpreter.Expression;
using Luax.Interpreter.Infrastructure;
using Luax.Parser;
using Luax.Parser.Test.Utils;
using Xunit;

namespace Luax.Interpreter.Test.Coverage
{
    public class Coverage
    {
        [Fact]
        public void RunCoverage()
        {
            var app = new LuaXApplication();
            app.CompileResource("InnerClass1");
            app.Pass2();
            var typelib = new LuaXTypesLibrary(app);

            var analyzer = new CoverageAnalyzer(typelib);

            typelib.SearchClass("complexClass", out var @class).Should().BeTrue();
            @class.SearchMethod("dummy", null, out var method).Should().BeTrue();

            LuaXMethodExecutor.Execute(method, typelib, null, new object[] { 1 }, out _);

            var classReport = analyzer.Report.Find(@class.LuaType);
            var methodReport = classReport.Find("dummy");

            methodReport.GetCoverage(out var total, out var covered);
            total.Should().Be(3);
            covered.Should().Be(3);
        }
    }
}
