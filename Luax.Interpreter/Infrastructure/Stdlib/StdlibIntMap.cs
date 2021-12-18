using System;
using System.Collections.Generic;
using System.Linq;
using Luax.Parser.Ast;

#pragma warning disable S125                // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibIntMap
    {
        private const string PropertyName = "__map";

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            typeLibrary.SearchClass("int_map", out LuaXClassInstance intMapClass);
            intMapClass.LuaType.Properties.Add(new LuaXProperty() { Name = PropertyName, Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern length() : int;
        [LuaXExternMethod("int_map", "length")]
        public static object MapLength(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties[PropertyName].Value is not Dictionary<int, LuaXObjectInstance> d)
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            return d.Count;
        }

        //public extern contains(key : int) : boolean;
        [LuaXExternMethod("int_map", "contains")]
        public static object MapContains(LuaXObjectInstance @this, int key)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties[PropertyName].Value is not Dictionary<int, LuaXObjectInstance> d)
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            return d.ContainsKey(key);
        }

        //public extern get(key : int) : object;
        [LuaXExternMethod("int_map", "get")]
        public static object MapGet(LuaXObjectInstance @this, int key)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties[PropertyName].Value is not Dictionary<int, LuaXObjectInstance> d)
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            return d[key];
        }

        //public extern set(key : int, value : object) : void;
        [LuaXExternMethod("int_map", "set")]
        public static object MapSet(LuaXObjectInstance @this, int key, LuaXObjectInstance v)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties[PropertyName].Value is not Dictionary<int, LuaXObjectInstance> d)
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            d[key] = v;
            return null;
        }

        //public extern remove(key : int) : void;
        [LuaXExternMethod("int_map", "remove")]
        public static object MapRemove(LuaXObjectInstance @this, int key)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties[PropertyName].Value is not Dictionary<int, LuaXObjectInstance> d)
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            d.Remove(key);
            return null;
        }

        //public extern keys() : int[];
        [LuaXExternMethod("int_map", "keys")]
        public static object MapKeys(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties[PropertyName].Value is not Dictionary<int, LuaXObjectInstance> d)
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            var keys = d.Keys;
            var result = new LuaXVariableInstanceArray(LuaXTypeDefinition.String, keys.Count);
            for (var i = 0; i < keys.Count; i++)
                result[i].Value = keys.ElementAt(i);
            return result;
        }


        //public extern clear() : void;
        [LuaXExternMethod("int_map", "clear")]
        public static object MapClear(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (@this.Properties[PropertyName].Value is not Dictionary<int, LuaXObjectInstance> d)
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            d.Clear();
            return null;
        }

        //public extern int_map() : void;
        [LuaXExternMethod("int_map", "int_map")]
        public static object MapMap(LuaXObjectInstance @this)
        {
            @this.Properties[PropertyName].Value = new Dictionary<int, LuaXObjectInstance>();
            return null;
        }
    }
}
