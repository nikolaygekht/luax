using FluentAssertions;
using Xunit;

namespace Luax.Parser.Test.Tools
{
    public class AstNodeExtensionsTest
    {
        [Fact]
        public void ASTTreeParse1()
        {
            var node = AstNodeExtensions.Parse("[symbola]");
            node.Should().NotBeNull();
            node.Symbol.Should().Be("symbola");
            node.Value.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ASTTreeParse2()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)]");
            node.Should().NotBeNull();
            node.Symbol.Should().Be("symbola");
            node.Value.Should().Be("valuea");
        }

        [Fact]
        public void ASTTreeParse3()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb][symbolc][symbold]]");
            node.Should().NotBeNull();
            node.Symbol.Should().Be("symbola");
            node.Value.Should().Be("valuea");
            node.Children.Count.Should().Be(3);
            node.Children[0].Symbol.Should().Be("symbolb");
            node.Children[1].Symbol.Should().Be("symbolc");
            node.Children[2].Symbol.Should().Be("symbold");
        }

        [Fact]
        public void ASTTreeParse4()
        {
            var node = AstNodeExtensions.Parse("[symbola[symbolb[symbolc(valuec)[symbold(valued)]]]]");
            node.Should().NotBeNull();
            node.Symbol.Should().Be("symbola");
            node.Children.Count.Should().Be(1);
            node.Children[0].Symbol.Should().Be("symbolb");
            node.Children[0].Children.Count.Should().Be(1);
            node.Children[0].Children[0].Symbol.Should().Be("symbolc");
            node.Children[0].Children[0].Value.Should().Be("valuec");
            node.Children[0].Children[0].Children.Count.Should().Be(1);
            node.Children[0].Children[0].Children[0].Symbol.Should().Be("symbold");
            node.Children[0].Children[0].Children[0].Value.Should().Be("valued");
            node.Children[0].Children[0].Children[0].Children.Count.Should().Be(0);
        }

        [Fact]
        public void ScanChildren1()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valued)]]");
            node.ScanChildren(false, "*", null, null).Should().Contain(n => n.Symbol == "symbolb")
                .And.Contain(n => n.Symbol == "symbolc")
                .And.Contain(n => n.Symbol == "symbold");
        }

        [Fact]
        public void ScanChildren2()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valued)]]");
            node.ScanChildren(false, "*c", null, null).Should().NotContain(n => n.Symbol == "symbolb")
                .And.Contain(n => n.Symbol == "symbolc")
                .And.NotContain(n => n.Symbol == "symbold");
        }

        [Fact]
        public void ScanChildren3()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valuex)[symbole(valuex)]]]");
            node.ScanChildren(false, "*", "*x", null).Should().NotContain(n => n.Symbol == "symbolb")
                .And.NotContain(n => n.Symbol == "symbolc")
                .And.NotContain(n => n.Symbol == "symbole")
                .And.Contain(n => n.Symbol == "symbold");
        }

        [Fact]
        public void ScanChidren4()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valuex)[symbole(valuex)]]]");
            node.ScanChildren(true, "*", "*x", null).Should().NotContain(n => n.Symbol == "symbolb")
                .And.NotContain(n => n.Symbol == "symbolc")
                .And.Contain(n => n.Symbol == "symbole")
                .And.Contain(n => n.Symbol == "symbold");
        }

        [Fact]
        public void ScanChildren5()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valuex)[symbole(valuex)][symbole1(valuex)]][symbold1(valuex)]]");
            node.ScanChildren(false, "*", "*x", 2).Should()
                .NotContain(n => n.Symbol == "symbolb")
                .And.NotContain(n => n.Symbol == "symbolc")
                .And.NotContain(n => n.Symbol == "symbold")
                .And.Contain(n => n.Symbol == "symbold1")
                .And.NotContain(n => n.Symbol == "symbole")
                .And.NotContain(n => n.Symbol == "symbole1");
        }

        [Fact]
        public void ScanChildren6()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valuex)[symbole(valuex)][symbole1(valuex)]][symbold1(valuex)]]");
            node.ScanChildren(true, "*", "*x", 2).Should()
                .NotContain(n => n.Symbol == "symbolb")
                .And.NotContain(n => n.Symbol == "symbolc")
                .And.NotContain(n => n.Symbol == "symbold")
                .And.Contain(n => n.Symbol == "symbold1")
                .And.NotContain(n => n.Symbol == "symbole")
                .And.Contain(n => n.Symbol == "symbole1");
        }

        [Fact]
        public void Select1()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valuex)[symbole(valuex)][symbole1(valuex)]][symbold1(valuex)]]");
            node.Select("/*")
                .Should().HaveCount(4)
                .And.Contain(n => n.Symbol == "symbolb")
                .And.Contain(n => n.Symbol == "symbolc")
                .And.Contain(n => n.Symbol == "symbold")
                .And.Contain(n => n.Symbol == "symbold1");
        }

        [Fact]
        public void Select2()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valuex)[symbole(valuex)][symbole1(valuex)]][symbold1(valuex)]]");
            node.Select("//*")
                .Should().HaveCount(6)
                .And.Contain(n => n.Symbol == "symbolb")
                .And.Contain(n => n.Symbol == "symbolc")
                .And.Contain(n => n.Symbol == "symbold")
                .And.Contain(n => n.Symbol == "symbold1")
                .And.Contain(n => n.Symbol == "symbole")
                .And.Contain(n => n.Symbol == "symbole1");
        }

        [Fact]
        public void Select3()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valuex)[symbole(valuex)][symbole1(valuex)]][symbold1(valuex)]]");

            node.Select("//*[2]")
                .Should().HaveCount(2)
                .And.NotContain(n => n.Symbol == "symbolb")
                .And.Contain(n => n.Symbol == "symbolc")
                .And.NotContain(n => n.Symbol == "symbold")
                .And.NotContain(n => n.Symbol == "symbold1")
                .And.NotContain(n => n.Symbol == "symbole")
                .And.Contain(n => n.Symbol == "symbole1");
        }

        [Fact]
        public void Select4()
        {
            var node = AstNodeExtensions.Parse("[symbola(valuea)[symbolb(valueb)][symbolc(valuec)][symbold(valuex)[symbole(valuex)][symbole1(valuex)]][symbold1(valuex)]]");

            node.Select("//*d//*(*x)")
                .Should().HaveCount(2)
                .And.NotContain(n => n.Symbol == "symbolb")
                .And.NotContain(n => n.Symbol == "symbolc")
                .And.NotContain(n => n.Symbol == "symbold")
                .And.NotContain(n => n.Symbol == "symbold1")
                .And.Contain(n => n.Symbol == "symbole")
                .And.Contain(n => n.Symbol == "symbole1");
        }
    }
}
