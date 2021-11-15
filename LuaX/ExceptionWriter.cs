using System;
using Luax.Interpreter;
using Luax.Interpreter.Infrastructure.Stdlib;
using Luax.Parser.Ast;

namespace LuaX
{
    internal static class ExceptionWriter
    {
        public static int WriteException(Exception e, Action<string> writerAction)
        {
            if (e is LuaXAssertionException assertion)
            {
                writerAction($"Unexpected assertion {assertion.Message}");
                return -2;
            }
            else if (e is LuaXAstGeneratorException error)
            {
                var errors = error.Errors;
                foreach (var err in errors)
                    writerAction($"{error.SourceName}({err.Line},{err.Column}) - {err.Message}");
                return -3;
            }
            else if (e is LuaXExecutionException executionError)
            {
                writerAction($"{executionError.Locations[0].Source}({executionError.Locations[0].Line},{executionError.Locations[0].Column}) - {executionError.Message}");
                for (int i = 1; i < executionError.Locations.Count; i++)
                    writerAction($"   called from {executionError.Locations[i].Source}({executionError.Locations[i].Line},{executionError.Locations[i].Column})");
                return -4;
            }
            writerAction("Unexpected exception:");
            writerAction(e.ToString());
            return -5;
        }
    }
}


