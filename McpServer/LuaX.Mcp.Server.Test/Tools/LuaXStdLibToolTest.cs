using FluentAssertions;
using LuaX.Mcp.Server.Tools;
using Xunit;

namespace LuaX.Mcp.Server.Test.Tools;

/// <summary>
/// Unit tests for LuaXStdLibTool.
/// </summary>
public class LuaXStdLibToolTest
{
    [Fact]
    public void GetStandardLibrary_ShouldReturnSuccess()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Error.Should().BeNull();
        response.StandardLibrary.Should().NotBeNull();
    }

    [Fact]
    public void GetStandardLibrary_ShouldReturnPackageInfo()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        response.StandardLibrary!.PackageName.Should().Be("LuaxStdlib");
        response.StandardLibrary.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetStandardLibrary_ShouldIncludeClasses()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        response.StandardLibrary!.Classes.Should().NotBeEmpty();
        response.StandardLibrary.Classes.Should().Contain(c => c.Name == "stdlib");
    }

    [Fact]
    public void GetStandardLibrary_ShouldIncludeCoreClasses()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var classes = response.StandardLibrary!.Classes;
        classes.Should().Contain(c => c.Name == "stdlib");
        classes.Should().Contain(c => c.Name == "file");
        classes.Should().Contain(c => c.Name == "buffer");
        classes.Should().Contain(c => c.Name == "list");
        classes.Should().Contain(c => c.Name == "csvParser");
    }

    [Fact]
    public void GetStandardLibrary_ShouldIncludeCategories()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        response.StandardLibrary!.Categories.Should().NotBeEmpty();
        response.StandardLibrary.Categories.Should().Contain(c => c.Name == "Core");
        response.StandardLibrary.Categories.Should().Contain(c => c.Name == "Collections");
        response.StandardLibrary.Categories.Should().Contain(c => c.Name == "I/O");
    }

    [Fact]
    public void GetStandardLibrary_ShouldReturnSummaryWithoutMethods()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert - Summary mode should have empty methods array
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        stdlibClass.Methods.Should().BeEmpty();
        stdlibClass.MethodCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetStandardLibrary_ShouldIncludeHint()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        response.Hint.Should().NotBeNullOrEmpty();
        response.Hint.Should().Contain("get_stdlib_class");
    }

    // New tests for get_stdlib_class tool
    [Fact]
    public void GetStdLibClass_WithValidClassName_ShouldReturnSuccess()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("stdlib");

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Error.Should().BeNull();
        response.Class.Should().NotBeNull();
    }

    [Fact]
    public void GetStdLibClass_WithInvalidClassName_ShouldReturnError()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("nonexistent");

        // Assert
        response.Success.Should().BeFalse();
        response.Error.Should().NotBeNullOrEmpty();
        response.Error.Should().Contain("not found");
        response.Error.Should().Contain("Available classes");
        response.Class.Should().BeNull();
    }

    [Fact]
    public void GetStdLibClass_Stdlib_ShouldHaveStringMethods()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("stdlib");

        // Assert
        var stdlibClass = response.Class!;
        stdlibClass.Methods.Should().NotBeEmpty();
        stdlibClass.Methods.Should().Contain(m => m.Name == "len");
        stdlibClass.Methods.Should().Contain(m => m.Name == "indexOf");
        stdlibClass.Methods.Should().Contain(m => m.Name == "upper");
        stdlibClass.Methods.Should().Contain(m => m.Name == "lower");
    }

    [Fact]
    public void GetStdLibClass_Stdlib_ShouldHaveMathMethods()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("stdlib");

        // Assert
        var stdlibClass = response.Class!;
        stdlibClass.Methods.Should().Contain(m => m.Name == "sin");
        stdlibClass.Methods.Should().Contain(m => m.Name == "cos");
        stdlibClass.Methods.Should().Contain(m => m.Name == "sqrt");
        stdlibClass.Methods.Should().Contain(m => m.Name == "abs");
    }

    [Fact]
    public void GetStdLibClass_Stdlib_ShouldHaveDateTimeMethods()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("stdlib");

        // Assert
        var stdlibClass = response.Class!;
        stdlibClass.Methods.Should().Contain(m => m.Name == "mkdate");
        stdlibClass.Methods.Should().Contain(m => m.Name == "nowlocal");
        stdlibClass.Methods.Should().Contain(m => m.Name == "year");
        stdlibClass.Methods.Should().Contain(m => m.Name == "month");
    }

    [Fact]
    public void GetStdLibClass_Methods_ShouldHaveDescriptions()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("stdlib");

        // Assert
        var stdlibClass = response.Class!;
        var lenMethod = stdlibClass.Methods.First(m => m.Name == "len");
        lenMethod.Description.Should().NotBeNullOrEmpty();
        lenMethod.Description.Should().Contain("length");
    }

    [Fact]
    public void GetStdLibClass_Methods_ShouldHaveReturnTypes()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("stdlib");

        // Assert
        var stdlibClass = response.Class!;
        var lenMethod = stdlibClass.Methods.First(m => m.Name == "len");
        lenMethod.ReturnType.Should().Contain("int");
    }

    [Fact]
    public void GetStdLibClass_Methods_ShouldHaveParameters()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("stdlib");

        // Assert
        var stdlibClass = response.Class!;
        var indexOfMethod = stdlibClass.Methods.First(m => m.Name == "indexOf");
        indexOfMethod.Parameters.Should().HaveCount(3);
        indexOfMethod.Parameters.Should().Contain(p => p.Name == "s" && p.Type.Contains("string"));
        indexOfMethod.Parameters.Should().Contain(p => p.Name == "sub" && p.Type.Contains("string"));
        indexOfMethod.Parameters.Should().Contain(p => p.Name == "caseSensitive" && p.Type.Contains("boolean"));
    }

    [Fact]
    public void GetStdLibClass_Methods_ShouldIndicateStatic()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("stdlib");

        // Assert
        var stdlibClass = response.Class!;
        var lenMethod = stdlibClass.Methods.First(m => m.Name == "len");
        lenMethod.IsStatic.Should().BeTrue();
    }

    [Fact]
    public void GetStdLibClass_Methods_ShouldIndicateExtern()
    {
        // Act
        var response = LuaXStdLibTool.GetStdLibClass("stdlib");

        // Assert
        var stdlibClass = response.Class!;
        var lenMethod = stdlibClass.Methods.First(m => m.Name == "len");
        lenMethod.IsExtern.Should().BeTrue();
    }

    [Fact]
    public void GetStdLibClass_ShouldBeCaseInsensitive()
    {
        // Act
        var response1 = LuaXStdLibTool.GetStdLibClass("stdlib");
        var response2 = LuaXStdLibTool.GetStdLibClass("STDLIB");
        var response3 = LuaXStdLibTool.GetStdLibClass("StdLib");

        // Assert
        response1.Success.Should().BeTrue();
        response2.Success.Should().BeTrue();
        response3.Success.Should().BeTrue();
    }

    [Fact]
    public void GetStandardLibrary_StdlibClass_ShouldHaveConstants()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        stdlibClass.Constants.Should().NotBeEmpty();
        stdlibClass.Constants.Should().Contain(c => c.Name == "PI");
        stdlibClass.Constants.Should().Contain(c => c.Name == "E");
    }

    [Fact]
    public void GetStandardLibrary_Constants_ShouldHaveValues()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        var piConstant = stdlibClass.Constants.First(c => c.Name == "PI");
        piConstant.Value.Should().Contain("3.14");
    }

    [Fact]
    public void GetStandardLibrary_ShouldBeCached()
    {
        // Act
        var response1 = LuaXStdLibTool.GetStandardLibrary();
        var response2 = LuaXStdLibTool.GetStandardLibrary();

        // Assert - Should return equivalent data (underlying data is cached)
        response1.StandardLibrary.Should().BeEquivalentTo(response2.StandardLibrary);
        response1.StandardLibrary!.Classes.Length.Should().Be(response2.StandardLibrary!.Classes.Length);
    }

    [Fact]
    public void GetStandardLibrary_Categories_ShouldHaveDescriptions()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var coreCategory = response.StandardLibrary!.Categories.First(c => c.Name == "Core");
        coreCategory.Description.Should().NotBeNullOrEmpty();
        coreCategory.Classes.Should().Contain("stdlib");
    }

    [Fact]
    public void GetStandardLibrary_Classes_ShouldBeCategorized()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        stdlibClass.Category.Should().Be("Core");

        var listClass = response.StandardLibrary!.Classes.First(c => c.Name == "list");
        listClass.Category.Should().Be("Collections");
    }
}
