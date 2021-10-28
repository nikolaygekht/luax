using System;
using System.Runtime.Serialization;
using Hime.Redist;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// The information about LuaXParser error
    /// </summary>
    [Serializable]
    public class LuaXParserError : ISerializable
    {
        /// <summary>
        /// The line
        /// </summary>
        public int Line { get; }
        /// <summary>
        /// The column
        /// </summary>
        public int Column { get; }
        /// <summary>
        /// The error message
        /// </summary>
        public string Message { get; }

        internal LuaXParserError(ParseError error)
        {
            Line = error.Position.Line;
            Column = error.Position.Column;
            Message = error.Message;
        }

        internal LuaXParserError(LuaXElementLocation location, string message)
        {
            Line = location.Line;
            Column = location.Column;
            Message = message;
        }

        internal LuaXParserError(IAstNode node, string message)
        {
            LuaXElementLocation location = new LuaXElementLocation("", node);
            Line = location.Line;
            Column = location.Column;
            Message = message;
        }

        protected LuaXParserError(SerializationInfo info, StreamingContext context)
        {
            Line = info.GetInt32("line");
            Column = info.GetInt32("column");
            Message = info.GetString("message");
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            => GetObjectData(info, context);

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("line", Line);
            info.AddValue("column", Column);
            info.AddValue("message", Message);
        }
    }
}
