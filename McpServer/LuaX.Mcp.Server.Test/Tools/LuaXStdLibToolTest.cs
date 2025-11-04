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
    public void GetStandardLibrary_StdlibClass_ShouldHaveMethods()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        stdlibClass.Methods.Should().NotBeEmpty();
    }

    [Fact]
    public void GetStandardLibrary_StdlibClass_ShouldHaveStringMethods()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        stdlibClass.Methods.Should().Contain(m => m.Name == "len");
        stdlibClass.Methods.Should().Contain(m => m.Name == "indexOf");
        stdlibClass.Methods.Should().Contain(m => m.Name == "upper");
        stdlibClass.Methods.Should().Contain(m => m.Name == "lower");
    }

    [Fact]
    public void GetStandardLibrary_StdlibClass_ShouldHaveMathMethods()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        stdlibClass.Methods.Should().Contain(m => m.Name == "sin");
        stdlibClass.Methods.Should().Contain(m => m.Name == "cos");
        stdlibClass.Methods.Should().Contain(m => m.Name == "sqrt");
        stdlibClass.Methods.Should().Contain(m => m.Name == "abs");
    }

    [Fact]
    public void GetStandardLibrary_StdlibClass_ShouldHaveDateTimeMethods()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        stdlibClass.Methods.Should().Contain(m => m.Name == "mkdate");
        stdlibClass.Methods.Should().Contain(m => m.Name == "nowlocal");
        stdlibClass.Methods.Should().Contain(m => m.Name == "year");
        stdlibClass.Methods.Should().Contain(m => m.Name == "month");
    }

    [Fact]
    public void GetStandardLibrary_Methods_ShouldHaveDescriptions()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        var lenMethod = stdlibClass.Methods.First(m => m.Name == "len");
        lenMethod.Description.Should().NotBeNullOrEmpty();
        lenMethod.Description.Should().Contain("length");
    }

    [Fact]
    public void GetStandardLibrary_Methods_ShouldHaveReturnTypes()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        var lenMethod = stdlibClass.Methods.First(m => m.Name == "len");
        lenMethod.ReturnType.Should().Contain("int");
    }

    [Fact]
    public void GetStandardLibrary_Methods_ShouldHaveParameters()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        var indexOfMethod = stdlibClass.Methods.First(m => m.Name == "indexOf");
        indexOfMethod.Parameters.Should().HaveCount(3);
        indexOfMethod.Parameters.Should().Contain(p => p.Name == "s" && p.Type.Contains("string"));
        indexOfMethod.Parameters.Should().Contain(p => p.Name == "sub" && p.Type.Contains("string"));
        indexOfMethod.Parameters.Should().Contain(p => p.Name == "caseSensitive" && p.Type.Contains("boolean"));
    }

    [Fact]
    public void GetStandardLibrary_Methods_ShouldIndicateStatic()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        var lenMethod = stdlibClass.Methods.First(m => m.Name == "len");
        lenMethod.IsStatic.Should().BeTrue();
    }

    [Fact]
    public void GetStandardLibrary_Methods_ShouldIndicateExtern()
    {
        // Act
        var response = LuaXStdLibTool.GetStandardLibrary();

        // Assert
        var stdlibClass = response.StandardLibrary!.Classes.First(c => c.Name == "stdlib");
        var lenMethod = stdlibClass.Methods.First(m => m.Name == "len");
        lenMethod.IsExtern.Should().BeTrue();
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

        // Assert - Should return same instance (cached)
        response1.StandardLibrary.Should().BeSameAs(response2.StandardLibrary);
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
