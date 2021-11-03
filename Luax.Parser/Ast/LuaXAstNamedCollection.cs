using System.Collections.Generic;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// The base class to collection of named objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LuaXAstNamedCollection<T> : LuaXAstCollection<T>
        where T : class, ILuaXNamedObject
    {
        /// <summary>
        /// Checks whether the variable with the name specified already exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual bool Contains(string name) => Find(name) >= 0;

        /// <summary>
        /// Find the variable by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual int Find(string name)
        {
            if (mIndex.Count != Count)
                UpdateIndex();

            if (mIndex.TryGetValue(name, out var index))
                return index;
            return -1;
        }

        protected readonly Dictionary<string, int> mIndex = new Dictionary<string, int>();

        protected virtual void UpdateIndex()
        {
            mIndex.Clear();

            for (int i = 0; i < Count; i++)
            {
                mIndex[this[i].Name] = i;
            }
        }

        /// <summary>
        /// Searches for the variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public virtual bool Search(string name, out T v)
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
