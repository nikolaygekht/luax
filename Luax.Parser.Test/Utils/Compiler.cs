using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast;

namespace Luax.Parser.Test.Utils
{
    internal static class Compiler
    {
        public static LuaXBody CompileResource(string resourceName, IEnumerable<Tuple<string, string>> substitutions = null)
        {
            var source = ResourceReader.Read(typeof(Compiler).Assembly, resourceName + ".luax");

            if (substitutions != null)
                foreach (var substitution in substitutions)
                    source = source.Replace(substitution.Item1, substitution.Item2);

            var parser = new LuaXAstGenerator();
            return parser.Compile(resourceName, source);
        }
    }
}
