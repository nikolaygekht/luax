using System;
using System.Xml;
using Luax.Parser.Ast;

#pragma warning disable S125                // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibXml
    {
        private static LuaXClassInstance mNodeClass;
        private static LuaXTypesLibrary mTypeLibrary;

        public static void Initialize(LuaXTypesLibrary typeLibrary)
        {
            mTypeLibrary = typeLibrary;
            typeLibrary.SearchClass("xmlNode", out mNodeClass);
            mNodeClass.LuaType.Properties.Add(new LuaXProperty() { Name = "__node", Visibility = LuaXVisibility.Private, LuaType = LuaXTypeDefinition.Void });
        }

        //public extern parse(xml : string) : xmlNode;
        [LuaXExternMethod("xmlParser", "parse")]
        public static object ParserParse(LuaXObjectInstance _, string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var node = mNodeClass.New(mTypeLibrary);
            node.Properties["__node"].Value = doc.DocumentElement;
            return node;
        }

        //public extern xmlParser() : void;
        [LuaXExternMethod("xmlParser", "xmlParser")]
        public static object ParserConstructor(LuaXObjectInstance _)
        {
            return null;
        }

        //public extern getType() : string;
        [LuaXExternMethod("xmlNode", "getType")]
        public static object NodeType(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));
            return node.NodeType switch
            {
                XmlNodeType.Element => "element",
                XmlNodeType.Attribute => "attribute",
                XmlNodeType.Text => "text",
                XmlNodeType.CDATA => "cdata",
                XmlNodeType.Comment => "comment",
                XmlNodeType.ProcessingInstruction => "processing-instructions",
                _ => "unknown"
            };
        }

        //public extern getName() : string;
        [LuaXExternMethod("xmlNode", "getName")]
        public static object NodeName(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            return node.Name ?? "";
        }

        //public extern getLocalName() : string;
        [LuaXExternMethod("xmlNode", "getLocalName")]
        public static object NodeLocalName(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            return node.LocalName ?? "";
        }

        //public extern getNamespaceURI() : string;
        [LuaXExternMethod("xmlNode", "getNamespaceURI")]
        public static object NodeNamespace(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            return node.NamespaceURI ?? "";
        }
        //public extern getValue() : string;
        [LuaXExternMethod("xmlNode", "getValue")]
        public static object NodeValue(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            return node.Value ?? "";
        }

        //public extern getChildrenCount() : int;
        [LuaXExternMethod("xmlNode", "getChildrenCount")]
        public static object NodeChildrenCount(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            return node.ChildNodes.Count;
        }

        //public extern getChild(index : int) : xmlNode;
        [LuaXExternMethod("xmlNode", "getChild")]
        public static object NodeGetChild(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            var rv = mNodeClass.New(mTypeLibrary);
            rv.Properties["__node"].Value = node.ChildNodes[index];
            return rv;
        }

        //public extern getAttributesCount() : int;
        [LuaXExternMethod("xmlNode", "getAttributesCount")]
        public static object NodeAttributesCount(LuaXObjectInstance @this)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            return node.Attributes.Count;
        }

        //public extern getAttribute(index : int) : xmlNode;
        [LuaXExternMethod("xmlNode", "getAttribute")]
        public static object NodeGetAttribute(LuaXObjectInstance @this, int index)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            var rv = mNodeClass.New(mTypeLibrary);
            rv.Properties["__node"].Value = node.Attributes[index];
            return rv;
        }

        //public extern getAttributeByName(name : string) : xmlNode;
        [LuaXExternMethod("xmlNode", "getAttributeByName")]
        public static object NodeGetAttributeByName(LuaXObjectInstance @this, string name)
        {
            if (@this.Properties["__node"].Value is not XmlNode node)
                throw new ArgumentException("The node isn't properly initialized", nameof(@this));

            for (int i = 0; i < node.Attributes.Count; i++)
                if (node.Attributes[i].LocalName == name)
                {
                    var rv = mNodeClass.New(mTypeLibrary);
                    rv.Properties["__node"].Value = node.Attributes[i];
                    return rv;
                }

            return null;
        }
    }
}
