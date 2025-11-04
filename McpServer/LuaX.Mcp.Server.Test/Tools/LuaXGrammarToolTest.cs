using FluentAssertions;
using LuaX.Mcp.Server.Tools;
using Xunit;

namespace LuaX.Mcp.Server.Test.Tools;

/// <summary>
/// Unit tests for LuaXGrammarTool.
/// </summary>
public class LuaXGrammarToolTest
{
    [Fact]
    public void GetGrammar_ShouldReturnSuccess()
    {
        // Act
        var response = LuaXGrammarTool.GetGrammar();

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Error.Should().BeNull();
    }

    [Fact]
    public void GetGrammar_ShouldReturnGrammarContent()
    {
        // Act
        var response = LuaXGrammarTool.GetGrammar();

        // Assert
        response.Grammar.Should().NotBeNullOrEmpty();
        response.Grammar.Should().Contain("grammar LuaX");
    }

    [Fact]
    public void GetGrammar_ShouldContainExpectedGrammarElements()
    {
        // Act
        var response = LuaXGrammarTool.GetGrammar();

        // Assert
        response.Grammar.Should().Contain("terminals");
        response.Grammar.Should().Contain("rules");
        response.Grammar.Should().Contain("IDENTIFIER");
        response.Grammar.Should().Contain("CLASS_DECLARATION");
        response.Grammar.Should().Contain("FUNCTION_DECLARATION");
        response.Grammar.Should().Contain("PACKAGE_DECLARATION");
    }

    [Fact]
    public void GetGrammar_ShouldContainTypeDeclarations()
    {
        // Act
        var response = LuaXGrammarTool.GetGrammar();

        // Assert
        response.Grammar.Should().Contain("TYPE_INT");
        response.Grammar.Should().Contain("TYPE_REAL");
        response.Grammar.Should().Contain("TYPE_BOOLEAN");
        response.Grammar.Should().Contain("TYPE_STRING");
        response.Grammar.Should().Contain("TYPE_DATETIME");
    }

    [Fact]
    public void GetGrammar_ShouldContainAttributeSupport()
    {
        // Act
        var response = LuaXGrammarTool.GetGrammar();

        // Assert
        response.Grammar.Should().Contain("ATTRIBUTE");
        response.Grammar.Should().Contain("ATTRIBUTES");
    }

    [Fact]
    public void GetGrammar_ShouldBeConsistentAcrossMultipleCalls()
    {
        // Act
        var response1 = LuaXGrammarTool.GetGrammar();
        var response2 = LuaXGrammarTool.GetGrammar();

        // Assert
        response1.Grammar.Should().Be(response2.Grammar);
        response1.Success.Should().Be(response2.Success);
    }
}
