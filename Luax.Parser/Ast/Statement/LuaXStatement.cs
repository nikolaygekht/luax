using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast.Builder;

namespace Luax.Parser.Ast.Statement
{
    /// <summary>
    /// The base class for LuaX statements
    /// </summary>
    public abstract class LuaXStatement
    {
        /// <summary>
        /// The location of the statement in the source code
        /// </summary>
        public LuaXElementLocation Location { get; }

        protected LuaXStatement(LuaXElementLocation location)
        {
            Location = location;
        }

        /// <summary>
        /// Execution/conversion-level object associated with the statement
        /// </summary>
        public object Tag { get; set; }
    }
}
