using System.Collections.Generic;
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
            if (mIndex.Count != Count)
                UpdateIndex();

            if (mIndex.TryGetValue(name, out var index))
                return index;
            return -1;
        }

        private readonly Dictionary<string, int> mIndex = new Dictionary<string, int>();

        private void UpdateIndex()
        {
            mIndex.Clear();

            for (int i = 0; i < Count; i++)
            {
                mIndex[this[i].Name] = i;
            }
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
