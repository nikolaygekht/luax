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
    public class LuaXElementLocation : ISerializable
    {
        public string Source { get; }
        public int Line { get; }
        public int Column { get; }

        public LuaXElementLocation(string source, int line, int column)
        {
            Source = source;
            Line = line;
            Column = column;
        }

        public LuaXElementLocation(string source, IAstNode node)
        {
            Source = source;
            FindPosition(node, out var line, out var column);
            Line = line;
            Column = column;
        }

        private static bool FindPosition(IAstNode node, out int line, out int column)
        {
            line = column = 0;
            if (node.Line != 0)
            {
                line = node.Line;
                column = node.Column;
                return true;
            }
            for (int i = 0; i < node.Children.Count; i++)
                if (FindPosition(node.Children[i], out line, out column))
                    return true;
            return false;
        }

        protected LuaXElementLocation(SerializationInfo info, StreamingContext context)
        {
            Source = info.GetString("source");
            Line = info.GetInt32("line");
            Column = info.GetInt32("column");
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("source", Source);
            info.AddValue("line", Line);
            info.AddValue("column", Column);
        }
    }
}
