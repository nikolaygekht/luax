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

    [Fact]
    public void GetLanguageInfo_ShouldIncludeSyntaxDetails()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.SyntaxDetails.Should().NotBeNull();
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_ShouldIncludeThisKeyword()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.SyntaxDetails!.Keywords.Should().NotBeEmpty();
        var thisKeyword = response.SyntaxDetails.Keywords.Should().Contain(k => k.Keyword == "this").Subject;
        thisKeyword.Description.Should().Contain("current instance");
        thisKeyword.Usage.Should().NotBeNullOrEmpty();
        thisKeyword.Example.Should().Contain("this.");
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_ShouldIncludeSuperKeyword()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        var superKeyword = response.SyntaxDetails!.Keywords.Should().Contain(k => k.Keyword == "super").Subject;
        superKeyword.Description.Should().Contain("parent");
        superKeyword.Usage.Should().NotBeNullOrEmpty();
        superKeyword.Example.Should().Contain("super");
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_ShouldIncludeAllEssentialKeywords()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        var keywords = response.SyntaxDetails!.Keywords.Select(k => k.Keyword).ToArray();
        keywords.Should().Contain("this");
        keywords.Should().Contain("super");
        keywords.Should().Contain("new");
        keywords.Should().Contain("throw");
        keywords.Should().Contain("return");
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_ShouldIncludeOperators()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.SyntaxDetails!.Operators.Should().NotBeEmpty();
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "==");
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "!=");
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "+");
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "-");
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "*");
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "/");
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "..");
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "and");
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "or");
        response.SyntaxDetails.Operators.Should().Contain(op => op.Operator == "not");
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_OperatorsShouldHavePrecedence()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        foreach (var op in response.SyntaxDetails!.Operators)
        {
            op.Precedence.Should().BeGreaterThan(0);
            op.Description.Should().NotBeNullOrEmpty();
            op.Example.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_ShouldIncludeScopingRules()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.SyntaxDetails!.ScopingRules.Should().NotBeEmpty();
        response.SyntaxDetails.ScopingRules.Should().Contain(rule => rule.Contains("var"));
        response.SyntaxDetails.ScopingRules.Should().Contain(rule => rule.Contains("this"));
        response.SyntaxDetails.ScopingRules.Should().Contain(rule => rule.Contains("public"));
        response.SyntaxDetails.ScopingRules.Should().Contain(rule => rule.Contains("private"));
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_ShouldIncludeCommonPatterns()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        response.SyntaxDetails!.CommonPatterns.Should().NotBeEmpty();
        response.SyntaxDetails.CommonPatterns.Should().Contain(p => p.Name.Contains("Constructor"));
        response.SyntaxDetails.CommonPatterns.Should().Contain(p => p.Name.Contains("Inheritance"));
        response.SyntaxDetails.CommonPatterns.Should().Contain(p => p.Name.Contains("Error"));
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_PatternsShouldIncludeThisKeyword()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        var constructorPattern = response.SyntaxDetails!.CommonPatterns
            .Should().Contain(p => p.Name.Contains("Constructor")).Subject;
        constructorPattern.Example.Should().Contain("this.");
        constructorPattern.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_PatternsShouldIncludeSuperKeyword()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        var inheritancePattern = response.SyntaxDetails!.CommonPatterns
            .Should().Contain(p => p.Name.Contains("Inheritance")).Subject;
        inheritancePattern.Example.Should().Contain("super");
        inheritancePattern.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetLanguageInfo_SyntaxDetails_AllPatternsShouldHaveExamples()
    {
        // Act
        var response = LuaXLanguageInfoTool.GetLanguageInfo();

        // Assert
        foreach (var pattern in response.SyntaxDetails!.CommonPatterns)
        {
            pattern.Name.Should().NotBeNullOrEmpty();
            pattern.Description.Should().NotBeNullOrEmpty();
            pattern.Example.Should().NotBeNullOrEmpty();
            pattern.Example.Length.Should().BeGreaterThan(20, "examples should be meaningful code snippets");
        }
    }
}
