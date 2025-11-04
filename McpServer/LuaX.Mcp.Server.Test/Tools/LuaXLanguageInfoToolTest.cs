using FluentAssertions;
using LuaX.Mcp.Server.Tools;
using Xunit;

namespace LuaX.Mcp.Server.Test.Tools;

/// <summary>
/// Unit tests for LuaXLanguageInfoTool.
/// </summary>
public class LuaXLanguageInfoToolTest
{
    [Fact]
    public void GetLanguageInfo_ShouldReturnBasicInfo()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.Should().NotBeNull();
        response.Name.Should().Be("LuaX");
        response.Description.Should().NotBeNullOrEmpty();
        response.Purpose.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLanguageInfo_ShouldIncludeDesignGoals()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.DesignGoals.Should().NotBeEmpty();
        response.DesignGoals.Should().Contain(goal => goal.Contains("Cross-compilation"));
        response.DesignGoals.Should().Contain(goal => goal.Contains("Type safety"));
        response.DesignGoals.Should().Contain(goal => goal.Contains("Object-oriented"));
    }

    [Fact]
    public void GetLanguageInfo_ShouldListTargetLanguages()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.TargetLanguages.Should().NotBeEmpty();
        response.TargetLanguages.Should().Contain("C#");
        response.TargetLanguages.Should().Contain("TypeScript/JavaScript");
        response.TargetLanguages.Should().Contain("Java");
        response.TargetLanguages.Should().Contain("Go");
        response.TargetLanguages.Should().Contain("Python");
    }

    [Fact]
    public void GetLanguageInfo_ShouldIncludeKeyFeatures()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.KeyFeatures.Should().NotBeEmpty();
        response.KeyFeatures.Should().Contain(feature => feature.Contains("Classes and inheritance"));
        response.KeyFeatures.Should().Contain(feature => feature.Contains("Strong static typing"));
        response.KeyFeatures.Should().Contain(feature => feature.Contains("Attributes/annotations"));
        response.KeyFeatures.Should().Contain(feature => feature.Contains("Exception handling"));
    }

    [Fact]
    public void GetLanguageInfo_ShouldDescribeTypeSystem()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.TypeSystem.Should().NotBeNull();
        response.TypeSystem.PrimitiveTypes.Should().Contain("int");
        response.TypeSystem.PrimitiveTypes.Should().Contain("real");
        response.TypeSystem.PrimitiveTypes.Should().Contain("boolean");
        response.TypeSystem.PrimitiveTypes.Should().Contain("string");
        response.TypeSystem.PrimitiveTypes.Should().Contain("datetime");
        response.TypeSystem.ArraySupport.Should().BeTrue();
        response.TypeSystem.ClassSupport.Should().BeTrue();
        response.TypeSystem.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLanguageInfo_ShouldIncludeStandardLibrary()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.StandardLibrary.Should().NotBeEmpty();
        response.StandardLibrary.Should().Contain(lib => lib.Contains("file"));
        response.StandardLibrary.Should().Contain(lib => lib.Contains("string"));
        response.StandardLibrary.Should().Contain(lib => lib.Contains("buffer"));
        response.StandardLibrary.Should().Contain(lib => lib.Contains("assert"));
    }

    [Fact]
    public void GetLanguageInfo_ShouldIncludeBestPractices()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.BestPractices.Should().NotBeEmpty();
        response.BestPractices.Should().Contain(practice => practice.Contains("@DocBrief"));
        response.BestPractices.Should().Contain(practice => practice.Contains("package"));
        response.BestPractices.Should().Contain(practice => practice.Contains("portable"));
    }

    [Fact]
    public void GetLanguageInfo_ShouldIncludeUseCases()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.UseCases.Should().NotBeEmpty();
        response.UseCases.Should().Contain(useCase => useCase.Contains("Cross-platform"));
        response.UseCases.Should().Contain(useCase => useCase.Contains("API data models"));
    }

    [Fact]
    public void GetLanguageInfo_ShouldIncludeSyntaxHighlights()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.SyntaxHighlights.Should().NotBeNull();
    }

    [Fact]
    public void GetLanguageInfo_ShouldBeConsistentAcrossMultipleCalls()
    {
        // Act
        var response1 = LuaXLanguageInfoTool.GetLanguageInfo();
        var response2 = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response1.Should().BeEquivalentTo(response2);
    }
}
