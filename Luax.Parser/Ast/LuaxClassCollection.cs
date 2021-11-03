using System;
using System.Collections.Generic;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// A collection of classes
    /// </summary>
    public class LuaXClassCollection : LuaXAstNamedCollection<LuaXClass>
    {
        public bool Exists(string name) => name == "object" || Find(name) >= 0;

        override protected void UpdateIndex()
        {
            base.UpdateIndex();

            for (int i = 0; i < Count; i++)
            {
                if (this[i].HasParent)
                {
                    if (this[i].Parent == "object")
                        this[i].ParentClass = LuaXClass.Object;
                    else if (mIndex.TryGetValue(this[i].Parent, out var parentClassIndex))
                        this[i].ParentClass = this[parentClassIndex];
                }
            }
        }

        /// <summary>
        /// Finds the class object by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        override public bool Search(string name, out LuaXClass @class)
        {
            if (name == "object")
            {
                @class = LuaXClass.Object;
                return true;
            }

            return base.Search(name, out @class);
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
            if (!Search(sourceClass, out var s) || !Search(targetClass, out var t))
                return false;
            return IsKindOf(s, t);
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
        public bool IsKindOf(LuaXClass sourceClass, LuaXClass targetClass)
        {
            if (ReferenceEquals(sourceClass, targetClass))
                return true;
            if (sourceClass.Parent != null)
                return IsKindOf(sourceClass.ParentClass, targetClass);
            return false;
        }
    }
}
