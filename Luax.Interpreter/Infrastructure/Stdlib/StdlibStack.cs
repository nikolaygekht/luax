using System;
using System.Collections.Generic;
using Luax.Parser.Ast;

#pragma warning disable S125                // Sections of code should not be commented out
#pragma warning disable IDE1006             // Naming rule violation.

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibStack
    {
        private static LuaXClassInstance mStackClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("stack", out mStackClass);
            mStackClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__list", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern length() : int;
        [LuaXExternMethod("stack", "length")]
        public static object Length(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is Stack<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            return l.Count;
        }

        //public extern dequeue() : object;
        [LuaXExternMethod("stack", "pop")]
        public static object Pop(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is Stack<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            if (l.Count == 0)
                return null;
            return l.Pop();
        }

        //public extern push(value : object) : void;
        [LuaXExternMethod("stack", "push")]
        public static object Push(LuaXObjectInstance @this, LuaXObjectInstance value)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is Stack<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            l.Push(value);
            return null;
        }

        //public extern dequeue() : object;
        [LuaXExternMethod("stack", "peek")]
        public static object Peek(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is Stack<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            if (l.Count == 0)
                return null;
            return l.Peek();
        }

        //public extern clear() : void;
        [LuaXExternMethod("stack", "clear")]
        public static object Clear(LuaXObjectInstance @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));
            if (!(@this.Properties["__list"].Value is Stack<LuaXObjectInstance> l))
                throw new ArgumentException("The list isn't properly initialized", nameof(@this));
            l.Clear();
            return null;
        }

        //public extern list() : void;
        [LuaXExternMethod("stack", "stack")]
        public static object Constructor(LuaXObjectInstance @this)
        {
            @this.Properties["__list"].Value = new Stack<LuaXObjectInstance>();
            return null;
        }
    }
}
