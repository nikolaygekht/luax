using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Statement;

namespace Luax.Interpreter.Execution.Coverage
{
    public class CoverageReportMethod
    {
        public LuaXMethod Method { get; }

        public string Name => Method.Name;

        public int Line => Method.Location.Line;

        public int Column => Method.Location.Column;

        private readonly List<CoverageReportStatement> mStatements = new List<CoverageReportStatement>();

        private readonly Dictionary<LuaXElementLocation, CoverageReportStatement> mIndex = new Dictionary<LuaXElementLocation, CoverageReportStatement>();

        public IReadOnlyList<CoverageReportStatement> Statements => mStatements;

        public CoverageReportStatement Find(LuaXStatement statement)
        {
            if (mIndex.TryGetValue(statement.Location, out var r))
                return r;
            return null;
        }

        public CoverageReportMethod(LuaXMethod method)
        {
            Method = method;
            foreach (var statement in method.Statements)
            {
                AddStatement(statement);
            }
        }

        private void AddStatement(LuaXStatement statement)
        {
            var reportStatement = new CoverageReportStatement(statement);
            mStatements.Add(reportStatement);
            if (mIndex.ContainsKey(statement.Location))
                throw new InvalidOperationException($"Two statements has the same location: {statement.Location.Source}({statement.Location.Line}:{statement.Location.Column})");
            mIndex[statement.Location] = reportStatement;

            if (statement is LuaXIfStatement @if)
                AddCompound(@if);
            else if (statement is LuaXWhileStatement @while)
                AddCompound(@while);
            else if (statement is LuaXRepeatStatement @repeat)
                AddCompound(@repeat);
            else if (statement is LuaXForStatement @for)
                AddCompound(@for);
            else if (statement is LuaXTryStatement @try)
                AddCompound(@try);
        }

        private void AddCompound(IEnumerable<LuaXStatement> statements)
        {
            foreach (var statement in statements)
                AddStatement(statement);
        }

        private void AddCompound(LuaXIfStatement statement)
        {
            foreach (var clause in statement.Clauses)
                AddCompound(clause.Statements);
            if (statement.ElseClause != null)
                AddCompound(statement.ElseClause);
        }

        private void AddCompound(LuaXWhileStatement statement)
            => AddCompound(statement.Statements);

        private void AddCompound(LuaXRepeatStatement statement)
            => AddCompound(statement.Statements);

        private void AddCompound(LuaXForStatement statement)
            => AddCompound(statement.Statements);

        private void AddCompound(LuaXTryStatement statement)
        {
            AddCompound(statement.TryStatements);
            AddCompound(statement.CatchClause.CatchStatements);
        }

        internal void GetCoverage(out int totalStatements, out int coveredStatements)
        {
            totalStatements = coveredStatements = 0;
            foreach (var statement in mStatements)
            {
                totalStatements++;
                if (statement.Count > 0)
                    coveredStatements++;
            }
        }

        internal void SaveTo(TextWriter writer)
        {
            GetCoverage(out var totalStatements, out var coveredStatements);
            writer.WriteLine($"  <method name='{Method.Name}' location-line='{Method.Location.Line}' location-column='{Method.Location.Column}' total-statements='{totalStatements}' covered-statements='{coveredStatements}' coverage='{(totalStatements == 0 ? 0 : coveredStatements * 100 / totalStatements)}%'>");
            foreach (var statement in mStatements)
                statement.SaveTo(writer);
            writer.WriteLine("  </method>");
        }
    }
}
