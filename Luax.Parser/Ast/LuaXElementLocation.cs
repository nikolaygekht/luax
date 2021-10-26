using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luax.Parser.Ast
{
    public class LuaXElementLocation
    {
        public string Source { get; }
        public int Line { get; }
        public int Column { get; }

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
    }
}
