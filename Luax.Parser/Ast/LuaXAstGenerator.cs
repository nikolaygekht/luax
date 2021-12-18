//NOTE: the classes LuaXLexer and LuaXParser are generated at run-time
//we load them dynamically to avoid error caused by static analyzers that
//cannot recognize auto-generated code.

using System;
using System.IO;
using Hime.Redist.Lexer;
using Hime.Redist.Parsers;
using Luax.Parser.Ast.Builder;

#pragma warning disable CA1822 

namespace Luax.Parser.Ast
{
    /// <summary>
    /// The parser of LuaX source into AST tree.
    /// </summary>
    public class LuaXAstGenerator
    {
        private static Type mLexerType, mParserType;

        private static ContextFreeLexer CreateLexer(object source)
        {
            if (mLexerType == null)
            {
                var assembly = typeof(LuaXAstGenerator).Assembly;
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name == "LuaXLexer" && type.Namespace == "Luax.Parser.Hime")
                    {
                        mLexerType = type;
                        break;
                    }
                }
            }

            if (mLexerType == null)
                throw new InvalidOperationException("Type Luax.Parser.Hime.LuaXLexer is not found");

            return (ContextFreeLexer)Activator.CreateInstance(mLexerType, new object[] { source });
        }

        private static RNGLRParser CreateParser(ContextFreeLexer lexer)
        {
            if (mParserType == null)
            {
                var assembly = typeof(LuaXAstGenerator).Assembly;
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name == "LuaXParser" && type.Namespace == "Luax.Parser.Hime")
                    {
                        mParserType = type;
                        break;
                    }
                }
            }

            if (mParserType == null)
                throw new InvalidOperationException("Type Luax.Parser.Hime.LuaXParser is not found");

            return (RNGLRParser)Activator.CreateInstance(mParserType, new object[] { lexer });
        }

        private LuaXBody Compile(string name, ContextFreeLexer lexer)
        {
            var parser = CreateParser(lexer);
            var r = parser.Parse();
            if (!r.IsSuccess)
                throw new LuaXAstGeneratorException(name, r.Errors);
            var creator = new LuaXAstTreeCreator(name);
            return creator.Create(r.Root);
        }

        /// <summary>
        /// Gets the source as a string and parses it
        /// </summary>
        /// <param name="name">The name of the source</param>
        /// <param name="source">The source code</param>
        /// <returns></returns>
        public LuaXBody Compile(string name, string source) => Compile(name, CreateLexer(source));

        /// <summary>
        /// Gets the source as a text reader and parses it
        /// </summary>
        /// <param name="name">The name of the source</param>
        /// <param name="source">The source code</param>
        /// <returns></returns>
        public LuaXBody Compile(string name, TextReader source) => Compile(name, CreateLexer(source));
    }
}
