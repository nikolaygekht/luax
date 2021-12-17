using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Interpreter.Expression;
using Luax.Interpreter.Infrastructure;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Execution
{
    /// <summary>
    /// The executor for a console app
    /// </summary>
    public class LuaXConsoleExecutor : LuaXProjectExecutorBase
    {
        public LuaXConsoleExecutor(LuaXProject project) : base(project)
        {
            Initialize(project);
        }

        override protected int RunBody(string[] args)
        {
            var luaArgs = new LuaXVariableInstanceArray(LuaXTypeDefinition.String.ArrayOf(), args.Length);
            for (int i = 0; i < args.Length; i++)
                luaArgs[i].Value = args[i];

            if (!TypesLibrary.SearchClass("Program", out var program))
                throw new LuaXAstGeneratorException(new LuaXElementLocation(Project.Name, 1, 1), "The project has not class named Program");

            if (!program.SearchMethod("Main", null, out var main))
                throw new LuaXAstGeneratorException(program.LuaType.Location, "The class Program does not have method Main");

            if (!main.ReturnType.IsInteger() && !main.ReturnType.IsVoid())
                throw new LuaXAstGeneratorException(main.Location, "The main method should have either an int or a void return type");

            if (!main.Static)
                throw new LuaXAstGeneratorException(main.Location, "The main method should be a static method");

            if (!(main.Arguments.Count == 0 || (main.Arguments.Count == 1 && main.Arguments[0].LuaType.IsArrayOf(LuaXType.String))))
                throw new LuaXAstGeneratorException(main.Location, "The main method should have either have no parameter or have a string[] parameter");

            var rc = LuaXMethodExecutor.Execute(main, TypesLibrary, null, main.Arguments.Count == 0 ? Array.Empty<object>() : new object[] { luaArgs }, out var r);
            if (rc == LuaXMethodExecutor.ResultType.ReachForEnd)
                return 0;
            else if (r is int i)
                return i;
            else
                return 0;
        }
    }
}
