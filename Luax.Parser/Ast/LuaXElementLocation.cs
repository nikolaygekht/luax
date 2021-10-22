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
            Line = node.Line;
            Column = node.Column;
        }
    }
}
