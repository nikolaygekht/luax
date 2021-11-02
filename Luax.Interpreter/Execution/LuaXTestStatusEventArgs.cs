using System;

namespace Luax.Interpreter.Execution
{
    public class LuaXTestStatusEventArgs : EventArgs
    {
        public string Class { get; }
        public string Method { get; }
        public string Data { get; }

        public LuaXTestStatus Status { get; }
        public string Message { get; }

        public LuaXTestStatusEventArgs(string @class, string method, LuaXTestStatus status, string message)
            : this(@class, method, "", status, message)
        {
        }

        public LuaXTestStatusEventArgs(string @class, string method, string data, LuaXTestStatus status, string message)
        {
            Class = @class;
            Method = method;
            Data = data;
            Status = status;
            Message = message;
        }
    }
}
