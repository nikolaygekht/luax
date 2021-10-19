using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luax.Parser.Ast
{
    public class LuaXClass
    {
        public string Name { get; internal init; }
        public string Parent { get; internal init; }
        public bool HasParent => !string.IsNullOrEmpty(Parent);

        internal LuaXClass()
        {
        }
    }
}
