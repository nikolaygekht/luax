using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Luax.Parser;
using Luax.Parser.Ast;

namespace LuaX.Doc
{
    public sealed class DocumentationWriter : IDisposable
    {
        private readonly LuaXClassCollection mClasses;
        private readonly StreamWriter mWriter;

        public DocumentationWriter(LuaXClassCollection application, StreamWriter writer)
        {
            mClasses = application;
            mWriter = writer;
        }

        public void Dispose() => mWriter.Dispose();

        public void Write()
        {
            for (int i = 0; i < mClasses.Count; i++)
                WriteClass(mClasses[i]);
        }

        private void WriteClass(LuaXClass @class)
        {
            var doc = new SourceDocumentation(@class);
            if (doc.Ignore)
                return;

            mWriter.WriteLine();
            mWriter.WriteLine("@class");
            mWriter.WriteLine("    @name={0}", @class.Name);
            mWriter.WriteLine("    @brief={0}", doc.Brief ?? "");
            mWriter.WriteLine("    @ingroup=");
            mWriter.WriteLine("    @type=class");
            mWriter.WriteLine("    @parent={0}", @class.Parent);

            WriteDescription(doc.Description, "    ");

            for (int i = 0; i < @class.Constants.Count; i++)
                WriteConstant(@class.Constants[i]);
            for (int i = 0; i < @class.Properties.Count; i++)
                WriteProperty(@class.Properties[i]);
            for (int i = 0; i < @class.Methods.Count; i++)
                WriteMethod(@class.Methods[i]);

            mWriter.WriteLine("@end");
        }

        private void WriteMethod(LuaXMethod @method)
        {
            var doc = new SourceDocumentation(@method);
            
            if (doc.Ignore)
                return;

            mWriter.WriteLine();
            mWriter.WriteLine("    @member");
            mWriter.WriteLine("        @name={0}", @method.Name);
            mWriter.WriteLine("        @type=method");
            mWriter.WriteLine("        @scope={0}", @method.Static ? "class" : "instance");
            mWriter.WriteLine("        @brief={0}", doc.Brief ?? "");
            mWriter.WriteLine("        @visibility={0}", Visibility(@method.Visibility));
            mWriter.WriteLine("        @divisor=.");

            WriteDescription(doc.Description, "        ");

            if (!string.IsNullOrEmpty(doc.Return))
            {
                mWriter.WriteLine();
                mWriter.WriteLine("        @return");
                mWriter.WriteLine("            {0}", doc.Return.Trim());
                mWriter.WriteLine("        @end");
            }

            mWriter.WriteLine();
            mWriter.WriteLine("        @declaration");
            mWriter.WriteLine("            @language=luax");
            mWriter.WriteLine("            @return={0}", @method.ReturnType.ToString());
            mWriter.WriteLine("            @prefix={0}", Prefix(@method.Visibility, @method.Static));
            mWriter.WriteLine("            @params={0}", Params(method.Arguments));
            mWriter.WriteLine("        @end");

            for (int i = 0; i < method.Arguments.Count; i++)
            {
                var paramDoc = doc.Params.Find(p => p.Name == method.Arguments[i].Name);
                mWriter.WriteLine();
                mWriter.WriteLine("        @param");
                mWriter.WriteLine("            @name={0}", method.Arguments[i].Name);
                if (paramDoc != null && !string.IsNullOrEmpty(paramDoc.Description))
                    mWriter.WriteLine("            {0}", paramDoc.Description.Trim());
                mWriter.WriteLine("        @end");
            }
            mWriter.WriteLine("    @end");
        }

        private void WriteProperty(LuaXProperty @property)
        {
            var doc = new SourceDocumentation(@property);
            if (doc.Ignore)
                return;

            mWriter.WriteLine();
            mWriter.WriteLine("    @member");
            mWriter.WriteLine("        @name={0}", property.Name);
            mWriter.WriteLine("        @type=property");
            mWriter.WriteLine("        @scope={0}", property.Static ? "class" : "instance");
            mWriter.WriteLine("        @brief={0}", doc.Brief ?? "");
            mWriter.WriteLine("        @visibility={0}", Visibility(property.Visibility));
            mWriter.WriteLine("        @divisor=.");
            mWriter.WriteLine("        @declaration");
            mWriter.WriteLine("            @language=luax");
            mWriter.WriteLine("            @return={0}", property.LuaType.ToString());
            mWriter.WriteLine("            @prefix={0}", Prefix(property.Visibility, property.Static));
            mWriter.WriteLine("        @end");

            WriteDescription(doc.Description, "        ");

            mWriter.WriteLine("    @end");
        }

        private static string Visibility(LuaXVisibility visibility)
        {
            return visibility switch
            {
                LuaXVisibility.Internal => "public",
                LuaXVisibility.Public => "public",
                LuaXVisibility.Private => "protected",
                _ => throw new ArgumentException("Unknown visibility", nameof(visibility))
            };
        }

        private static string Prefix(LuaXVisibility visibility, bool @static)
        {
            var sb = new StringBuilder();
            sb.Append(Visibility(visibility));
            if (@static)
                sb.Append(" static");
            return sb.ToString();
        }

        private static string Params(LuaXVariableCollection args)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < args.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(args[i].Name).Append(" : ").Append(args[i].LuaType.ToString());
            }
            return sb.ToString();
        }

        private void WriteConstant(LuaXConstantVariable @constant)
        {
            var doc = new SourceDocumentation(@constant);
            if (doc.Ignore)
                return;

            mWriter.WriteLine();
            mWriter.WriteLine("    @member");
            mWriter.WriteLine("        @name={0}", constant.Name);
            mWriter.WriteLine("        @type=property");
            mWriter.WriteLine("        @scope=class");
            mWriter.WriteLine("        @brief={0}", doc.Brief ?? "");
            mWriter.WriteLine("        @visibility=public");
            mWriter.WriteLine("        @divisor=.");

            WriteDescription(doc.Description, "        ");

            mWriter.WriteLine("        Declaration:");
            mWriter.WriteLine("        ```luax");
            mWriter.WriteLine("        const {0} = {1};", constant.Name, constant.Value.Value);
            mWriter.WriteLine("        ```");

            mWriter.WriteLine("    @end");
        }

        private void WriteDescription(List<string> description, string offset)
        {
            mWriter.WriteLine();
            for (int i = 0; i < description.Count; i++)
                mWriter.WriteLine("{0}", offset + (description[i] ?? "").Trim());
            mWriter.WriteLine();
        }
    }
}



