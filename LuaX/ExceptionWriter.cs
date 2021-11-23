using System;
using System.Text;
using Luax.Interpreter;
using Luax.Interpreter.Infrastructure.Stdlib;
using Luax.Parser.Ast;

namespace LuaX
{
    public static class ExceptionWriter
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
                writerAction($"{nameof(LuaXExecutionException)}: {executionError.Message}.{(executionError.Properties["code"] != null ? $" Code: {executionError.Properties["code"].Value}." : "")}");
                foreach (var stackFrame in executionError.LuaXStackTrace)
                    writerAction($"\tat {BuildMethodSignature(stackFrame.CallSite)} in {stackFrame.Location.Source}({stackFrame.Location.Line},{stackFrame.Location.Column})");

                return -4;
            }
            writerAction("Unexpected exception:");
            writerAction(e.ToString());
            return -5;
        }

        private static string BuildMethodSignature(LuaXMethod method)
        {
            var sb = new StringBuilder();
            sb.Append(method.Class.Name)
                .Append('.')
                .Append(method.Name);
            sb.Append('(');

            for (var i = 0; i < method.Arguments.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(method.Arguments[i].Name)
                    .Append(" : ")
                    .Append(method.Arguments[i].LuaType.IsObject() ?
                                method.Arguments[i].LuaType.Class :
                                method.Arguments[i].LuaType.ToString());
            }

            sb.Append(')');

            return sb.ToString();
        }
    }
}


