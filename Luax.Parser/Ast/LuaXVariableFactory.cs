namespace Luax.Parser.Ast
{
    /// <summary>
    /// The factory for LuaXVariable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LuaXVariableFactory<T>
        where T : LuaXVariable, new()
    {
        public virtual T Create(string name, LuaXTypeDefinition type, LuaXElementLocation location)
        {
            return new T()
            {
                Name = name,
                LuaType = type,
                Location = location
            };
        }
    }
}
