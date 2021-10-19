//Attention:
//if you open this file in Visual Studio, the following errors will be displayed:
//1) No Luax.Parser.Hime namespace
//2) Classes LuaXLexer & LuaXParser could not be found
//Relax. These files are auto-generated and will be created during the build.
//Just close this file in visual studio and build the project.
using System.IO;
using Hime.Redist.Parsers;
using Luax.Parser.Hime;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// The parser of LuaX source into AST tree.
    /// </summary>
    public class LuaXAstGenerator
    {
        private LuaXBody Compile(string name, LuaXLexer lexer)
        {
            RNGLRParser parser = new LuaXParser(lexer);
            var r = parser.Parse();
            if (!r.IsSuccess)
                throw new LuaXAstGeneratorException(name, r.Errors);
            return LuaXAstTreeCreator.Create(r.Root);
        }

        /// <summary>
        /// Gets the source as a string and parses it
        /// </summary>
        /// <param name="name">The name of the source</param>
        /// <param name="source">The source code</param>
        /// <returns></returns>
        public LuaXBody Compile(string name, string source) => Compile(name, new LuaXLexer(source));

        /// <summary>
        /// Gets the source as a text reader and parses it
        /// </summary>
        /// <param name="name">The name of the source</param>
        /// <param name="source">The source code</param>
        /// <returns></returns>
        public LuaXBody Compile(string name, TextReader source) => Compile(name, new LuaXLexer(source));
    }
}
