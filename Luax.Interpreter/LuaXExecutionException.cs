using System;
using System.Runtime.Serialization;
using Luax.Interpreter.Infrastructure;
using Luax.Parser.Ast;

namespace Luax.Interpreter
{
    public class LuaXExecutionException : Exception
    {
        public LuaXStackTrace LuaXStackTrace { get; } = new LuaXStackTrace();
        public LuaXVariableInstanceSet Properties { get; } = new LuaXVariableInstanceSet();

        public LuaXExecutionException(LuaXElementLocation location, string message) : base(message)
        {
            LuaXStackTrace.Add(null, location);
        }

        public LuaXExecutionException(LuaXMethod callSite, LuaXElementLocation location, string message, LuaXVariableInstanceSet properties) : base(message)
        {
            LuaXStackTrace.Add(callSite, location);
            Properties = properties;
        }

        public LuaXExecutionException(LuaXMethod callSite, LuaXElementLocation location, string message, Exception innerException) : base(message, innerException)
        {
            LuaXStackTrace.Add(callSite, location);
        }

        public LuaXExecutionException(LuaXElementLocation location, string message, Exception innerException) : base(message, innerException)
        {
            LuaXStackTrace.Add(null, location);
        }
    }
}
