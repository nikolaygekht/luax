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
        public LuaXElementLocationCollection Locations { get; } = new LuaXElementLocationCollection();

        public LuaXVariableInstanceSet Properties { get; } = new LuaXVariableInstanceSet();

        public LuaXExecutionException(LuaXElementLocation location, string message) : base(message)
        {
            Locations.Add(location);
        }

        public LuaXExecutionException(LuaXElementLocation location, string message, LuaXVariableInstanceSet properties) : base(message)
        {
            Locations.Add(location);
            Properties = properties;
        }

        public LuaXExecutionException(LuaXElementLocation location, string message, Exception innerException) : base(message, innerException)
        {
            Locations.Add(location);
        }

        protected LuaXExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Locations = (LuaXElementLocationCollection)info.GetValue("locations", typeof(LuaXElementLocationCollection));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("locations", Locations);
        }
    }
}
