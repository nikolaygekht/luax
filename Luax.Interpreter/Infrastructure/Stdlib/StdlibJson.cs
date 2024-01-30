using System;
using Luax.Parser.Ast;
using Newtonsoft.Json.Linq;

#pragma warning disable S125                // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibJson
    {
        private static LuaXClassInstance mNodeClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("jsonNode", out mNodeClass);
            mNodeClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__node", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern parse(json : string) : jsonNode;
        [LuaXExternMethod("jsonParser", "parse")]
        public static object ParserParse(LuaXObjectInstance _, string json)
        {
            JContainer obj;
            if (json.TrimStart().StartsWith("["))
                obj = JArray.Parse(json);
            else
                obj = JObject.Parse(json);

            LuaXObjectInstance node = mNodeClass.New(mTypeLibrary);
            node.Properties["__node"].Value = obj;
            return node;
        }

        //public extern jsonParser() : void;
        [LuaXExternMethod("jsonParser", "jsonParser")]
        public static object ParserConstructor(LuaXObjectInstance _)
        {
            return null;
        }

        //public extern getType() : string;
        [LuaXExternMethod("jsonNode", "getType")]
        public static object NodeType(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JToken node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));
            return node.Type switch
            {
                JTokenType.Object => "object",
                JTokenType.Array => "array",
                JTokenType.Property => "property",
                JTokenType.Integer => "int",
                JTokenType.Float => "real",
                JTokenType.String => "string",
                JTokenType.Boolean => "boolean",
                JTokenType.Null => "nil",
                _ => "unknown"
            };
        }

        //public extern getName() : string;
        [LuaXExternMethod("jsonNode", "getName")]
        public static object NodeName(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JToken node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            if (node is not JProperty property)
                throw new ArgumentException("The node isn't a property", nameof(@this));
            return property.Name;
        }

        //public extern getValueAsNode() : jsonNode;
        [LuaXExternMethod("jsonNode", "getValueAsNode")]
        public static object NodeValue(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JToken node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            if (node is not JProperty property)
                throw new ArgumentException("The node isn't a property", nameof(@this));

            var rv = mNodeClass.New(mTypeLibrary);
            JToken token = property.Value;
            if (token == null)
                token = JValue.CreateNull();
            rv.Properties["__node"].Value = token;
            return rv;
        }

        //public extern getValueAsString() : string;
        [LuaXExternMethod("jsonNode", "getValueAsString")]
        public static object NodeValueAsString(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JToken node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            if (node.Type != JTokenType.String)
                throw new ArgumentException("The node isn't a string", nameof(@this));
            return node.Value<string>();
        }

        //public extern getValueAsInt() : int;
        [LuaXExternMethod("jsonNode", "getValueAsInt")]
        public static object NodeValueAsInt(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JToken node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            if (node.Type != JTokenType.Integer)
                throw new ArgumentException("The node isn't an integer", nameof(@this));
            return node.Value<int>();
        }

        //public extern getValueAsDatetime() : datetime;
        [LuaXExternMethod("jsonNode", "getValueAsDatetime")]
        public static object NodeValueAsDatetime(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JToken node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            if (node.Type != JTokenType.Integer)
                throw new ArgumentException("The node isn't an integer", nameof(@this));
            long unixTimeStamp =  node.Value<long>();
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        //public extern getValueAsIntegerString() : string;
        [LuaXExternMethod("jsonNode", "getValueAsIntegerString")]
        public static object NodeValueAsIntegerString(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JToken node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            if (node.Type != JTokenType.Integer)
                throw new ArgumentException("The node isn't an integer", nameof(@this));
            return node.Value<string>();
        }

        //public extern getValueAsBoolean() : boolean;
        [LuaXExternMethod("jsonNode", "getValueAsBoolean")]
        public static object NodeValueAsBoolean(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JToken node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            if (node.Type != JTokenType.Boolean)
                throw new ArgumentException("The node isn't a boolean", nameof(@this));
            return node.Value<bool>();
        }

        //public extern getValueAsReal() : real;
        [LuaXExternMethod("jsonNode", "getValueAsReal")]
        public static object NodeValueAsReal(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JToken node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            if (node.Type != JTokenType.Float)
                throw new ArgumentException("The node isn't a real", nameof(@this));
            return node.Value<double>();
        }

        //public extern getChildrenCount() : int;
        [LuaXExternMethod("jsonNode", "getChildrenCount")]
        public static object NodeChildrenCount(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not JContainer node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            return node.Count;
        }

        //public extern getChildByIndex(index : int) : jsonNode;
        [LuaXExternMethod("jsonNode", "getChildByIndex")]
        public static object NodeGetChildByIndex(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__node"].Value is not JContainer node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            if (index >= node.Count)
                throw new IndexOutOfRangeException($"Index {index} exceeds array size {node.Count}: {nameof(@this)}");

            var rv = mNodeClass.New(mTypeLibrary);
            JToken token;
            if (node is JArray)
                token = node[index];
            else
            {
                int i = 0;
                token = node.First;
                while(i < index)
                {
                    i++;
                    token = token.Next;
                }
            }
            if (token == null)
                token = JValue.CreateNull();
            rv.Properties["__node"].Value = token;
            return rv;
        }

        //public extern getPropertyByName(propertyName : string) : jsonNode;
        [LuaXExternMethod("jsonNode", "getPropertyByName")]
        public static object NodeGetChildByPropertyName(LuaXObjectInstance @this, string propertyName)
        {
            if (@this.Properties["__node"].Value is not JContainer node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            var rv = mNodeClass.New(mTypeLibrary);
            var val = node[propertyName];
            if (val == null)
                val = JValue.CreateNull();
            rv.Properties["__node"].Value = val;
            return rv;
        }
    }
}
