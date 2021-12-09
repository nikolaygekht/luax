using Luax.Interpreter.Expression;
using Luax.Interpreter.Infrastructure;

namespace Luax.Interpreter.Execution.Coverage
{
    public class CoverageAnalyzer
    {
        public CoverageReport Report { get; }

        public CoverageAnalyzer(LuaXTypesLibrary typesLibrary)
        {
            Report = new CoverageReport(typesLibrary);
            LuaXMethodExecutor.BeforeStatementExecution += OnStatementIsAboutToBeExecuted;
        }

        public void OnStatementIsAboutToBeExecuted(object _, BeforeStatementExecutionEventArgs args)
        {
            var @class = Report.Find(args.Method.Class);
            if (@class == null)
                return;
            var method = @class.Find(args.Method.Name);
            if (method == null)
                return;
            var stmt = method.Find(args.Statement);
            if (stmt != null)
                stmt.Count++;
        }
    }

}
