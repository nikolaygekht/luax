﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Luax.Interpreter.Infrastructure;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Execution.Coverage
{
    public class CoverageReport
    {
        private readonly List<CoverageReportClass> mClasses = new List<CoverageReportClass>();

        private readonly Dictionary<string, CoverageReportClass> mIndex = new Dictionary<string, CoverageReportClass>();

        public IReadOnlyList<CoverageReportClass> Classes => mClasses;

        public CoverageReportClass Find(LuaXClass @class)
        {
            if (mIndex.TryGetValue(@class.Name, out var r))
                return r;
            return null;
        }

        private static bool ShouldBeCovered(LuaXClassInstance classInstance)
        {
            if (classInstance.LuaType.Attributes.Any(attr => attr.Name == "ExcludeFromCoverage"))
                return false;

            if (classInstance.LuaType.Location.Source == "stdlib.luax" ||
                    classInstance.LuaType.Location.Source == "typeslib.luax" ||
                    classInstance.LuaType.Location.Source == "internal")
                return false;

            return true;
        }

        public CoverageReport(LuaXTypesLibrary typesLibrary)
        {
            foreach (var luaType in typesLibrary.Classes.Where(ci => ShouldBeCovered(ci)).Select(ci => ci.LuaType))
            {
                var classCoverage = new CoverageReportClass(luaType);
                mClasses.Add(classCoverage);
                mIndex[luaType.Name] = classCoverage;
            }
        }

        internal void GetCoverage(out int totalStatements, out int coveredStatements)
        {
            totalStatements = coveredStatements = 0;
            foreach (var @class in mClasses)
            {
                @class.GetCoverage(out var total, out var covered);
                totalStatements += total;
                coveredStatements += covered;
            }
        }

        public void SaveTo(Stream stream, Encoding encoding)
        {
            using var textWriter = new StreamWriter(stream, encoding, 4096, true);
            textWriter.WriteLine($"<?xml version='1.0' encoding='{encoding.WebName}'?>");
            GetCoverage(out var totalStatements, out var coveredStatements);
            textWriter.WriteLine($"<report total-statements='{totalStatements}' covered-statements='{coveredStatements}' coverage='{(totalStatements == 0 ? 0 : coveredStatements * 100 / totalStatements)}%'>");
            foreach (var @class in mClasses)
                @class.SaveTo(textWriter);
            textWriter.WriteLine("</report>");
        }
    }
}
