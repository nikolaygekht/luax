using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Execution.Coverage
{
    public class CoverageReportClass
    {
        [XmlIgnore]
        public LuaXClass Class { get; }

        [XmlAttribute("name")]
        public string Name => Class.Name;

        [XmlAttribute("line")]
        public int Line => Class.Location.Line;

        [XmlAttribute("column")]
        public int Column => Class.Location.Column;

        private readonly List<CoverageReportMethod> mMethods = new List<CoverageReportMethod>();

        private readonly Dictionary<string, CoverageReportMethod> mIndex = new Dictionary<string, CoverageReportMethod>();

        [XmlArray("methods")]
        [XmlArrayItem("method")]
        public IReadOnlyList<CoverageReportMethod> Methods => mMethods;

        public CoverageReportMethod Find(string method)
        {
            if (mIndex.TryGetValue(method, out var r))
                return r;
            return null;
        }

        public CoverageReportClass(LuaXClass @class)
        {
            Class = @class;
            foreach (var method in @class.Methods)
            {
                var coverageMethod = new CoverageReportMethod(method);
                mMethods.Add(coverageMethod);
                mIndex[method.Name] = coverageMethod;
            }
        }

        internal void GetCoverage(out int totalStatements, out int coveredStatements)
        {
            totalStatements = coveredStatements = 0;
            foreach (var method in mMethods)
            {
                method.GetCoverage(out var total, out var covered);
                totalStatements += total;
                coveredStatements += covered;
            }
        }

        internal void SaveTo(TextWriter writer)
        {
            GetCoverage(out var totalStatements, out var coveredStatements);

            writer.WriteLine($" <class name='{Class.Name}' location-file='{Class.Location.Source}' location-line='{Class.Location.Line}' location-column='{Class.Location.Column}' total-statements='{totalStatements}' covered-statements='{coveredStatements}' coverage='{(totalStatements == 0 ? 0 : coveredStatements * 100 / totalStatements)}%'>");
            foreach (var method in mMethods)
                method.SaveTo(writer);
            writer.WriteLine(" </class>");
        }
    }
}
