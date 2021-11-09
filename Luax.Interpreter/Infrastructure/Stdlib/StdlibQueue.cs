using System;
using System.Collections.Generic;
using Luax.Parser.Ast;

#pragma warning disable S125                // Sections of code should not be commented out
#pragma warning disable IDE1006             // Naming rule violation.

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibQueue
    {
        private static LuaXClassInstance mQueueClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("queue", out mQueueClass);
            mQueueClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__list", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern length() : int;
        [LuaXExternMethod("queue", "length")]
        public static object Length(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is Queue<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            return l.Count;
        }

        //public extern dequeue() : object;
        [LuaXExternMethod("queue", "dequeue")]
        public static object Dequeue(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is Queue<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            if (l.Count == 0)
                return null;
            return l.Dequeue();
        }

        //public extern enqueue(value : object) : void;
        [LuaXExternMethod("queue", "enqueue")]
        public static object Enqueue(LuaXObjectInstance @this, LuaXObjectInstance value)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is Queue<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            l.Enqueue(value);
            return null;
        }

        //public extern dequeue() : object;
        [LuaXExternMethod("queue", "peek")]
        public static object Peek(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is Queue<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            if (l.Count == 0)
                return null;
            return l.Peek();
        }

        //public extern clear() : void;
        [LuaXExternMethod("queue", "clear")]
        public static object Clear(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is List<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            l.Clear();
            return null;
        }

        //public extern list() : void;
        [LuaXExternMethod("queue", "queue")]
        public static object Constructor(LuaXObjectInstance @this)
        {
            @this.Properties["__list"].Value = new Queue<LuaXObjectInstance>();
            return null;
        }
    }
}
