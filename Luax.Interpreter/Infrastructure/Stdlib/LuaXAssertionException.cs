using System;
using System.Runtime.Serialization;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    [Serializable]
    public class LuaXAssertionException : Exception
    {
        public LuaXAssertionException(string message) : base(message)
        {
        }

        protected LuaXAssertionException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}
