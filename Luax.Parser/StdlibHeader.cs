using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Luax.Parser.Ast;

namespace Luax.Parser
{
    /// <summary>
    /// Standard library metadata provider
    /// </summary>
    public static class StdlibHeader
    {
        private static string ReadStdlibHeader() => ResourceReader.Read(typeof(StdlibHeader).Assembly, "stdlib.luax");

        /// <summary>
        /// Provides metadata for the standard library.
        /// </summary>
        /// <returns></returns>
        public static  LuaXBody ReadStdlib()
        {
            var header = ReadStdlibHeader();
            var parser = new LuaXAstGenerator();
            return parser.Compile("stdlib.luax", header);
        }
    }
}
