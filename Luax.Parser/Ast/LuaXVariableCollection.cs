namespace Luax.Parser.Ast
{
    /// <summary>
    /// Collection of variables
    /// </summary>
    public class LuaXVariableCollection : LuaXAstCollection<LuaXVariable>
    {
        /// <summary>
        /// Checks whether the variable with the name specified already exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name) => Find(name) >= 0;

        /// <summary>
        /// Find the variable by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int Find(string name)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].Name == name)
                    return i;
            return -1;
        }

        /// <summary>
        /// Searches for the variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool Search(string name, out LuaXVariable v)
        {
            var ix = Find(name);
            if (ix < 0)
            {
                v = null;
                return false;
            }
            v = this[ix];
            return true;
        }
    }
}
