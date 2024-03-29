﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public int TotalTests { get; private set;  } = 0;
        public int SuccessfullTests { get; private set; } = 0;

        public LuaXTestExecutor(LuaXProject project) : base(project)
        {
            Initialize(project);
        }

        public bool Success { get; private set; }

        override protected int RunBody(string[] args)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ProcessTheoryParams(LuaXAttribute attribute, List<object> args)
        {
            var sb = new StringBuilder();
            foreach (var p in attribute.Parameters.Select(p => p.Value))
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                if (p == null)
                    sb.Append("nil");
                else if (p is int i)
                    sb.Append(i);
                else if (p is double v)
                    sb.Append(v);
                else if (p is bool b)
                    sb.Append(b ? "true" : "false");
                else if (p is string s)
                    sb.Append('"').Append(s).Append('"');
                args.Add(p);
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTheoryArgumentValid(object arg, LuaXTypeDefinition type)
        {
            return (arg == null && type.IsString()) ||
                          (arg is string && type.IsString()) ||
                          (arg is bool && type.IsBoolean()) ||
                          (arg is int && type.IsInteger()) ||
                          (arg is double && type.IsReal());
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
                List<object> args = new List<object>();
                var argsText = ProcessTheoryParams(attribute, args);

                if (args.Count != method.Arguments.Count)
                {
                    OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Incorrect, "The theory data arguments count does not match to theory arguments count"));
                    success = false;
                    continue;
                }

                bool matchargs = true;
                for (int i = 0; i < args.Count; i++)
                {
                    if (!IsTheoryArgumentValid(args[i], method.Arguments[i].LuaType))
                    {
                        OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Incorrect, $"The theory data {i + 1}th argument type is does not match to the theory argument type"));
                        success = false;
                        matchargs = false;
                    }
                }

                if (matchargs)
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

        private bool RunStep(Func<bool> action, LuaXClassInstance @class, LuaXMethod method, string argsText)
        {
            bool success = true;
            try
            {
                success = action();
            }
            catch (LuaXAssertionException assertion)
            {
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Assert, $"Assertion: {assertion.Message}", assertion));
                success = false;
            }
            catch (LuaXExecutionException executionException)
            {
                if (executionException.InnerException is LuaXAssertionException assertionException1)
                    OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Assert, $"Assertion: {assertionException1.Message}", executionException));
                else
                    OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Exception, $"Exception: {executionException.Message}", executionException));
                success = false;
            }
            catch (LuaXAstGeneratorException error)
            {
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Incorrect, $"Unexpected exception {error.SourceName}({error.Errors[0].Line},{error.Errors[0].Column}) - {error.Errors[0].Message}", error));
                success = false;
            }
            catch (Exception exception)
            {
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.Exception, $"Unexpected exception {exception.GetType().Name}: {exception.Message}", exception));
                success = false;
            }
            return success;
        }

        private bool RunCase(LuaXClassInstance @class, LuaXMethod method, object[] args, string argsText)
        {
            TotalTests++;
            bool success = true;
            LuaXObjectInstance @this = null;

            success = RunStep(() =>
            {
                @this = @class.New(TypesLibrary);
                return true;
            }, @class, method, argsText);

            if (success)
            {
                success = RunStep(() =>
                {
                    LuaXMethodExecutor.Execute(method, TypesLibrary, @this, args, out var _);
                    return true;
                }, @class, method, argsText);
            }

            if (success)
            {
                OnTest?.Invoke(this, new LuaXTestStatusEventArgs(@class.LuaType.Name, method.Name, argsText, LuaXTestStatus.OK, ""));
                SuccessfullTests++;
            }

            var finalizer = @class.LuaType.Methods.FirstOrDefault(m => m.Attributes.Any(a => a.Name == "TearDown"));
            if (finalizer != null)
            {
                success = RunStep(() =>
                {
                    LuaXMethodExecutor.Execute(finalizer, TypesLibrary, @this, Array.Empty<object>(), out var _);
                    return true;
                }, @class, finalizer, argsText);
            }

            return success;
        }
    }
}
