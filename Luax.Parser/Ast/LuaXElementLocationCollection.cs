using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast.Builder;

namespace Luax.Parser.Ast
{
    [Serializable]
    public class LuaXElementLocationCollection : LuaXAstCollection<LuaXElementLocation>, ISerializable
    {
        public LuaXElementLocationCollection()
        {
        }

        protected LuaXElementLocationCollection(SerializationInfo info, StreamingContext context)
        {
            var count = info.GetInt32("count");
            for (int i = 0; i < count; i++)
                Add((LuaXElementLocation)info.GetValue($"item{i}", typeof(LuaXElementLocation)));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("count", Count);
            for (int i = 0; i < Count; i++)
                info.AddValue($"item{i}", this[i]);
        }
    }
}
