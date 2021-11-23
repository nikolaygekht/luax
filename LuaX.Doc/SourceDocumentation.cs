using System.Collections.Generic;
using Luax.Parser.Ast;

namespace LuaX.Doc
{
    internal class SourceDocumentation
    {
        public bool Ignore { get; set; }
        public string Brief { get; set; }
        public List<string> Description { get; } = new List<string>();
        public string Return { get; set; }
        public List<SourceDocumentationParam> Params { get; } = new List<SourceDocumentationParam>();

        public SourceDocumentation(LuaXAttributeCollection attributes)
        {
            foreach (var attribute in attributes)
            {
                if (attribute.Name == "DocHide")
                    Ignore = true;
                else if (attribute.Name == "DocBrief" && attribute.Parameters.Count > 0)
                    Brief = attribute.Parameters[0].Value?.ToString() ?? "";
                else if (attribute.Name == "DocDescription" && attribute.Parameters.Count > 0)
                    Description.Add(attribute.Parameters[0].Value?.ToString() ?? "");
                else if (attribute.Name == "DocReturn" && attribute.Parameters.Count > 0)
                    Return = attribute.Parameters[0].Value?.ToString() ?? "";
                else if (attribute.Name == "DocParameter" && attribute.Parameters.Count > 1)
                    Params.Add(new SourceDocumentationParam() { Name = attribute.Parameters[0].Value?.ToString() ?? "", Description = attribute.Parameters[1].Value?.ToString() ?? "" });
            }
        }

        public SourceDocumentation(LuaXClass @class) : this(@class.Attributes)
        {
        }

        public SourceDocumentation(LuaXMethod @method) : this(@method.Attributes)
        {
        }

        public SourceDocumentation(LuaXProperty @property) : this(@property.Attributes)
        {
        }

        public SourceDocumentation(LuaXConstantVariable @property) : this(@property.Attributes)
        {
        }
    }
}


