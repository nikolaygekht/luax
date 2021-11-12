using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast;

#pragma warning disable S125 // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibBuffer
    {
        private static LuaXClassInstance mClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("buffer", out mClass);
            mClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__array", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern length() : int;
        [LuaXExternMethod("buffer", "length")]
        public static object Length(LuaXObjectInstance @this)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            return buffer.Length;
        }
        //public extern get(index : int) : int;
        [LuaXExternMethod("buffer", "get")]
        public static object Get(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ((int)buffer[index]) & 0xff;
        }
        //public extern set(index : int, value : int) : void;
        [LuaXExternMethod("buffer", "set")]
        public static object Set(LuaXObjectInstance @this, int index, int value)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            buffer[index] = (byte)(value & 0xff);
            return null;
        }
        //public static extern create(length : int) : buffer;
        [LuaXExternMethod("buffer", "create")]
        public static object Create(int length)
        {
            var @this = mClass.New(mTypeLibrary);
            @this.Properties["__array"].Value = new byte[length];
            return @this;
        }
    }
}
