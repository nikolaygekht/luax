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
            var resourceFullName = $"Luax.Parser.Test.TestSources.{resourceName}.luax";
            string source;
            using (var stream = typeof(Compiler).Assembly.GetManifestResourceStream(resourceFullName))
            {
                if (stream == null)
                    throw new ArgumentException($"The resource {resourceFullName} is not found");
                var l = (int)stream.Length;
                var t = new byte[l];
                stream.Read(t, 0, l);
                if (l > 3 && t[0] == 0xef && t[1] == 0xbb && t[2] == 0xbf)
                    source = Encoding.UTF8.GetString(t, 3, l - 3); 
                else
                    source = Encoding.UTF8.GetString(t);
            }

            if (substitutions != null)
                foreach (var substitution in substitutions)
                    source = source.Replace(substitution.Item1, substitution.Item2);

            var parser = new LuaXAstGenerator();
            return parser.Compile(resourceName, source);
        }
    }
}
