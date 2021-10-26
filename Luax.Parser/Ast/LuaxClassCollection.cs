using System;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// A collection of classes
    /// </summary>
    public class LuaXClassCollection : LuaXAstCollection<LuaXClass>
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

        /// <summary>
        /// Finds the class object by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public LuaXClass Search(string name)
        {
            var index = Find(name);
            if (index < 0)
                return null;
            return this[index];
        }
    }
}
