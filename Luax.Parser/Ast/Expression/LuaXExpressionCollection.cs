using System.Text;

namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// Collection of expression
    /// </summary>
    public class LuaXExpressionCollection : LuaXAstCollection<LuaXExpression>
    {
        new public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (i != 0)
                    sb.Append(", ");
                sb.Append(this[i].ToString());
            }
            return sb.ToString();
        }
    }
}
