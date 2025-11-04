using FluentAssertions;
using LuaX.Mcp.Server.Tools;
using Xunit;

namespace LuaX.Mcp.Server.Test.Tools;

/// <summary>
/// Unit tests for LuaXParseTool.
/// </summary>
public class LuaXParseToolTest
{
    private const string SimpleClassSource = @"
class Person
    var name: string;
    var age: int;

    function getName(): string
        return name;
    end
end
";

    private const string ClassWithAttributesSource = @"
@DocBrief(""A simple calculator"")
@DocInclude()
class Calculator
    public static function add(a: int, b: int): int
        return a + b;
    end

    private function subtract(a: int, b: int): int
        return a - b;
    end
end
";

    private const string PackageWithClassesSource = @"
@DocBrief(""Math utilities"")
package MathUtils
    class Point
        public var x: int;
        public var y: int;
    end

    class Vector
        public var dx: int;
        public var dy: int;
    end
end
";

    [Fact]
    public void Parse_WithEmptySource_ShouldReturnError()
    {
        // Act
        var response = LuaXParseTool.Parse("");

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Error.Should().NotBeNullOrEmpty();
        response.Ast.Should().BeNull();
    }

    [Fact]
    public void Parse_WithSimpleClass_ShouldReturnSuccess()
    {
        // Act
        var response = LuaXParseTool.Parse(SimpleClassSource, "test.luax");

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Error.Should().BeNull();
        response.Ast.Should().NotBeNull();
    }

    [Fact]
    public void Parse_WithSimpleClass_ShouldParseClassName()
    {
        // Act
        var response = LuaXParseTool.Parse(SimpleClassSource);

        // Assert
        response.Ast.Should().NotBeNull();
        response.Ast!.Classes.Should().ContainSingle(c => c.Name == "Person");
    }

    [Fact]
    public void Parse_WithSimpleClass_ShouldParseProperties()
    {
        // Act
        var response = LuaXParseTool.Parse(SimpleClassSource);

        // Assert
        var personClass = response.Ast!.Classes.First(c => c.Name == "Person");
        personClass.Properties.Should().HaveCount(2);
        personClass.Properties.Should().Contain(p => p.Name == "name" && p.Type.Contains("string"));
        personClass.Properties.Should().Contain(p => p.Name == "age" && p.Type.Contains("int"));
    }

    [Fact]
    public void Parse_WithSimpleClass_ShouldParseMethods()
    {
        // Act
        var response = LuaXParseTool.Parse(SimpleClassSource);

        // Assert
        var personClass = response.Ast!.Classes.First(c => c.Name == "Person");
        personClass.Methods.Should().ContainSingle(m => m.Name == "getName");
        var method = personClass.Methods.First(m => m.Name == "getName");
        method.ReturnType.Should().Contain("string");
    }

    [Fact]
    public void Parse_WithClassWithAttributes_ShouldParseAttributes()
    {
        // Act
        var response = LuaXParseTool.Parse(ClassWithAttributesSource);

        // Assert
        response.Ast.Should().NotBeNull();
        var calcClass = response.Ast!.Classes.First(c => c.Name == "Calculator");
        calcClass.Attributes.Should().HaveCount(2);
        calcClass.Attributes.Should().Contain(a => a.Name == "DocBrief");
        calcClass.Attributes.Should().Contain(a => a.Name == "DocInclude");
    }

    [Fact]
    public void Parse_WithClassWithAttributes_ShouldParseAttributeParameters()
    {
        // Act
        var response = LuaXParseTool.Parse(ClassWithAttributesSource);

        // Assert
        var calcClass = response.Ast!.Classes.First(c => c.Name == "Calculator");
        var docBrief = calcClass.Attributes.First(a => a.Name == "DocBrief");
        docBrief.Parameters.Should().NotBeEmpty();
        docBrief.Parameters.First().Should().Contain("calculator");
    }

    [Fact]
    public void Parse_WithStaticMethod_ShouldDetectStatic()
    {
        // Act
        var response = LuaXParseTool.Parse(ClassWithAttributesSource);

        // Assert
        var calcClass = response.Ast!.Classes.First(c => c.Name == "Calculator");
        var addMethod = calcClass.Methods.First(m => m.Name == "add");
        addMethod.IsStatic.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithPrivateMethod_ShouldDetectVisibility()
    {
        // Act
        var response = LuaXParseTool.Parse(ClassWithAttributesSource);

        // Assert
        var calcClass = response.Ast!.Classes.First(c => c.Name == "Calculator");
        var subtractMethod = calcClass.Methods.First(m => m.Name == "subtract");
        subtractMethod.Visibility.Should().Contain("Private");
    }

    [Fact]
    public void Parse_WithMethodParameters_ShouldParseParameters()
    {
        // Act
        var response = LuaXParseTool.Parse(ClassWithAttributesSource);

        // Assert
        var calcClass = response.Ast!.Classes.First(c => c.Name == "Calculator");
        var addMethod = calcClass.Methods.First(m => m.Name == "add");
        addMethod.Parameters.Should().HaveCount(2);
        addMethod.Parameters.Should().Contain(p => p.Name == "a" && p.Type.Contains("int"));
        addMethod.Parameters.Should().Contain(p => p.Name == "b" && p.Type.Contains("int"));
    }

    [Fact]
    public void Parse_WithPackage_ShouldParsePackage()
    {
        // Act
        var response = LuaXParseTool.Parse(PackageWithClassesSource);

        // Assert
        response.Ast.Should().NotBeNull();
        response.Ast!.Packages.Should().ContainSingle(p => p.Name == "MathUtils");
    }

    [Fact]
    public void Parse_WithPackage_ShouldParseClassesInPackage()
    {
        // Act
        var response = LuaXParseTool.Parse(PackageWithClassesSource);

        // Assert
        // Classes from packages are in the main Classes collection
        // The PackageName property indicates which package they belong to
        response.Ast!.Classes.Should().Contain(c => c.Name == "Point" && c.PackageName == "MathUtils");
        response.Ast!.Classes.Should().Contain(c => c.Name == "Vector" && c.PackageName == "MathUtils");

        var pointClass = response.Ast!.Classes.First(c => c.Name == "Point");
        pointClass.PackageName.Should().Be("MathUtils");
    }

    [Fact]
    public void Parse_WithInvalidSyntax_ShouldReturnError()
    {
        // Arrange
        var invalidSource = "class Person var name: string end"; // Missing proper structure

        // Act
        var response = LuaXParseTool.Parse(invalidSource);

        // Assert
        response.Success.Should().BeFalse();
        response.Error.Should().NotBeNullOrEmpty();
        response.Ast.Should().BeNull();
    }

    [Fact]
    public void Parse_WithSourceName_ShouldUseProvidedName()
    {
        // Act
        var response = LuaXParseTool.Parse(SimpleClassSource, "MyFile.luax");

        // Assert
        response.Ast.Should().NotBeNull();
        response.Ast!.SourceName.Should().Be("MyFile.luax");
    }

    [Fact]
    public void Parse_WithoutSourceName_ShouldUseDefaultName()
    {
        // Act
        var response = LuaXParseTool.Parse(SimpleClassSource);

        // Assert
        response.Ast.Should().NotBeNull();
        response.Ast!.SourceName.Should().Be("inline");
    }
}
