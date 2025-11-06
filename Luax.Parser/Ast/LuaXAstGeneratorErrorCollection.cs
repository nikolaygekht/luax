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
    public class LuaXAstGeneratorErrorCollection : LuaXAstCollection<LuaXParserError> 
    {
        internal LuaXAstGeneratorErrorCollection()
        {
        }

        internal void AddRange(IEnumerable<ParseError> errors)
        {
            foreach (var error in errors)
                Add(new LuaXParserError(error));
        }
    }
}
