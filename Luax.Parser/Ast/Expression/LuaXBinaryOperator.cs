using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast.Builder;

namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// Binary operator codes
    /// </summary>
    public enum LuaXBinaryOperator
    {
        Add,
        Subtract,
        Concat,
        Multiply,
        Divide,
        Reminder,
        Power,
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreatorOrEqual,
        And,
        Or,
        Not,
    }
}
