using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Interpreter.Expression;
using Luax.Interpreter.Infrastructure;
using Luax.Interpreter.Infrastructure.Stdlib;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Execution
{
    /// <summary>
    /// The executor for the tests
    /// </summary>
    public class LuaXTestExecutor : LuaXProjectExecutorBase
    {
        public event EventHandler<LuaXTestStatusEventArgs> OnTest;

        public LuaXTestExecutor(LuaXProject project) : base(project)
        {
            Initialize(project);
        }

        public bool Success { get; private set; }

        override public int Run(string[] args)
        {
            Success = true;
            foreach (var className in TypesLibrary.GetClassNames())
            {
                TypesLibrary.SearchClass(className, out var @class);
                if (@class.LuaType.Attributes.Any(a => a.Name == "TestSuite"))
                    RunTestSuite(@class);
            }
            return Success ? 0 : 1;
        }

        private void RunTestSuite(LuaXClassInstance @class)
        {
            foreach (var method in @class.LuaType.Methods)
            {
                if (method.Attributes.Any(a => a.Name == "Fact"))
                    Success &= RunFact(@class, method);
                else if (method.Attributes.Any(a => a.Name == "Theory"))
                    Success &= RunTheory(@class, method);
            }
        }

        private bool RunTheory(LuaXClassInstance @class, LuaXMethod method)
        {
            if (method.Arguments.Count == 0 || !method.ReturnType.IsVoid() || method.Static)
            {
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, LuaXTestStatus.Incorrect, "The theory method should be an instance method and have arguments and void return type"));
                return false;
            }

            bool success = true;

            foreach (var attribute in method.Attributes.Where(a => a.Name == "TheoryData"))
            {
                StringBuilder sb = new StringBuilder();
                List<object> args = new List<object>();
                foreach (var p in attribute.Parameters)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    if (p.Value == null)
                        sb.Append("nil");
                    else if (p.Value is int i)
                        sb.Append(i);
                    else if (p.Value is double v)
                        sb.Append(v);
                    else if (p.Value is bool b)
                        sb.Append(b ? "true" : "false");
                    else if (p.Value is string s)
                        sb.Append('"').Append(s).Append('"');
                    args.Add(p.Value);
                }

                var argsText = sb.ToString();

                if (args.Count != method.Arguments.Count)
                {
                    OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Incorrect, "The theory data arguments count does not match to theory arguments count"));
                    success = false;
                    continue;
                }

                for (int i = 0; i < args.Count; i++)
                {
                    if (!((args[i] == null && method.Arguments[i].LuaType.IsString()) ||
                          (args[i] is string && method.Arguments[i].LuaType.IsString()) ||
                          (args[i] is bool && method.Arguments[i].LuaType.IsBoolean()) ||
                          (args[i] is int && method.Arguments[i].LuaType.IsInteger()) ||
                          (args[i] is double && method.Arguments[i].LuaType.IsReal())))
                        OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Incorrect, $"The theory data {i + 1}th argument type is does not match to the theory argument type"));
                }

                success &= RunCase(@class, method, args.ToArray(), argsText);
            }
            return success;
        }

        private bool RunFact(LuaXClassInstance @class, LuaXMethod method)
        {
            if (method.Arguments.Count != 0 || !method.ReturnType.IsVoid() || method.Static)
            {
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, LuaXTestStatus.Incorrect, "The fact method should be an instance method and should have void return type and no arguments"));
                return false;
            }

            return RunCase(@class, method, Array.Empty<object>(), "");
        }

        private bool RunCase(LuaXClassInstance @class, LuaXMethod method, object[] args, string argsText)
        {
            bool success = true;

            try
            {
                var @this = @class.New(TypesLibrary);
                LuaXMethodExecutor.Execute(method, TypesLibrary, @this, args, out var _);
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.OK, ""));
                success = false;
            }
            catch (LuaXAssertionException assertion)
            {
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Assert, $"Assertion: {assertion.Message}"));
                success = false;
            }
            catch (LuaXExecutionException executionException)
            {
                if (executionException.InnerException is LuaXAssertionException assertionException1)
                {
                    OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Assert, $"Assertion: {assertionException1.Message}"));
                }
                else
                {
                    OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Assert, $"Exception: {executionException.Message}"));
                    success = false;
                }
            }
            catch (LuaXAstGeneratorException error)
            {
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Incorrect, $"Unexpected exception {error.SourceName}({error.Errors[0].Line},{error.Errors[0].Column}) - {error.Errors[0].Message}"));
                success = false;
            }
            catch (Exception exception)
            {
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Exception, $"Unexpected exception {exception.GetType().Name}: {exception.Message}"));
                success = false;
            }

            return success;
        }
    }
}
