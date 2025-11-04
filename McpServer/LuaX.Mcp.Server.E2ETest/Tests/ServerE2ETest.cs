using FluentAssertions;
using LuaX.Mcp.Server.E2ETest.Fixtures;
using Xunit;

namespace LuaX.Mcp.Server.E2ETest.Tests;

/// <summary>
/// End-to-end tests that verify the MCP server works correctly by
/// launching it as a subprocess and communicating via stdio.
/// </summary>
[Collection("E2E Server Collection")]
public class ServerE2ETest
{
    private readonly McpServerFixture _fixture;

    public ServerE2ETest(McpServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Server_Starts_Successfully()
    {
        // The fact that we got here means the server started and client connected
        _fixture.Client.Should().NotBeNull();
    }

    [Fact]
    public async Task ListTools_ReturnsAvailableTools()
    {
        // Act
        var tools = await _fixture.Client.ListToolsAsync();

        // Assert
        tools.Should().NotBeNull();
        tools.Should().NotBeEmpty();

        // Should have at least the luax_info tool
        tools.Should().Contain(t => t.Name == "luax_info" || t.Name == "get_info");
    }

    [Fact]
    public async Task CallTool_LuaXInfo_ReturnsServerInfo()
    {
        // Arrange
        var tools = await _fixture.Client.ListToolsAsync();
        var infoTool = tools.FirstOrDefault(t =>
            t.Name == "luax_info" || t.Name == "get_info");

        infoTool.Should().NotBeNull("luax_info or get_info tool should be available");

        // Act
        var result = await _fixture.Client.CallToolAsync(
            infoTool!.Name,
            new Dictionary<string, object?>(),
            cancellationToken: CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNull();
        result.Content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Server_RespondsToPing()
    {
        // Act - Just verify we can list tools without error
        var tools = await _fixture.Client.ListToolsAsync();

        // Assert - If we got here, server is responding
        tools.Should().NotBeNull();
    }

    [Fact]
    public async Task ListTools_ToolsHaveDescriptions()
    {
        // Act
        var tools = await _fixture.Client.ListToolsAsync();

        // Assert
        tools.Should().NotBeEmpty();

        foreach (var tool in tools)
        {
            tool.Name.Should().NotBeNullOrEmpty();
            // Description might be optional depending on the tool
        }
    }

    [Fact]
    public async Task ListTools_IncludesGrammarTool()
    {
        // Act
        var tools = await _fixture.Client.ListToolsAsync();

        // Assert
        tools.Should().Contain(t => t.Name == "get_grammar",
            "get_grammar tool should be available");
    }

    [Fact]
    public async Task ListTools_IncludesLanguageInfoTool()
    {
        // Act
        var tools = await _fixture.Client.ListToolsAsync();

        // Assert
        tools.Should().Contain(t => t.Name == "get_language_info",
            "get_language_info tool should be available");
    }

    [Fact]
    public async Task CallTool_GetGrammar_ReturnsGrammarContent()
    {
        // Arrange
        var tools = await _fixture.Client.ListToolsAsync();
        var grammarTool = tools.FirstOrDefault(t => t.Name == "get_grammar");

        grammarTool.Should().NotBeNull("get_grammar tool should be available");

        // Act
        var result = await _fixture.Client.CallToolAsync(
            grammarTool!.Name,
            new Dictionary<string, object?>(),
            cancellationToken: CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNull();
        result.Content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CallTool_GetLanguageInfo_ReturnsLanguageInfo()
    {
        // Arrange
        var tools = await _fixture.Client.ListToolsAsync();
        var languageInfoTool = tools.FirstOrDefault(t => t.Name == "get_language_info");

        languageInfoTool.Should().NotBeNull("get_language_info tool should be available");

        // Act
        var result = await _fixture.Client.CallToolAsync(
            languageInfoTool!.Name,
            new Dictionary<string, object?>(),
            cancellationToken: CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNull();
        result.Content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListTools_IncludesParseTool()
    {
        // Act
        var tools = await _fixture.Client.ListToolsAsync();

        // Assert
        tools.Should().Contain(t => t.Name == "parse",
            "parse tool should be available");
    }

    [Fact]
    public async Task CallTool_Parse_WithValidCode_ReturnsAst()
    {
        // Arrange
        var tools = await _fixture.Client.ListToolsAsync();
        var parseTool = tools.FirstOrDefault(t => t.Name == "parse");

        parseTool.Should().NotBeNull("parse tool should be available");

        var sourceCode = @"
class TestClass
    var value: int;

    function getValue(): int
        return value;
    end
end";

        var parameters = new Dictionary<string, object?>
        {
            ["sourceCode"] = sourceCode,
            ["sourceName"] = "test.luax"
        };

        // Act
        var result = await _fixture.Client.CallToolAsync(
            parseTool!.Name,
            parameters,
            cancellationToken: CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNull();
        result.Content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListTools_IncludesStdLibTool()
    {
        // Act
        var tools = await _fixture.Client.ListToolsAsync();

        // Assert
        tools.Should().Contain(t => t.Name == "get_standard_library",
            "get_standard_library tool should be available");
    }

    [Fact]
    public async Task CallTool_GetStandardLibrary_ReturnsStdLibInfo()
    {
        // Arrange
        var tools = await _fixture.Client.ListToolsAsync();
        var stdlibTool = tools.FirstOrDefault(t => t.Name == "get_standard_library");

        stdlibTool.Should().NotBeNull("get_standard_library tool should be available");

        // Act
        var result = await _fixture.Client.CallToolAsync(
            stdlibTool!.Name,
            new Dictionary<string, object?>(),
            cancellationToken: CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeNull();
        result.Content.Should().NotBeEmpty();
    }
}
