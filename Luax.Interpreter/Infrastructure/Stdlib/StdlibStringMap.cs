using System;
using System.Collections.Generic;
using System.Linq;
using Luax.Parser.Ast;

#pragma warning disable S125                // Sections of code should not be commented out
#pragma warning disable IDE1006             // Naming rule violation.

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibStringMap
    {
        private static LuaXClassInstance mStringMapClass;
        private static LuaXTypesLibrary mTypeLibrary;
        private const string PropertyName = "__map";

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("stringMap", out mStringMapClass);
            mStringMapClass.LuaType.Properties.Add(new LuaXProperty() { Name = PropertyName, Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern length() : int;
        [LuaXExternMethod("stringMap", "length")]
        public static object MapLength(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties[PropertyName].Value is Dictionary<string, LuaXObjectInstance> d))
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            return d.Count;
        }
        
        //public extern contains(key : string) : boolean;
        [LuaXExternMethod("stringMap", "contains")]
        public static object MapContains(LuaXObjectInstance @this, string key)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties[PropertyName].Value is Dictionary<string, LuaXObjectInstance> d))
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            return d.ContainsKey(key);
        }
        
        //public extern get(key : string) : object;
        [LuaXExternMethod("stringMap", "get")]
        public static object MapGet(LuaXObjectInstance @this, string key)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties[PropertyName].Value is Dictionary<string, LuaXObjectInstance> d))
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            return d[key];
        }
        
        //public extern set(key : string, value : object) : void;
        [LuaXExternMethod("stringMap", "set")]
        public static object MapSet(LuaXObjectInstance @this, string key, LuaXObjectInstance v)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties[PropertyName].Value is Dictionary<string, LuaXObjectInstance> d))
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            d.Add(key, v);
            return null;
        }

        //public extern remove(key : string) : void;
        [LuaXExternMethod("stringMap", "remove")]
        public static object MapRemove(LuaXObjectInstance @this, string key)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties[PropertyName].Value is Dictionary<string, LuaXObjectInstance> d))
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            d.Remove(key);
            return null;
        }
        
        //public extern keys() : string[];
        [LuaXExternMethod("stringMap", "keys")]
        public static object MapKeys(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties[PropertyName].Value is Dictionary<string, LuaXObjectInstance> d))
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            return d.Keys.ToArray();
        }


        //public extern clear() : void;
        [LuaXExternMethod("stringMap", "clear")]
        public static object MapClear(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties[PropertyName].Value is Dictionary<string, LuaXObjectInstance> d))
                throw new ArgumentException("The map isn't properly initialized", nameof(@this));
            d.Clear();
            return null;
        }

        //public extern list() : void;
        [LuaXExternMethod("stringMap", "stringMap")]
        public static object MapMap(LuaXObjectInstance @this)
        {
            @this.Properties[PropertyName].Value = new Dictionary<string, LuaXObjectInstance>();
            return null;
        }

    }
}
