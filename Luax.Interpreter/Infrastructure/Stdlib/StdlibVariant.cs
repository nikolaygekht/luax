using System;
using Luax.Parser.Ast;

#pragma warning disable S125                // Sections of code should not be commented out
#pragma warning disable IDE1006             // Naming rule violation.

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibVariant
    {
        private static LuaXClassInstance mVariant;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            mTypeLibrary.SearchClass("variant", out mVariant);
            mVariant.LuaType.Properties.Add(new LuaXProperty() { Name = "__data", LuaType = LuaXTypeDefinition.Void, Visibility = LuaXVisibility.Private });
        }

        private static LuaXObjectInstance CreateVariant(object value)
        {
            var r = mVariant.New(mTypeLibrary);
            r.Properties["__data"].Value = value;
            return r;
        }

        //public static extern fromInt(v : int) : variant;
        [LuaXExternMethod("variantCast", "fromInt")]
        public static object variant_fromInt(int v) => CreateVariant(v);

        //public static extern fromReal(v : real) : variant;
        [LuaXExternMethod("variantCast", "fromReal")]
        public static object variant_fromReal(double v) => CreateVariant(v);

        //public static extern fromDatetime(v : datetime) : variant;
        [LuaXExternMethod("variantCast", "fromDatetime")]
        public static object variant_fromDatetime(DateTime v) => CreateVariant(v);

        //public static extern fromBoolean(v : boolean) : variant;
        [LuaXExternMethod("variantCast", "fromBoolean")]
        public static object variant_fromBoolean(bool v) => CreateVariant(v);

        //public static extern fromString(v : string) : variant;
        [LuaXExternMethod("variantCast", "fromString")]
        public static object variant_fromString(string v) => CreateVariant(v);

        //public static extern fromObject(v : object) : variant;
        [LuaXExternMethod("variantCast", "fromObject")]
        public static object variant_fromObject(LuaXObjectInstance v) => CreateVariant(v);

        //public static extern castToInt(v : variant) : int;
        [LuaXExternMethod("variantCast", "castToInt")]
        public static object variant_castToInt(LuaXObjectInstance v) => variant_asInt(v);

        //public static extern castToReal(v : variant) : real;
        [LuaXExternMethod("variantCast", "castToReal")]
        public static object variant_castToReal(LuaXObjectInstance v) => variant_asReal(v);

        //public static extern castToDatetime(v : variant) : datetime;
        [LuaXExternMethod("variantCast", "castToDatetime")]
        public static object variant_castToDatetime(LuaXObjectInstance v) => variant_asDatetime(v);

        //public static extern castToBoolean(v : variant) : boolean;
        [LuaXExternMethod("variantCast", "castToBoolean")]
        public static object variant_castToBoolean(LuaXObjectInstance v) => variant_asBoolean(v);

        //public static extern castToString(v : variant) : string;
        [LuaXExternMethod("variantCast", "castToString")]
        public static object variant_castToString(LuaXObjectInstance v) => variant_asString(v);

        //public static extern castToObject(v : variant) : object;
        [LuaXExternMethod("variantCast", "castToObject")]
        public static object variant_castToObject(LuaXObjectInstance v) => variant_asObject(v);

        //public extern type() : string;
        [LuaXExternMethod("variant", "type")]
        public static object variant_type(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            if (v == null)
                return "object";
            if (v is int)
                return "int";
            if (v is double)
                return "real";
            if (v is string)
                return "string";
            if (v is DateTime)
                return "datetime";
            if (v is bool)
                return "boolean";
            if (v is LuaXObjectInstance obj)
                return obj.Class.LuaType.Name;
            return "native";
        }

        //public extern asInt() : int;
        [LuaXExternMethod("variant", "asInt")]
        public static object variant_asInt(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            mTypeLibrary.CastTo(LuaXTypeDefinition.Integer, ref v);
            return v;
        }

        //public extern asReal() : real;
        [LuaXExternMethod("variant", "asReal")]
        public static object variant_asReal(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            mTypeLibrary.CastTo(LuaXTypeDefinition.Real, ref v);
            return v;
        }

        //public extern asDatetime() : datetime;
        [LuaXExternMethod("variant", "asDatetime")]
        public static object variant_asDatetime(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            mTypeLibrary.CastTo(LuaXTypeDefinition.Datetime, ref v);
            return v;
        }

        //public extern asBoolean() : boolean;
        [LuaXExternMethod("variant", "asBoolean")]
        public static object variant_asBoolean(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            mTypeLibrary.CastTo(LuaXTypeDefinition.Boolean, ref v);
            return v;
        }

        //public extern asString() : string;
        [LuaXExternMethod("variant", "asString")]
        public static object variant_asString(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            mTypeLibrary.CastTo(LuaXTypeDefinition.String, ref v);
            return v;
        }

        //public extern asObject() : object;
        [LuaXExternMethod("variant", "asObject")]
        public static object variant_asObject(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            if (v == null)
                return null;
            if (v is LuaXObjectInstance obj)
                return obj;
            return @this;
        }

        //public extern isInt() : boolean;
        [LuaXExternMethod("variant", "isInt")]
        public static object variant_isInt(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            return v is int;
        }

        //public extern isReal() : boolean;
        [LuaXExternMethod("variant", "isReal")]
        public static object variant_isReal(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            return v is double;
        }

        //public extern isDatetime() : boolean;
        [LuaXExternMethod("variant", "isDatetime")]
        public static object variant_isDatetime(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            return v is DateTime;
        }

        //public extern isBoolean() : boolean;
        [LuaXExternMethod("variant", "isBoolean")]
        public static object variant_isBoolean(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            return v is bool;
        }

        //public extern isString() : boolean;
        [LuaXExternMethod("variant", "isString")]
        public static object variant_isString(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            return v is string;
        }

        //public extern isObject() : boolean;
        [LuaXExternMethod("variant", "isObject")]
        public static object variant_isObject(LuaXObjectInstance @this)
        {
            var v = @this.Properties["__data"].Value;
            return v == null || v is LuaXObjectInstance;
        }
    }
}
