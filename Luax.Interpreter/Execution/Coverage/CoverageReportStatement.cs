using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Luax.Parser.Ast.Statement;

namespace Luax.Interpreter.Execution.Coverage
{
    public class CoverageReportStatement
    {
        [XmlIgnore]
        public LuaXStatement Statement { get; }

        [XmlAttribute("count")]
        public int Count { get; set; }

        [XmlAttribute("line")]
        public int Line => Statement.Location.Line;

        [XmlAttribute("column")]
        public int Column => Statement.Location.Column;

        public CoverageReportStatement(LuaXStatement statement)
        {
            Statement = statement;
        }

        internal void SaveTo(TextWriter writer)
        {
            writer.WriteLine($"    <statement location-line='{Statement.Location.Line}' location-column='{Statement.Location.Column}' count='{Count}' />");
        }
    }
}
