using System;
using Luax.Interpreter.Infrastructure;
using Luax.Parser.Ast;
using Luax.Parser.Ast.Statement;

namespace Luax.Interpreter.Expression
{
    public class BeforeStatementExecutionEventArgs : EventArgs
    {
        /// <summary>
        /// Method executed
        /// </summary>
        public LuaXMethod Method { get; internal init; }

        /// <summary>
        /// The type library used for method execution
        /// </summary>
        public LuaXTypesLibrary TypesLibrary { get; internal init; }

        /// <summary>
        /// The local variables
        /// </summary>
        public LuaXVariableInstanceSet LocalVariables { get; internal init; }

        /// <summary>
        /// The statement to be executed
        /// </summary>
        public LuaXStatement Statement { get; internal init; }
    }
}
