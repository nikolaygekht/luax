using System;
using System.Collections.Generic;
using Luax.Interpreter.Expression;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibSortedList
    {
        private static LuaXClassInstance mListClass;
        private static LuaXTypesLibrary mTypeLibrary;

        internal class LuaComparer : IComparer<LuaXObjectInstance>
        {
            private readonly LuaXTypesLibrary mTypeLibrary;
            private readonly LuaXObjectInstance mComparer;
            private readonly LuaXMethod mCompareMethod;

            public LuaComparer(LuaXTypesLibrary typeLibrary, LuaXObjectInstance comparer)
            {
                mTypeLibrary = typeLibrary;
                mComparer = comparer;
                if (!mComparer.Class.LuaType.Methods.Search("compare", out var method) ||
                    method.Static || !method.ReturnType.IsInteger() ||
                    method.Arguments.Count != 2)
                    throw new ArgumentException("The comparer object has no method compare", nameof(comparer));
                mCompareMethod = method;
            }

            public int Compare(LuaXObjectInstance x, LuaXObjectInstance y)
            {
                LuaXMethodExecutor.Execute(mCompareMethod, mTypeLibrary, mComparer, new object[] { x, y }, out var r);
                return (int)r;
            }
        }

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("sorted_list", out mListClass);
            mListClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__comparer", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
            mListClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__list", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        [LuaXExternMethod("sorted_list", "length")]
        public static object Length(LuaXObjectInstance @this)
        {
            if (@this.Properties["__list"]?.Value is not SortedList<LuaXObjectInstance, object> list)
                throw new ArgumentException("The list is not properly initialized", nameof(@this));

            return list.Count;
        }

        [LuaXExternMethod("sorted_list", "add")]
        public static object Add(LuaXObjectInstance @this, LuaXObjectInstance item)
        {
            if (@this.Properties["__list"]?.Value is not SortedList<LuaXObjectInstance, object> list)
                throw new ArgumentException("The list is not properly initialized", nameof(@this));
            list.Add(item, null);
            return null;
        }

        [LuaXExternMethod("sorted_list", "get")]
        public static object Get(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__list"]?.Value is not SortedList<LuaXObjectInstance, object> list)
                throw new ArgumentException("The list is not properly initialized", nameof(@this));
            return list.Keys[index];
        }

        [LuaXExternMethod("sorted_list", "remove")]
        public static object Remove(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__list"]?.Value is not SortedList<LuaXObjectInstance, object> list)
                throw new ArgumentException("The list is not properly initialized", nameof(@this));
            list.Remove(list.Keys[index]);
            return null;
        }

        [LuaXExternMethod("sorted_list", "find")]
        public static object Find(LuaXObjectInstance @this, LuaXObjectInstance value)
        {
            if (@this.Properties["__list"]?.Value is not SortedList<LuaXObjectInstance, object> list)
                throw new ArgumentException("The list is not properly initialized", nameof(@this));

            return list.IndexOfKey(value);
        }

        [LuaXExternMethod("sorted_list", "initialize")]
        public static object Create(LuaXObjectInstance @this, LuaXObjectInstance comparer)
        {
            var luaComparer = new LuaComparer(mTypeLibrary, comparer);
            @this.Properties["__list"].Value = new SortedList<LuaXObjectInstance, object>(luaComparer);
            @this.Properties["__comparer"].Value = luaComparer;
            return null;
        }
    }
}



