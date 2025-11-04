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
        result.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetInfo_ReturnsVersion()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.Version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetInfo_ReturnsAvailableTools()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.AvailableTools.Should().NotBeEmpty();
        result.AvailableTools.Should().HaveCount(6);
        result.AvailableTools.Should().Contain(t => t.Contains("get_info"));
        result.AvailableTools.Should().Contain(t => t.Contains("get_grammar"));
        result.AvailableTools.Should().Contain(t => t.Contains("get_language_info"));
        result.AvailableTools.Should().Contain(t => t.Contains("parse"));
        result.AvailableTools.Should().Contain(t => t.Contains("get_standard_library"));
        result.AvailableTools.Should().Contain(t => t.Contains("get_stdlib_class"));
    }

    [Fact]
    public void GetInfo_ReturnsFeatures()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.Features.Should().NotBeEmpty();
        result.Features.Should().Contain(f => f.Contains("grammar"));
        result.Features.Should().Contain(f => f.Contains("parsing"));
        result.Features.Should().Contain(f => f.Contains("standard library"));
    }

    [Fact]
    public void GetInfo_ReturnsSupportedFileTypes()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.SupportedFileTypes.Should().NotBeEmpty();
        result.SupportedFileTypes.Should().Contain(".luax");
    }

    [Fact]
    public void GetInfo_ReturnsStandardLibraryClassCount()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.StandardLibraryClasses.Should().BeGreaterThan(0);
        result.StandardLibraryClasses.Should().Be(26);
    }

    [Fact]
    public void GetInfo_ReturnsRecommendedWorkflow()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.RecommendedWorkflow.Should().NotBeEmpty();
        result.RecommendedWorkflow.Should().HaveCountGreaterThan(3);
    }

    [Fact]
    public void GetInfo_RecommendedWorkflow_MentionsParseFirst()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.RecommendedWorkflow.Should().Contain(w => w.Contains("parse") && w.Contains("first"));
    }

    [Fact]
    public void GetInfo_RecommendedWorkflow_IsOrderedSteps()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.RecommendedWorkflow[0].Should().StartWith("1.");
        result.RecommendedWorkflow.Should().Contain(w => w.Contains("get_language_info"));
        result.RecommendedWorkflow.Should().Contain(w => w.Contains("get_standard_library"));
    }

    [Fact]
    public void GetInfo_ReturnsBestPractices()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.BestPractices.Should().NotBeEmpty();
        result.BestPractices.Should().HaveCountGreaterThan(3);
    }

    [Fact]
    public void GetInfo_BestPractices_MentionsThisKeyword()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.BestPractices.Should().Contain(p => p.Contains("this"));
    }

    [Fact]
    public void GetInfo_BestPractices_MentionsSuperKeyword()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.BestPractices.Should().Contain(p => p.Contains("super"));
    }

    [Fact]
    public void GetInfo_BestPractices_MentionsValidation()
    {
        // Act
        var result = LuaXInfoTool.GetInfo();

        // Assert
        result.BestPractices.Should().Contain(p => p.Contains("parse") || p.Contains("validate"));
    }
}
