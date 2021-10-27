using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast.Builder;

namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// LuaX base class for all expressions
    /// </summary>
    public abstract class LuaXExpression
    {
        /// <summary>
        /// The return type of the expression
        /// </summary>
        public LuaXTypeDefinition ReturnType { get; }

        /// <summary>
        /// The location of the element in the source
        /// </summary>
        public LuaXElementLocation Location { get; }

        protected LuaXExpression(LuaXTypeDefinition returnType, LuaXElementLocation location)
        {
            ReturnType = returnType;
            Location = location;
        }

        public LuaXExpression CastTo(LuaXTypeDefinition type)
            => new LuaXCastOperatorExpression(this, type, this.Location);
    }
}
