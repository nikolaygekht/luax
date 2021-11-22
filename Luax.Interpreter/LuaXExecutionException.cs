using System;
using System.Runtime.Serialization;
using Luax.Interpreter.Infrastructure;
using Luax.Parser.Ast;

#pragma warning disable S3925 // TBD: "ISerializable" should be implemented correctly

namespace Luax.Interpreter
{
    [Serializable]
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

        protected LuaXExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            LuaXStackTrace = (LuaXStackTrace)info.GetValue("luaStackTrace", typeof(LuaXStackTrace));
            Properties = (LuaXVariableInstanceSet)info.GetValue("properties", typeof(LuaXVariableInstanceSet));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("luaStackTrace", LuaXStackTrace);
            info.AddValue("properties", Properties);
        }
    }
}
