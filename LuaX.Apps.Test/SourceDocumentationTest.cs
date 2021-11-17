using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Luax.Parser;
using Luax.Parser.Test.Utils;
using LuaX.Doc;
using Xunit;

namespace Luax.Apps.Test
{
    public class SourceDocumentationTest
    {
        private readonly LuaXApplication mApplication;

        public SourceDocumentationTest()
        {
            mApplication = new LuaXApplication();
            mApplication.CompileResource("DocTest1");
        }

        [Fact]
        public void ClassDocumentation()
        {
            mApplication.Classes.Search("class1", out var @class);
            var doc = new SourceDocumentation(@class);

            doc.Brief.Should().Be("This is brief for class 1");
            doc.Description.Should().HaveCount(2);
            doc.Description[0].Should().Be("This is the first line of the description");
            doc.Description[1].Should().Be("This is the second line of the description");
        }

        [Fact]
        public void ConstDocumentation()
        {
            mApplication.Classes.Search("class1", out var @class);
            @class.Constants.Search("C", out var @const);
            var doc = new SourceDocumentation(@const);

            doc.Brief.Should().Be("This constant brief");
        }

        [Fact]
        public void PropertyDocumentation()
        {
            mApplication.Classes.Search("class1", out var @class);
            @class.Properties.Search("a", out var @property);
            var doc = new SourceDocumentation(@property);

            doc.Brief.Should().Be("This is property brief");
            doc.Description.Should().HaveCount(0);
        }

        [Fact]
        public void PropertyNoDocumentation()
        {
            mApplication.Classes.Search("class1", out var @class);
            @class.Properties.Search("b", out var @property);
            var doc = new SourceDocumentation(@property);

            doc.Brief.Should().BeNullOrEmpty();
            doc.Description.Should().HaveCount(0);
        }

        [Fact]
        public void Method1Documentation()
        {
            mApplication.Classes.Search("class1", out var @class);
            @class.Methods.Search("f1", out var @method);
            var doc = new SourceDocumentation(@method);

            doc.Brief.Should().Be("This is f1 brief");
            doc.Description.Should().HaveCount(1);
            doc.Description[0].Should().Be("This is f1 description");
            doc.Return.Should().BeNullOrEmpty();

            doc.Params.Should().HaveCount(0);
        }

        [Fact]
        public void Method2Documentation()
        {
            mApplication.Classes.Search("class1", out var @class);
            @class.Methods.Search("f2", out var @method);
            var doc = new SourceDocumentation(@method);

            doc.Brief.Should().Be("This is f2 brief");
            doc.Description.Should().HaveCount(1);
            doc.Description[0].Should().Be("This is f2 description");
            doc.Return.Should().Be("This is f2 return");

            doc.Params.Should().HaveCount(2);
            doc.Params[0].Name.Should().Be("a");
            doc.Params[0].Description.Should().Be("a description");
            doc.Params[1].Name.Should().Be("b");
            doc.Params[1].Description.Should().Be("b description");
        }
    }
}
