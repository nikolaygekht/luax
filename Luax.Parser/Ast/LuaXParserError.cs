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
            : this(error.Position.Line, error.Position.Column, error.Message)
        {
        }

        internal LuaXParserError(int line, int column, string message)
        {
            Line = line;
            Column = column;
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
