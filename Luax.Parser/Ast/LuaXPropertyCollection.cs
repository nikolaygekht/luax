namespace Luax.Parser.Ast
{
    /// <summary>
    /// The collection of the class properties
    /// </summary>
    public class LuaXPropertyCollection : LuaXAstCollection<LuaXProperty>
    {
        /// <summary>
        /// Checks whether the property with the name specified already exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name) => Find(name) >= 0;

        /// <summary>
        /// Find the property by its name
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
    }
}
