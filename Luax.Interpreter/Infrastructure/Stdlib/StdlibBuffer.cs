﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast;

#pragma warning disable S125 // Sections of code should not be commented out
#pragma warning disable IDE1006 // Naming Styles

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibBuffer
    {
        private static LuaXClassInstance mBufferClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("buffer", out mBufferClass);
            mBufferClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__array", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
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

        //public extern getInt16(index : int) : int;
        [LuaXExternMethod("buffer", "getInt16")]
        public static object getInt16(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte b1 = buffer[index];
            byte b2 = buffer[index + 1];

            return b2 << 8 | b1;
        }

        //public extern setInt16(index : int, value : int) : int;
        [LuaXExternMethod("buffer", "setInt16")]
        public static object setInt16(LuaXObjectInstance @this, int index, int value)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte b1 = (byte)(value & 0xff);
            byte b2 = (byte)((value >> 8) & 0xff);

            buffer[index] = b1;
            buffer[index + 1] = b2;

            return 2;
        }

        //public extern getInt16B(index : int) : int;
        [LuaXExternMethod("buffer", "getInt16B")]
        public static object getInt16B(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte b1 = buffer[index + 1];
            byte b2 = buffer[index];

            return (int)((uint)(b2 << 8) | b1);
        }

        //public extern setInt16B(index : int, value : int) : int;
        [LuaXExternMethod("buffer", "setInt16B")]
        public static object setInt16B(LuaXObjectInstance @this, int index, int value)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte b1 = (byte)(value & 0xff);
            byte b2 = (byte)((value >> 8) & 0xff);

            buffer[index + 1] = b1;
            buffer[index] = b2;

            return 2;
        }

        //public extern getInt32(index : int) : int;
        [LuaXExternMethod("buffer", "getInt32")]
        public static object getInt32(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - 3)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte b1 = buffer[index];
            byte b2 = buffer[index + 1];
            byte b3 = buffer[index + 2];
            byte b4 = buffer[index + 3];

            return (int)((uint)(b4 << 24) | (uint)(b3 << 16) | (uint)(b2 << 8) | b1);
        }

        //public extern setInt32(index : int, value : int) : int;
        [LuaXExternMethod("buffer", "setInt32")]
        public static object setInt32(LuaXObjectInstance @this, int index, int value)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - 3)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte b1 = (byte)(value & 0xff);
            byte b2 = (byte)((value >> 8) & 0xff);
            byte b3 = (byte)((value >> 16) & 0xff);
            byte b4 = (byte)((value >> 24) & 0xff);

            buffer[index] = b1;
            buffer[index + 1] = b2;
            buffer[index + 2] = b3;
            buffer[index + 3] = b4;

            return 4;
        }

        //public extern getInt32B(index : int) : int;
        [LuaXExternMethod("buffer", "getInt32B")]
        public static object getInt32B(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - 3)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte b1 = buffer[index + 3];
            byte b2 = buffer[index + 2];
            byte b3 = buffer[index + 1];
            byte b4 = buffer[index];

            return (int)((uint)(b4 << 24) | (uint)(b3 << 16) | (uint)(b2 << 8) | b1);
        }

        //public extern setInt32B(index : int, value : int) : int;
        [LuaXExternMethod("buffer", "setInt32B")]
        public static object setInt32B(LuaXObjectInstance @this, int index, int value)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - 3)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte b1 = (byte)(value & 0xff);
            byte b2 = (byte)((value >> 8) & 0xff);
            byte b3 = (byte)((value >> 16) & 0xff);
            byte b4 = (byte)((value >> 24) & 0xff);

            buffer[index + 3] = b1;
            buffer[index + 2] = b2;
            buffer[index + 1] = b3;
            buffer[index] = b4;

            return 4;
        }

        //public extern getFloat32(index : int) : real;
        [LuaXExternMethod("buffer", "getFloat32")]
        public static object getFloat32(LuaXObjectInstance @this, int index)
        {
            int v = (int)getInt32(@this, index);
            return BitConverter.Int32BitsToSingle(v);
        }

        //public extern setFloat32(index : int, value : real) : int;
        [LuaXExternMethod("buffer", "setFloat32")]
        public static object setFloat32(LuaXObjectInstance @this, int index, double value)
        {
            return setInt32(@this, index, BitConverter.SingleToInt32Bits((float)value));
        }

        //public extern getFloat64(index : int) : real;
        [LuaXExternMethod("buffer", "getFloat64")]
        public static object getFloat64(LuaXObjectInstance @this, int index)
        {
            int r1 = (int)getInt32(@this, index);
            int r2 = (int)getInt32(@this, index + 4);
            long l = (long)r1 | ((long)r2 << 32);
            return BitConverter.Int64BitsToDouble(l);
        }

        //public extern setFloat64(index : int, value : real) : int;
        [LuaXExternMethod("buffer", "setFloat64")]
        public static object setFloat64(LuaXObjectInstance @this, int index, double value)
        {
            ulong l = (ulong)BitConverter.DoubleToInt64Bits(value);
            var r1 = (uint)(l & 0xffff_fffff);
            var r2 = (uint)((l >> 32) & 0xffff_fffff);
            setInt32(@this, index, (int)r1);
            setInt32(@this, index + 4, (int)r2);
            return 8;
        }

        //public extern getEncodedString(index : int, encodedLength : int, codePage : int) : string;
        [LuaXExternMethod("buffer", "getEncodedString")]
        public static object getEncodedString(LuaXObjectInstance @this, int index, int encodedLength, int codePage)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - encodedLength + 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            var encoding = Encoding.GetEncoding(codePage);
            var l = 0;
            for (int i = 0; i < encodedLength && buffer[i + index] != 0 && i + index < buffer.Length; i++)
                l++;
            return encoding.GetString(buffer, index, l);
        }

        //public extern setEncodedString(index : int, value : string, codePage : int) : int;
        [LuaXExternMethod("buffer", "setEncodedString")]
        public static object setEncodedString(LuaXObjectInstance @this, int index, string value, int codePage)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));

            var encoding = Encoding.GetEncoding(codePage);
            var content = encoding.GetBytes(value);

            if (index < 0 || index >= buffer.Length - content.Length + 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            for (int i = 0; i < content.Length; i++)
                buffer[i + index] = content[i];
            return content.Length;
        }

        //public extern getUnicodeString(index : int, maximumLength : int) : string;
        [LuaXExternMethod("buffer", "getUnicodeString")]
        public static object getUnicodeString(LuaXObjectInstance @this, int index, int maximumLength)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - maximumLength * 2 + 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < maximumLength; i++)
            {
                var b1 = buffer[index + i * 2];
                var b2 = buffer[index + i * 2 + 1];

                if (b1 == 0 && b2 == 0)
                    break;

                sb.Append((char)(b1 | (b2 << 8)));
            }

            return sb.ToString();
        }

        //public extern setUnicodeString(index : int, value : string) : int;
        [LuaXExternMethod("buffer", "setUnicodeString")]
        public static object setUnicodeString(LuaXObjectInstance @this, int index, string value)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));
            if (index < 0 || index >= buffer.Length - value.Length * 2 + 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                byte b1 = (byte)(c & 0xff);
                byte b2 = (byte)((c >> 8) & 0xff);
                buffer[i * 2 + index] = b1;
                buffer[i * 2 + index + 1] = b2;
            }
            return value.Length * 2;
        }

        //public static extern getEncodedStringLength(value : string, codePage : int) : int;
        [LuaXExternMethod("buffer", "getEncodedStringLength")]
        public static object getEncodedStringLength(string value, int codePage)
        {
            var encoding = Encoding.GetEncoding(codePage);
            return encoding.GetByteCount(value);
        }

        //@DocBrief("Resize")
        //public extern resize(newsize : int) : void;
        [LuaXExternMethod("buffer", "resize")]
        public static object resize(LuaXObjectInstance @this, int newsize)
        {
            if (@this.Properties["__array"]?.Value is not byte[] buffer)
                throw new ArgumentException("The object isn't properly initialized", nameof(@this));

            Array.Resize<byte>(ref buffer, newsize);
            @this.Properties["__array"].Value = buffer;
            return null;
        }

        //public static extern create(length : int) : buffer;
        [LuaXExternMethod("buffer", "create")]
        public static object Create(int length)
        {
            var @this = mBufferClass.New(mTypeLibrary);
            @this.Properties["__array"].Value = new byte[length];
            return @this;
        }
    }
}
