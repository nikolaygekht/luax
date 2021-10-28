using System.Runtime.CompilerServices;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// The collection of methods
    /// </summary>
    public class LuaXMethodCollection : LuaXAstCollection<LuaXMethod>
    {
        /// <summary>
        /// Checks whether the method with the name specified already exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name) => Find(name) >= 0;

        /// <summary>
        /// Find the method by its name
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
        /// Finds the method object by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Search(string name, out LuaXMethod method)
        {
            var index = Find(name);
            if (index < 0)
            {
                method = null;
                return false;
            }
            method = this[index];
            return true;
        }
    }
}
