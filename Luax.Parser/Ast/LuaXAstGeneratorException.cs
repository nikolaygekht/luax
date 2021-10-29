using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Hime.Redist;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// The exception for the parser.
    ///
    /// See [clink=LuaXAstGenerator.Create]LuaXAstGenerator.Create[/clink] for details.
    /// </summary>
    [Serializable]
    public class LuaXAstGeneratorException : Exception
    {
        public LuaXAstGeneratorErrorCollection Errors { get; } = new LuaXAstGeneratorErrorCollection();

        public string SourceName { get; }

        internal LuaXAstGeneratorException(string name, IEnumerable<ParseError> errors)
            : this(name, ToLuaXErrors(errors))
        {
        }

        internal LuaXAstGeneratorException(string name, LuaXParserError error)
            : this(name, new LuaXParserError[] { error })
        {
        }

        internal LuaXAstGeneratorException(string name, IAstNode node, string message)
            : this(name, new LuaXParserError[] { new LuaXParserError(node, message) })
        {
        }

        public LuaXAstGeneratorException(LuaXElementLocation location, string message)
            : this(location.Source, new LuaXParserError[] { new LuaXParserError(location, message) })
        {
        }

        internal LuaXAstGeneratorException(string name, IEnumerable<LuaXParserError> errors)
            : base(CreateMessage(name, errors))
        {
            Errors.AddRange(errors);
            SourceName = name;
        }

        protected LuaXAstGeneratorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SourceName = info.GetString("sourcename");
            Errors = (LuaXAstGeneratorErrorCollection)info.GetValue("parsererrors", typeof(LuaXAstGeneratorErrorCollection));
        }

        private static IEnumerable<LuaXParserError> ToLuaXErrors(IEnumerable<ParseError> errors)
        {
            foreach (var error in errors)
                yield return new LuaXParserError(error);
        }

        private static string CreateMessage(string name, IEnumerable<LuaXParserError> errors)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var error in errors)
            {
                sb.Append(name)
                    .Append('(')
                    .Append(error.Line)
                    .Append(',')
                    .Append(error.Column)
                    .Append(')')
                    .Append(" : ")
                    .AppendLine(error.Message);
            }
            return sb.ToString();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("sourcename", SourceName);
            info.AddValue("parsererrors", Errors);
        }
    }
}
