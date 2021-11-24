using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Luax.Parser.Ast;

#pragma warning disable S125                // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibList
    {
        private static LuaXClassInstance mListClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("list", out mListClass);
            mListClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__list", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern length() : int;
        [LuaXExternMethod("list", "length")]
        public static object ListLength(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__list"].Value is not List<LuaXObjectInstance> l)
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            return l.Count;
        }

        //public extern get(index : int) : object;
        [LuaXExternMethod("list", "get")]
        public static object ListGet(LuaXObjectInstance @this, int index)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__list"].Value is not List<LuaXObjectInstance> l)
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            return l[index];
        }

        //public extern set(index : int, value : object) : void;
        [LuaXExternMethod("list", "set")]
        public static object ListSet(LuaXObjectInstance @this, int index, LuaXObjectInstance v)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__list"].Value is not List<LuaXObjectInstance> l)
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            l[index] = v;
            return null;
        }

        //public extern add(value : object) : void;
        [LuaXExternMethod("list", "add")]
        public static object ListAdd(LuaXObjectInstance @this, LuaXObjectInstance v)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__list"].Value is not List<LuaXObjectInstance> l)
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            l.Add(v);
            return null;
        }

        //public extern insert(index : int, value : object) : void;
        [LuaXExternMethod("list", "insert")]
        public static object ListInsert(LuaXObjectInstance @this, int index, LuaXObjectInstance v)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__list"].Value is not List<LuaXObjectInstance> l)
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            l.Insert(index, v);
            return null;
        }

        //public extern remove(index : int) : void;
        [LuaXExternMethod("list", "remove")]
        public static object ListRemove(LuaXObjectInstance @this, int index)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__list"].Value is not List<LuaXObjectInstance> l)
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            l.RemoveAt(index);
            return null;
        }

        //public extern clear() : void;
        [LuaXExternMethod("list", "clear")]
        public static object ListClear(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__list"].Value is not List<LuaXObjectInstance> l)
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            l.Clear();
            return null;
        }

        //public extern toArray() : object[];
        [LuaXExternMethod("list", "toArray")]
        public static object ListToArray(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties["__list"].Value is not List<LuaXObjectInstance> l)
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            mTypeLibrary.SearchClass("object", out var t);
            var r = new LuaXVariableInstanceArray(t.LuaType.TypeOf().ArrayOf(), l.Count);
            for (int i = 0; i < l.Count; i++)
                r[i].Value = l[i];
            return r;
        }

        //public extern list() : void;
        [LuaXExternMethod("list", "list")]
        public static object ListList(LuaXObjectInstance @this)
        {
            @this.Properties["__list"].Value = new List<LuaXObjectInstance>();
            return null;
        }

        //public static extern create(initial : object[]) : list
        [LuaXExternMethod("list", "create")]
        public static object Create(LuaXVariableInstanceArray array)
        {
            var r = mListClass.New(mTypeLibrary);

            if (r.Properties["__list"].Value is not List<LuaXObjectInstance> l)
                throw new InvalidOperationException("The list isn't properly initialized");

            if (array != null)
            {
                if (!array.ElementType.IsObject())
                    throw new ArgumentException("The array must be the array of objects", nameof(array));

                for (int i = 0; i < array.Length; i++)
                    l.Add(array[i].Value as LuaXObjectInstance);
            }
            return r;
        }
    }
}



