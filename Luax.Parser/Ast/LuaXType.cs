using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// The data type of a LuaX constant or value
    /// </summary>
    public enum LuaXType
    {
        Void,
        Boolean,
        Integer,
        Real,
        String,
        Class,
    }
}
