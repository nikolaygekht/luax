using FluentAssertions;
using Xunit;
using LuaX.Mcp.Server.Tools;

namespace LuaX.Mcp.Server.Test.Tools;

/// <summary>
/// Tests for LuaXInfoTool
/// </summary>
public class LuaXInfoToolTest
{
    [Fact]
    public void GetInfo_ReturnsServerInfo()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.Should().NotBeNull();
        result.ServerName.Should().Be("LuaX MCP Server");
        result.Status.Should().Be("Running");
        result.Features.Should().NotBeEmpty();
        result.Features.Should().Contain("Parser integration");
    }

    [Fact]
    public void GetInfo_ReturnsVersion()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.Version.Should().NotBeNullOrEmpty();
    }
}
