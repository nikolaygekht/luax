using System;
using System.Collections.Generic;

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
        /// Finds the class object by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Search(string name, out LuaXClass @class)
        {
            var index = Find(name);
            if (index < 0)
            {
                @class = null;
                return false;
            }
            @class = this[index];
            return true;
        }

        /// <summary>
        /// The method checks whether an instance of a sourceClass may be assigned
        /// to a targetClass.
        ///
        /// It is possible only if a sourceClass is a targetClass or a targetClass is one
        /// of parents of the sourceClass.
        /// </summary>
        /// <param name="sourceClass"></param>
        /// <param name="targetClass"></param>
        /// <returns></returns>
        public bool IsKindOf(string sourceClass, string targetClass)
        {
            if (sourceClass == targetClass)
                return true;
            if (!Search(sourceClass, out var @class))
                return false;
            if (string.IsNullOrEmpty(@class.Parent))
                return false;
            return IsKindOf(@class.Parent, targetClass);
        }
    }
}
