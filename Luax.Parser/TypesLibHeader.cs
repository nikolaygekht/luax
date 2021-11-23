using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Luax.Parser.Ast;

namespace Luax.Parser
{
    /// <summary>
    /// Standard library metadata provider
    /// </summary>
    public static class TypesLibHeader
    {
        private static string ReadTypesLibHeader() => ResourceReader.Read(typeof(TypesLibHeader).Assembly, "typeslib.luax");

        /// <summary>
        /// Provides metadata for the standard library.
        /// </summary>
        /// <returns></returns>
        public static  LuaXBody ReadTypesLib()
        {
            var header = ReadTypesLibHeader();
            var parser = new LuaXAstGenerator();
            return parser.Compile("typeslib.luax", header);
        }
    }
}
