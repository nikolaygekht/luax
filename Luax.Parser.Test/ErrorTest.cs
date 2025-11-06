using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Hime.Redist;
using Luax.Parser.Ast;
using Luax.Parser.Test.Tools;
using Moq;
using Xunit;

#pragma warning disable SYSLIB0011  // Type or member is obsolete

namespace Luax.Parser.Test
{
    public class ErrorTest
    {
        [Fact]
        public void ErrorConstructor_FromParams()
        {
            AstNodeWrapper w = new AstNodeWrapper(1, 2, "", "");
            var error = new LuaXParserError(w, "message");
            error.Line.Should().Be(1);
            error.Column.Should().Be(2);
            error.Message.Should().Be("message");
        }

        [Fact]
        public void ErrorConstructor_FromHime()
        {
            var himeError = new UnexpectedEndOfInput(new TextPosition(1, 2));

            var error = new LuaXParserError(himeError);
            error.Line.Should().Be(1);
            error.Column.Should().Be(2);
            error.Message.Should().Be(himeError.Message);
        }

        [Fact]
        public void ErrorCollection_Add()
        {
            var collection = new LuaXAstGeneratorErrorCollection();
            collection.Count.Should().Be(0);
            collection.Should().HaveCount(0);

            AstNodeWrapper w = new AstNodeWrapper(1, 2, "", "");
            var o = new LuaXParserError(w, "message");
            collection.Add(o);
            collection.Count.Should().Be(1);
            collection.Should().HaveCount(1);
            collection[0].Should().BeSameAs(o);
        }

        [Fact]
        public void Exception_Constructor_1()
        {
            var himeError = new UnexpectedEndOfInput(new TextPosition(1, 2));
            var exception = new LuaXAstGeneratorException("sourceName", new ParseError[] { himeError });
            exception.SourceName.Should().Be("sourceName");
            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].Should().Match<LuaXParserError>(o => o.Line == 1 && o.Column == 2 && o.Message == himeError.Message);
            exception.Message.Should().Be("sourceName(1,2) : " + himeError.Message + Environment.NewLine);
        }

        [Fact]
        public void Exception_Constructor_2()
        {
            var himeError1 = new UnexpectedEndOfInput(new TextPosition(1, 2));
            var himeError2 = new UnexpectedEndOfInput(new TextPosition(3, 4));
            var exception = new LuaXAstGeneratorException("sourceName", new ParseError[] { himeError1, himeError2 });
            exception.SourceName.Should().Be("sourceName");
            exception.Errors.Should().HaveCount(2);
            exception.Errors[0].Should().Match<LuaXParserError>(o => o.Line == 1 && o.Column == 2 && o.Message == himeError1.Message);
            exception.Errors[1].Should().Match<LuaXParserError>(o => o.Line == 3 && o.Column == 4 && o.Message == himeError2.Message);
            exception.Message.Should().Be("sourceName(1,2) : " + himeError1.Message + Environment.NewLine +
                                          "sourceName(3,4) : " + himeError2.Message + Environment.NewLine);
        }
    }
}
