namespace Luax.Parser.Ast.Extensions
{
    public static class LuaXMethodExtensions
    {
        public static LuaXVariable FindVariableByName(this LuaXMethod method, string variableName)
        {
            if (method.Variables == null || method.Variables.Count == 0)
                return null;

            foreach (var variable in method.Variables)
            {
                if (variable.Name == variableName)
                    return variable;
            }

            return null;
        }
    }
}
