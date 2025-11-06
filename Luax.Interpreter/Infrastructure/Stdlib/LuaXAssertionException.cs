using System;
using System.Runtime.Serialization;

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    public class LuaXAssertionException : Exception
    {
        public LuaXAssertionException(string message) : base(message)
        {
        }
    }
}
