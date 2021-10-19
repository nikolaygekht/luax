using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization;
using Hime.Redist;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// Collection of the parser errors.
    ///
    /// The parser error is a Hime object.
    /// </summary>
    [Serializable]
    public class LuaXAstGeneratorErrorCollection : LuaxAstCollection<LuaXParserError>, ISerializable
    {
        internal LuaXAstGeneratorErrorCollection()
        {
        }

        protected LuaXAstGeneratorErrorCollection(SerializationInfo info, StreamingContext context)
        {
            int count = info.GetInt32("count");
            for (int i = 0; i < count; i++)
                Add((LuaXParserError)info.GetValue($"item{i}", typeof(LuaXParserError)));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
               => GetObjectData(info, context);

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("count", this.Count);
            for (int i = 0; i < Count; i++)
                info.AddValue($"item{i}", this[i]);
        }

        internal void AddRange(IEnumerable<ParseError> errors)
        {
            foreach (var error in errors)
                Add(new LuaXParserError(error));
        }
    }
}
