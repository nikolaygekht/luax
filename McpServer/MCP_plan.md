# LuaX MCP Server - Development Plan

## Overview

This document outlines the plan for developing a Model Context Protocol (MCP) server that enables Claude Code and other MCP clients to intelligently work with LuaX programming language files.

## Goals

- Provide Claude with deep understanding of LuaX syntax, semantics, and **purpose**
- Enable code navigation, validation, and analysis of LuaX projects
- Support LuaX's **cross-compilation** workflow and portability checking
- Provide **context-aware assistance** beyond syntax (intent, best practices, examples)
- Enable **code generation** and scaffolding for LuaX classes, methods, and documentation
- Make LuaX tooling accessible across all Claude Code sessions
- Leverage existing Hime parser infrastructure

## Project Structure

```
/mnt/d/develop/work/tools/luax/
├── McpServer/                          # MCP Server root folder
│   ├── MCP_plan.md                     # This document
│   ├── LuaX.Mcp.Server/               # Main server project
│   │   ├── LuaX.Mcp.Server.csproj
│   │   ├── Program.cs
│   │   ├── Tools/                     # MCP tool implementations
│   │   ├── Services/                  # Parser, symbol extraction, etc.
│   │   └── Resources/                 # luax:// resource providers
│   └── LuaX.Mcp.Server.Test/          # Test project
│       ├── LuaX.Mcp.Server.Test.csproj
│       ├── Tools/                     # Tool tests
│       ├── Integration/               # Integration tests
│       └── TestSources/               # Test .luax files
├── Luax.Parser/                        # Existing parser (reference this)
├── Luax.Interpreter/                   # Existing interpreter
├── grammar/luax.gram                   # Grammar definition
└── [other LuaX projects...]

/mnt/d/develop/work/projects/WebTS-API/core/
└── [hundreds of .luax files]          # Production codebase for testing
```

## Technology Stack

### C# MCP Server
- **Language**: C# / .NET
- **SDK**: [ModelContextProtocol NuGet package](https://www.nuget.org/packages/ModelContextProtocol)
  - GitHub: https://github.com/modelcontextprotocol/csharp-sdk
- **Rationale**:
  - Reuse existing Hime parser and LuaX infrastructure
  - Direct integration with LuaX codebase
  - Official C# SDK available with full MCP support
  - No need for cross-language communication overhead
- **Deployment**: Standalone executable
- **Transport**: stdio (standard input/output for MCP communication)

## Phase 1: Foundation (Week 1-2)

### 1.1 Project Setup
- [ ] Create new .NET console project: `McpServer/LuaX.Mcp.Server/`
- [ ] Add NuGet package: `ModelContextProtocol`
- [ ] Reference existing LuaX projects:
  - `../Luax.Parser/Luax.Parser.csproj`
  - `../Luax.Interpreter/Luax.Interpreter.csproj`
- [ ] Set up build configuration for standalone executable
- [ ] Configure stdio transport for MCP communication
- [ ] Set up dependency injection for MCP server components

### 1.2 Core Infrastructure
- [ ] Integrate Hime parser for LuaX
- [ ] Create AST wrapper/simplification layer
- [ ] Implement file system utilities
- [ ] Add logging and error handling

### 1.3 Basic Tools (MVP)

#### Tool: `luax_parse`
**Description**: Parse a LuaX file and return structured representation
```json
{
  "name": "luax_parse",
  "parameters": {
    "file_path": "string (absolute path to .luax file)"
  },
  "returns": {
    "success": "boolean",
    "ast": "simplified AST object",
    "errors": ["array of parse errors"]
  }
}
```

#### Tool: `luax_validate`
**Description**: Validate LuaX syntax and return errors/warnings
```json
{
  "name": "luax_validate",
  "parameters": {
    "file_path": "string"
  },
  "returns": {
    "valid": "boolean",
    "errors": [{"line": 10, "column": 5, "message": "..."}],
    "warnings": [{"line": 15, "message": "..."}]
  }
}
```

#### Tool: `luax_list_symbols`
**Description**: List all classes, functions, and properties in a file
```json
{
  "name": "luax_list_symbols",
  "parameters": {
    "file_path": "string",
    "symbol_type": "optional: 'class' | 'function' | 'property' | 'all'"
  },
  "returns": {
    "symbols": [
      {
        "name": "ClassName",
        "type": "class",
        "line": 15,
        "visibility": "public",
        "parent": "ParentClass"
      }
    ]
  }
}
```

### 1.4 Testing Infrastructure
- [ ] Create test fixture with sample .luax files
- [ ] Unit tests for parser integration
- [ ] Integration tests for MCP tool invocations
- [ ] Test error handling scenarios

## Phase 2: Enhanced Features (Week 3-4)

### 2.1 Symbol Navigation

#### Tool: `luax_find_definition`
**Description**: Find the definition location of a symbol
```json
{
  "parameters": {
    "symbol_name": "string",
    "context_file": "optional path for scoped search"
  },
  "returns": {
    "file_path": "string",
    "line": "number",
    "column": "number",
    "definition": "code snippet"
  }
}
```

#### Tool: `luax_get_class_info`
**Description**: Get detailed information about a class
```json
{
  "parameters": {
    "class_name": "string"
  },
  "returns": {
    "name": "string",
    "parent": "string or null",
    "properties": ["array of property info"],
    "methods": ["array of method signatures"],
    "static_members": ["array"],
    "attributes": ["array"]
  }
}
```

### 2.2 Project-Level Analysis

#### Tool: `luax_get_hierarchy`
**Description**: Get class inheritance hierarchy for the project
```json
{
  "parameters": {
    "root_directory": "string"
  },
  "returns": {
    "classes": {
      "ClassName": {
        "parent": "ParentClass",
        "children": ["ChildClass1", "ChildClass2"],
        "file": "path/to/file.luax"
      }
    }
  }
}
```

#### Tool: `luax_find_references`
**Description**: Find all references to a symbol
```json
{
  "parameters": {
    "symbol_name": "string",
    "root_directory": "string"
  },
  "returns": {
    "references": [
      {"file": "path", "line": 10, "context": "code snippet"}
    ]
  }
}
```

### 2.3 Workspace Management
- [ ] Implement workspace caching for faster repeated queries
- [ ] Watch for file changes and update cache
- [ ] Handle multi-file projects efficiently

## Phase 3: Advanced Features (Week 5-6)

### 3.1 Type System Support

#### Tool: `luax_get_type_info`
**Description**: Get type information for an expression or variable
```json
{
  "parameters": {
    "file_path": "string",
    "line": "number",
    "column": "number"
  },
  "returns": {
    "type": "int | real | string | ClassName | int[] | ...",
    "is_array": "boolean",
    "is_nullable": "boolean"
  }
}
```

#### Tool: `luax_check_types`
**Description**: Perform type checking on a file
```json
{
  "parameters": {
    "file_path": "string"
  },
  "returns": {
    "errors": [{"line": 10, "message": "Type mismatch: expected int, got string"}]
  }
}
```

### 3.2 Code Generation Support

#### Tool: `luax_get_translation_hints`
**Description**: Get hints for translating LuaX to other languages
```json
{
  "parameters": {
    "file_path": "string",
    "target_language": "csharp | typescript | java | python"
  },
  "returns": {
    "hints": [
      {
        "element": "class ClassName",
        "suggestion": "Maps to C# class with properties",
        "considerations": ["Handle datetime type conversion"]
      }
    ]
  }
}
```

### 3.3 Language Context & Resources

#### Resource: `luax://intro`
**Description**: Introduction to LuaX - what it is, why use it, cross-compilation goals
```json
{
  "uri": "luax://intro",
  "mimeType": "text/markdown",
  "content": "LuaX overview, purpose, and design principles"
}
```

#### Resource: `luax://grammar`
**Description**: Complete grammar reference
```json
{
  "uri": "luax://grammar",
  "mimeType": "text/markdown"
}
```

#### Resource: `luax://stdlib`
**Description**: Standard library reference (file, string, buffer, assert, etc.)
```json
{
  "uri": "luax://stdlib/{category}",
  "categories": ["file", "string", "buffer", "assert", "csvParser", "all"]
}
```

#### Resource: `luax://examples/{category}`
**Description**: Code examples and patterns
```json
{
  "uri": "luax://examples/basic-class",
  "categories": [
    "basic-class",
    "inheritance",
    "exception-handling",
    "arrays",
    "custom-cast",
    "package-structure",
    "constructor-patterns",
    "attributes"
  ]
}
```

#### Resource: `luax://best-practices`
**Description**: LuaX coding best practices and conventions
```json
{
  "uri": "luax://best-practices",
  "mimeType": "text/markdown",
  "content": "Naming conventions, design patterns, portability guidelines"
}
```

#### Resource: `luax://project/summary`
**Description**: Project statistics and summary
```json
{
  "total_classes": 25,
  "total_functions": 150,
  "packages": ["PackageName1", "PackageName2"],
  "dependencies": "class dependency graph"
}
```

### 3.4 Attribute/Annotation Support

LuaX supports attributes like `@DocBrief`, `@DocDescription`, `@Cast`, `@DocInclude`, etc.

#### Tool: `luax_get_attributes`
**Description**: Get all attributes for a symbol
```json
{
  "parameters": {
    "file_path": "string",
    "symbol_name": "string (class, method, or property name)"
  },
  "returns": {
    "attributes": [
      {
        "name": "@DocBrief",
        "parameters": ["This is a description"]
      }
    ]
  }
}
```

#### Tool: `luax_list_attribute_usage`
**Description**: Find all uses of a specific attribute in the project
```json
{
  "parameters": {
    "attribute_name": "string (e.g., '@Cast', '@DocInclude')",
    "root_directory": "string"
  },
  "returns": {
    "usages": [
      {
        "file": "path/to/file.luax",
        "symbol": "ClassName",
        "line": 5
      }
    ]
  }
}
```

### 3.5 Cross-Compilation Support

Since LuaX is designed for cross-compilation to C#, Java, JavaScript, Go, Python:

#### Tool: `luax_check_portability`
**Description**: Analyze code for portability issues
```json
{
  "parameters": {
    "file_path": "string"
  },
  "returns": {
    "portability_score": "int (0-100)",
    "issues": [
      {
        "line": 10,
        "severity": "warning",
        "message": "Custom cast may need special handling in Java",
        "element": "class MyClass"
      }
    ],
    "recommendations": ["Consider using standard types for better portability"]
  }
}
```

#### Tool: `luax_get_translation_summary`
**Description**: Get high-level translation guidance for a file
```json
{
  "parameters": {
    "file_path": "string",
    "target_language": "csharp | java | javascript | go | python"
  },
  "returns": {
    "summary": "This file contains 3 classes that will translate to...",
    "class_mappings": [
      {
        "luax_class": "MyClass",
        "target_equivalent": "C# class with properties",
        "notes": "datetime property needs conversion helper"
      }
    ],
    "required_runtime": ["datetime helper", "exception handling"]
  }
}
```

### 3.6 Package Support

#### Tool: `luax_get_package_info`
**Description**: Get information about packages in the project
```json
{
  "parameters": {
    "root_directory": "string"
  },
  "returns": {
    "packages": [
      {
        "name": "PackageName",
        "classes": ["Class1", "Class2"],
        "file": "path/to/package.luax"
      }
    ]
  }
}
```

#### Tool: `luax_find_package`
**Description**: Find which package a class belongs to
```json
{
  "parameters": {
    "class_name": "string",
    "root_directory": "string"
  },
  "returns": {
    "package_name": "string or null",
    "file_path": "string"
  }
}
```

### 3.7 Documentation Extraction

#### Tool: `luax_get_documentation`
**Description**: Extract documentation from @Doc* attributes
```json
{
  "parameters": {
    "symbol_name": "string",
    "file_path": "optional string"
  },
  "returns": {
    "brief": "string",
    "description": ["line1", "line2"],
    "parameters": [
      {"name": "a", "description": "a description"}
    ],
    "return_description": "string"
  }
}
```

### 3.8 Code Generation/Scaffolding

#### Tool: `luax_generate_class_template`
**Description**: Generate a complete class template
```json
{
  "parameters": {
    "class_name": "string",
    "parent_class": "optional string",
    "package_name": "optional string",
    "include_constructor": "boolean (default true)",
    "visibility": "public | internal | private"
  },
  "returns": {
    "code": "complete class code ready to save"
  }
}
```

#### Tool: `luax_generate_method_stub`
**Description**: Generate method signature with empty body
```json
{
  "parameters": {
    "method_name": "string",
    "parameters": [{"name": "string", "type": "string"}],
    "return_type": "string",
    "is_static": "boolean",
    "visibility": "public | internal | private"
  },
  "returns": {
    "code": "function methodName(param: type) : returnType\nend"
  }
}
```

#### Tool: `luax_add_override_method`
**Description**: Generate override method stub from parent class
```json
{
  "parameters": {
    "class_name": "string",
    "method_name": "string",
    "file_path": "string"
  },
  "returns": {
    "code": "override method code",
    "parent_signature": "original method signature"
  }
}
```

#### Tool: `luax_generate_docs_template`
**Description**: Generate @Doc* attribute template for a symbol
```json
{
  "parameters": {
    "symbol_type": "class | method | property",
    "symbol_name": "string",
    "include_parameters": "boolean (for methods)"
  },
  "returns": {
    "template": "@DocBrief(\"\")\n@DocDescription(\"\")\n..."
  }
}
```

### 3.9 Semantic Analysis

#### Tool: `luax_analyze_inheritance_chain`
**Description**: Analyze complete inheritance chain with method resolution
```json
{
  "parameters": {
    "class_name": "string",
    "root_directory": "string"
  },
  "returns": {
    "chain": ["object", "BaseClass", "ChildClass"],
    "method_resolution": [
      {"method": "doSomething", "defined_in": "BaseClass"}
    ],
    "overridden_methods": [
      {
        "method": "onCreate",
        "parent_class": "BaseClass",
        "child_class": "ChildClass"
      }
    ]
  }
}
```

#### Tool: `luax_check_constructor_chain`
**Description**: Verify constructor initialization chain
```json
{
  "parameters": {
    "class_name": "string"
  },
  "returns": {
    "constructor_chain": ["object", "BaseClass()", "ChildClass()"],
    "warnings": ["BaseClass constructor initializes property before ChildClass"]
  }
}
```

#### Tool: `luax_explain_code`
**Description**: Explain what code does in LuaX context
```json
{
  "parameters": {
    "file_path": "string",
    "line_start": "int",
    "line_end": "int"
  },
  "returns": {
    "explanation": "natural language explanation",
    "features_used": ["inheritance", "custom cast", "exception handling"],
    "related_symbols": ["BaseClass", "helper method"]
  }
}
```

## Phase 4: Polish & Deployment (Week 7-8)

### 4.1 Documentation
- [ ] Write README.md with installation instructions
- [ ] Document all MCP tools with examples
- [ ] Create troubleshooting guide
- [ ] Add contribution guidelines

### 4.2 Configuration
- [ ] Create MCP server configuration file schema
- [ ] Support workspace-specific settings
- [ ] Add configuration for parser options

### 4.3 Distribution
- [ ] Package server as standalone executable (self-contained .NET deployment)
- [ ] Create installation script (PowerShell for Windows, bash for Linux/Mac)
- [ ] Optionally publish as .NET tool: `dotnet tool install -g luax-mcp-server`
- [ ] Add to MCP server registry
- [ ] Create releases for multiple platforms (win-x64, linux-x64, osx-x64)

### 4.4 Claude Code Integration
- [ ] Test with Claude Code MCP configuration
- [ ] Verify all tools are discoverable
- [ ] Create example MCP configuration:

**For global .NET tool installation:**
```json
{
  "mcpServers": {
    "luax": {
      "command": "luax-mcp-server",
      "args": [],
      "env": {}
    }
  }
}
```

**For local executable:**
```json
{
  "mcpServers": {
    "luax": {
      "command": "/path/to/LuaX.Mcp.Server.exe",
      "args": [],
      "env": {}
    }
  }
}
```

**For development/debugging:**
```json
{
  "mcpServers": {
    "luax": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/LuaX.Mcp.Server/LuaX.Mcp.Server.csproj"],
      "env": {}
    }
  }
}
```

## Testing Strategy

### Overview
The MCP server testing strategy uses **xUnit** for both unit and integration tests, following patterns established in the LuaX codebase.

### Test Infrastructure Setup

#### Test Project: `LuaX.Mcp.Server.Test`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- xUnit Framework -->
    <PackageReference Include="xunit" Version="2.4.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.2.0" />

    <!-- Assertion Libraries -->
    <PackageReference Include="FluentAssertions" Version="6.7.0" />

    <!-- Mocking Framework -->
    <PackageReference Include="Moq" Version="4.18.1" />

    <!-- Code Coverage -->
    <PackageReference Include="coverlet.collector" Version="3.1.2" />
    <PackageReference Include="LiquidTestReports.Markdown" Version="1.0.9" />

    <!-- MCP Testing Utilities (if available) -->
    <PackageReference Include="ModelContextProtocol" Version="*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\LuaX.Mcp.Server\LuaX.Mcp.Server.csproj" />
    <ProjectReference Include="..\Luax.Parser\Luax.Parser.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Embed test .luax files as resources -->
    <EmbeddedResource Include="TestSources\**\*.luax" />
  </ItemGroup>
</Project>
```

### Unit Tests

#### 1. Parser Integration Tests
**Namespace**: `LuaX.Mcp.Server.Test.Parser`

**Test Class**: `LuaXParserServiceTest`
```csharp
public class LuaXParserServiceTest
{
    [Fact]
    public void ParseFile_ValidLuaXFile_ReturnsAst()
    {
        // Arrange
        var service = new LuaXParserService();
        var filePath = GetTestFilePath("ClassesTest1.luax");

        // Act
        var result = service.ParseFile(filePath);

        // Assert
        result.Success.Should().BeTrue();
        result.Ast.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("InvalidSyntax.luax", 3, 5)] // line, column
    public void ParseFile_InvalidSyntax_ReturnsErrors(
        string fileName, int expectedLine, int expectedColumn)
    {
        // Arrange & Act & Assert
        var service = new LuaXParserService();
        var filePath = GetTestFilePath(fileName);
        var result = service.ParseFile(filePath);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Line.Should().Be(expectedLine);
        result.Errors[0].Column.Should().Be(expectedColumn);
    }
}
```

#### 2. Tool Implementation Tests
**Namespace**: `LuaX.Mcp.Server.Test.Tools`

**Test Class**: `LuaXParseToolTest`
```csharp
public class LuaXParseToolTest
{
    [Fact]
    public async Task Execute_ValidFile_ReturnsSuccessResponse()
    {
        // Arrange
        var tool = new LuaXParseTool();
        var request = new ToolRequest
        {
            Parameters = new { file_path = GetTestFilePath("ClassesTest1.luax") }
        };

        // Act
        var response = await tool.ExecuteAsync(request);

        // Assert
        response.Success.Should().BeTrue();
        response.Ast.Should().NotBeNull();
        response.Errors.Should().BeEmpty();
    }
}

public class LuaXListSymbolsToolTest
{
    [Theory]
    [InlineData("ClassesTest1.luax", "class", 7)]
    [InlineData("ParseClassMethods.luax", "function", 3)]
    public async Task Execute_FilterByType_ReturnsFilteredSymbols(
        string fileName, string symbolType, int expectedCount)
    {
        // Arrange
        var tool = new LuaXListSymbolsTool();
        var request = new ToolRequest
        {
            Parameters = new
            {
                file_path = GetTestFilePath(fileName),
                symbol_type = symbolType
            }
        };

        // Act
        var response = await tool.ExecuteAsync(request);

        // Assert
        response.Symbols.Should().HaveCount(expectedCount);
        response.Symbols.Should().AllSatisfy(s =>
            s.Type.Should().Be(symbolType));
    }
}
```

#### 3. Symbol Extraction Tests
**Test Class**: `SymbolExtractorTest`
```csharp
public class SymbolExtractorTest
{
    [Fact]
    public void ExtractClasses_FromAst_ReturnsAllClasses()
    {
        // Arrange
        var ast = ParseTestFile("ClassesTest1.luax");
        var extractor = new SymbolExtractor();

        // Act
        var classes = extractor.ExtractClasses(ast);

        // Assert
        classes.Should().HaveCount(7);
        classes.Should().Contain(c => c.Name == "a");
        classes.Should().Contain(c => c.Name == "g");
    }

    [Fact]
    public void ExtractMethods_WithInheritance_ResolvesCorrectly()
    {
        // Arrange
        var ast = ParseTestFile("OverridingOK.luax");
        var extractor = new SymbolExtractor();

        // Act
        var classes = extractor.ExtractClasses(ast);
        var parentClass = classes.First(c => c.Name == "parent");
        var childClass = classes.First(c => c.Name == "child");

        // Assert
        parentClass.Methods.Should().Contain(m => m.Name == "doWork");
        childClass.Methods.Should().Contain(m => m.Name == "doWork");
        childClass.Methods.First(m => m.Name == "doWork")
            .IsOverride.Should().BeTrue();
    }
}
```

#### 4. Attribute Processing Tests
**Test Class**: `AttributeProcessorTest`
```csharp
public class AttributeProcessorTest
{
    [Fact]
    public void ExtractDocumentation_FromAttributes_ReturnsDocInfo()
    {
        // Arrange
        var ast = ParseTestFile("DocTest1.luax");
        var processor = new AttributeProcessor();
        var classNode = ast.Classes[0];

        // Act
        var doc = processor.ExtractDocumentation(classNode);

        // Assert
        doc.Brief.Should().Be("This is brief for class 1");
        doc.Description.Should().HaveCount(2);
        doc.Description[0].Should()
            .Be("This is the first line of the description");
    }

    [Theory]
    [InlineData("@DocBrief", "Brief description", 1)]
    [InlineData("@Cast", null, 0)]
    public void FindAttributeUsage_ByName_ReturnsOccurrences(
        string attributeName, string expectedParam, int paramCount)
    {
        // Test attribute search functionality
    }
}
```

#### 5. Resource Provider Tests
**Test Class**: `ResourceProviderTest`
```csharp
public class ResourceProviderTest
{
    [Theory]
    [InlineData("luax://intro")]
    [InlineData("luax://grammar")]
    [InlineData("luax://stdlib")]
    public async Task GetResource_ValidUri_ReturnsContent(string uri)
    {
        // Arrange
        var provider = new LuaXResourceProvider();

        // Act
        var content = await provider.GetResourceAsync(uri);

        // Assert
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetResource_ExampleCategory_ReturnsCodeExample()
    {
        // Arrange
        var provider = new LuaXResourceProvider();

        // Act
        var content = await provider.GetResourceAsync(
            "luax://examples/basic-class");

        // Assert
        content.Should().Contain("class");
        content.Should().Contain("end");
    }
}
```

#### 6. Code Generation Tests
**Test Class**: `CodeGeneratorTest`
```csharp
public class CodeGeneratorTest
{
    [Theory]
    [InlineData("MyClass", null, false)]
    [InlineData("MyClass", "BaseClass", true)]
    public void GenerateClassTemplate_WithOptions_ReturnsValidCode(
        string className, string parentClass, bool includeConstructor)
    {
        // Arrange
        var generator = new CodeGenerator();

        // Act
        var code = generator.GenerateClassTemplate(
            className, parentClass, includeConstructor);

        // Assert
        code.Should().Contain($"class {className}");
        if (parentClass != null)
            code.Should().Contain($": {parentClass}");
        if (includeConstructor)
            code.Should().Contain($"function {className}()");
        code.Should().Contain("end");

        // Verify generated code is valid
        var parseResult = ParseString(code);
        parseResult.Success.Should().BeTrue();
    }
}
```

### Integration Tests

#### 1. MCP Protocol Tests
**Namespace**: `LuaX.Mcp.Server.Test.Integration`

**Test Class**: `McpServerIntegrationTest`
```csharp
public class McpServerIntegrationTest : IAsyncLifetime
{
    private McpServer _server;
    private McpClient _client;

    public async Task InitializeAsync()
    {
        // Set up server and client connected via stdio or test transport
        _server = new McpServer();
        await _server.StartAsync();

        _client = new McpClient();
        await _client.ConnectAsync(_server);
    }

    [Fact]
    public async Task ListTools_ServerRunning_ReturnsAllTools()
    {
        // Act
        var tools = await _client.ListToolsAsync();

        // Assert
        tools.Should().Contain(t => t.Name == "luax_parse");
        tools.Should().Contain(t => t.Name == "luax_validate");
        tools.Should().Contain(t => t.Name == "luax_list_symbols");
    }

    [Fact]
    public async Task InvokeTool_ParseFile_ReturnsValidResponse()
    {
        // Arrange
        var request = new
        {
            file_path = GetTestFilePath("ClassesTest1.luax")
        };

        // Act
        var response = await _client.InvokeToolAsync(
            "luax_parse", request);

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
    }

    public async Task DisposeAsync()
    {
        await _client?.DisconnectAsync();
        await _server?.StopAsync();
    }
}
```

#### 2. Multi-File Project Tests
**Test Class**: `MultiFileProjectTest`
```csharp
public class MultiFileProjectTest
{
    [Fact]
    public async Task GetHierarchy_MultipleFiles_BuildsCompleteTree()
    {
        // Arrange
        var tool = new LuaXGetHierarchyTool();
        var projectPath = GetTestProjectPath("TestProject1");

        // Act
        var hierarchy = await tool.ExecuteAsync(new
        {
            root_directory = projectPath
        });

        // Assert
        hierarchy.Classes.Should().NotBeEmpty();
        hierarchy.Classes["ChildClass"]
            .Parent.Should().Be("ParentClass");
    }

    [Fact]
    public async Task FindReferences_AcrossFiles_FindsAllOccurrences()
    {
        // Test cross-file reference finding
    }
}
```

#### 3. Performance Tests
**Test Class**: `PerformanceTest`
```csharp
public class PerformanceTest
{
    [Fact]
    public async Task ParseFile_TypicalFile_CompletesWithin100ms()
    {
        // Arrange
        var tool = new LuaXParseTool();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await tool.ExecuteAsync(new
        {
            file_path = GetTestFilePath("ClassesTest1.luax")
        });
        stopwatch.Stop();

        // Assert
        result.Success.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public async Task ScanProject_100Files_CompletesWithin2Seconds()
    {
        // Arrange
        var tool = new LuaXGetHierarchyTool();
        var projectPath = GetLargeTestProject(); // 100+ files
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await tool.ExecuteAsync(new
        {
            root_directory = projectPath
        });
        stopwatch.Stop();

        // Assert
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }
}
```

#### 4. Error Recovery Tests
**Test Class**: `ErrorRecoveryTest`
```csharp
public class ErrorRecoveryTest
{
    [Fact]
    public async Task ParseFile_FileNotFound_ReturnsGracefulError()
    {
        // Arrange
        var tool = new LuaXParseTool();

        // Act
        var result = await tool.ExecuteAsync(new
        {
            file_path = "/nonexistent/file.luax"
        });

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not found");
        // Should not throw exception
    }

    [Fact]
    public async Task ProcessLargeFile_OutOfMemory_HandlesGracefully()
    {
        // Test memory limit handling
    }
}
```

#### 5. WebTS-API Production Codebase Tests
**Test Class**: `WebTsApiIntegrationTest`
```csharp
public class WebTsApiIntegrationTest
{
    private const string WebTsApiPath =
        "/mnt/d/develop/work/projects/WebTS-API/core";

    [Fact]
    public async Task ScanWebTsApi_AllFiles_CompletesSuccessfully()
    {
        // Arrange
        var tool = new LuaXGetHierarchyTool();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await tool.ExecuteAsync(new
        {
            root_directory = WebTsApiPath
        });
        stopwatch.Stop();

        // Assert
        result.Classes.Should().HaveCountGreaterThan(500);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("GetAccountsCommand")]
    [InlineData("GetAccountsCommandBuilder")]
    [InlineData("CommandEnvironment")]
    public async Task FindDefinition_WebTsApiClasses_FindsCorrectly(
        string className)
    {
        // Arrange
        var tool = new LuaXFindDefinitionTool();

        // Act
        var result = await tool.ExecuteAsync(new
        {
            symbol_name = className,
            root_directory = WebTsApiPath
        });

        // Assert
        result.Found.Should().BeTrue();
        result.FilePath.Should().Contain(className);
    }

    [Fact]
    public async Task AnalyzeInheritance_CommandClasses_BuildsHierarchy()
    {
        // Test inheritance analysis on Command pattern classes
        var tool = new LuaXAnalyzeInheritanceChainTool();

        var result = await tool.ExecuteAsync(new
        {
            class_name = "GetAccountsCommand",
            root_directory = WebTsApiPath
        });

        result.Chain.Should().Contain("Command");
    }

    [Fact]
    public async Task ExtractDocumentation_ProductionCode_ParsesAttributes()
    {
        // Test @Doc* attribute extraction from real production code
    }
}
```

### End-to-End Tests

#### Test Class: `ClaudeCodeIntegrationTest`
```csharp
[Collection("E2E")]
public class ClaudeCodeIntegrationTest
{
    // These tests require Claude Code to be running
    // and configured with the MCP server

    [Fact(Skip = "Requires Claude Code instance")]
    public async Task ClaudeCode_ListsTools_ShowsLuaXTools()
    {
        // Test actual Claude Code integration
    }

    [Fact(Skip = "Requires Claude Code instance")]
    public async Task ClaudeCode_WorksWithWebTsApi_NavigatesSuccessfully()
    {
        // Open WebTS-API project in Claude Code
        // Verify MCP tools work on production codebase
    }
}
```

### Test Data Organization

```
McpServer/LuaX.Mcp.Server.Test/
├── TestSources/
│   ├── Valid/
│   │   ├── ClassesTest1.luax
│   │   ├── InheritanceTest.luax
│   │   ├── AttributesTest.luax
│   │   └── PackageTest.luax
│   ├── Invalid/
│   │   ├── SyntaxError.luax
│   │   ├── TypeMismatch.luax
│   │   └── UndefinedSymbol.luax
│   └── Projects/
│       ├── SmallProject/ (5-10 files)
│       ├── MediumProject/ (20-50 files)
│       └── LargeProject/ (100+ files)
├── Fixtures/
│   ├── TestFileProvider.cs
│   └── McpServerFixture.cs
└── Utilities/
    ├── AssertionExtensions.cs
    └── TestHelpers.cs

# Also use existing test files from parent directory:
../Luax.Interpreter.Test/TestSources/*.luax
../Luax.Parser.Test/TestSources/*.luax

# Production codebase for real-world testing:
/mnt/d/develop/work/projects/WebTS-API/core/
```

### Test Utilities

**File**: `TestHelpers.cs`
```csharp
public static class TestHelpers
{
    public static string GetTestFilePath(string fileName)
    {
        return Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestSources", "Valid", fileName);
    }

    public static LuaXClassCollection ParseTestFile(string fileName)
    {
        var parser = new LuaXParser();
        var result = parser.Parse(GetTestFilePath(fileName));
        return result.Root;
    }
}
```

### Existing Test Resources

**From LuaX Repository** (relative to `/mnt/d/develop/work/tools/luax/`):
- `Luax.Interpreter.Test/TestSources/*.luax` - ~30 files
  - Valid runtime tests (classes, inheritance, exceptions, loops)
- `Luax.Parser.Test/TestSources/*.luax` - ~40 files
  - Parser tests (valid and invalid syntax, edge cases)
- `LuaX.Apps.Test/TestSources/*.luax` - Documentation tests
  - Attribute and documentation extraction

**Production Codebase** (`/mnt/d/develop/work/projects/WebTS-API/core/`):
- **1,368 LuaX source files** - substantial real-world project
- **Project Structure**:
  - `core.common/source/` - Main business logic
    - `commands/` - Command pattern implementations
    - `models/` - Data models and entities
    - `services/` - Business services
  - `core.common.test/` - Unit tests
  - `core.common.integration.test/` - Integration tests
  - `core.common.mock/` - Mock implementations
  - `samples/` - Example code
- **Characteristics**:
  - Real-world project structure with organized folders
  - Complex inheritance hierarchies (Command, Builder patterns)
  - Production-level documentation attributes
  - Package organization
  - Consistent naming conventions (e.g., `GetAccountsCommand`, `GetAccountsCommandBuilder`)
- **Use Cases for MCP Testing**:
  - Large-scale integration testing (1,300+ files)
  - Performance benchmarking at scale
  - Real-world validation of all MCP tools
  - Testing project-level features (hierarchy, references, packages)
  - Validating cross-file navigation and search
  - Testing attribute extraction on production code
  - Benchmark workspace caching performance

### Test Execution

**Local Development**:
```bash
dotnet test LuaX.Mcp.Server.Test/LuaX.Mcp.Server.Test.csproj
```

**With Code Coverage**:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**CI/CD Pipeline** (GitHub Actions):
```yaml
- name: Run Tests
  run: dotnet test --configuration Release --logger "trx;LogFileName=test-results.trx"

- name: Generate Coverage Report
  run: dotnet test /p:CollectCoverage=true /p:CoverletOutput=./coverage/
```

### Coverage Goals
- **Unit Tests**: 80%+ code coverage
- **Integration Tests**: Cover all MCP tools
- **Performance Tests**: All performance targets validated
- **Error Scenarios**: All error paths tested

## Performance Targets

- **Parse file**: < 100ms for typical file
- **Validate file**: < 150ms
- **Find symbol**: < 50ms with cache
- **Project scan**:
  - < 2s for 100 files (typical project)
  - < 5s for 1,368 files (WebTS-API scale)
- **Memory**:
  - < 100MB for typical project (10-100 files)
  - < 500MB for large project (1,000+ files like WebTS-API)
- **Workspace caching**:
  - First scan: Allow full processing time
  - Subsequent queries: < 10ms for cached symbol lookups
  - Cache invalidation: < 50ms per modified file

## Success Criteria

### Phase 1 (MVP)
- ✓ Claude can parse and validate LuaX files
- ✓ Claude can list all symbols in a file
- ✓ Basic error reporting works
- ✓ Server runs reliably via stdio

### Phase 2
- ✓ Claude can navigate to definitions
- ✓ Claude understands class hierarchies
- ✓ Multi-file projects work smoothly

### Phase 3
- ✓ Type checking provides useful feedback
- ✓ Translation hints help with cross-compilation
- ✓ Performance meets targets

### Phase 4
- ✓ Documentation complete
- ✓ Easy installation process
- ✓ Works in production Claude Code sessions

## Future Enhancements (Post-MVP)

- **Code formatting**: `luax_format` tool with style configuration
- **Refactoring support**:
  - Rename symbol across project
  - Extract method
  - Move class to package
  - Extract interface/base class
- **LSP compatibility**: Extend to full Language Server Protocol for IDE integration
- **Interactive code generation**:
  - `luax_convert_from_csharp`: Convert C# class to LuaX
  - `luax_convert_to_csharp`: Generate C# from LuaX
- **Advanced portability**:
  - `luax_simulate_translation`: Preview exact translation output
  - Platform-specific optimization suggestions
- **Code completion**: Context-aware suggestions for members, methods
- **Semantic highlighting**: Rich syntax information with cross-compilation context
- **Call hierarchy**: Show caller/callee relationships
- **Dependency analysis**: Visualize class dependencies and coupling
- **Test generation**: Generate test stubs for classes/methods
- **Performance hints**: Suggest optimizations for target platforms

## Resources & References

### MCP Documentation
- MCP Specification: https://spec.modelcontextprotocol.io/
- MCP C# SDK: https://github.com/modelcontextprotocol/csharp-sdk
- NuGet Package: https://www.nuget.org/packages/ModelContextProtocol

### LuaX Resources
- **Grammar file**: `/grammar/luax.gram` (relative to luax root)
- **Existing parser**: `Luax.Parser` project (uses Hime)
- **Test cases**:
  - `/Luax.Interpreter.Test/TestSources/` (~30 files)
  - `/Luax.Parser.Test/TestSources/` (~40 files)
  - `/LuaX.Apps.Test/TestSources/` (documentation tests)
- **Production codebase**: `/mnt/d/develop/work/projects/WebTS-API/core/`
  - Real-world LuaX project with hundreds of source files
  - Use for integration testing and validation

### Example MCP Servers
- Look at existing MCP servers for reference implementations
- Study Claude Code's built-in MCP integrations

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| ModelContextProtocol SDK API changes | Low | Official SDK is stable; pin to specific version |
| Parser performance issues | Medium | Implement caching, lazy parsing, async operations |
| Complex type system | Medium | Start simple, iterate based on usage |
| Installation complexity | Low | Create install scripts, publish as .NET tool, clear docs |
| Protocol version changes | Low | Track MCP spec updates, SDK handles protocol versioning |
| .NET runtime dependency | Medium | Use self-contained deployment or document .NET requirement |

## Timeline Summary

- **Week 1-2**: Foundation + MVP tools (Phase 1)
- **Week 3-4**: Navigation + project features (Phase 2)
- **Week 5-6**: Advanced features (Phase 3)
- **Week 7**: Polish + deployment (Phase 4)

**Total Estimated Time**: 7 weeks for full implementation

**MVP Delivery**: 2 weeks (Phase 1 only)

## Next Steps

1. ✅ Technology stack decided: C# with ModelContextProtocol NuGet package
2. Set up development environment
   - Install latest .NET SDK (if needed)
   - Verify access to existing LuaX parser code
3. Create project repository structure
   - Add new `LuaX.Mcp.Server` console project to solution
   - Add ModelContextProtocol NuGet package
   - Reference necessary LuaX projects
4. Implement Phase 1.1-1.2 (foundation)
   - Set up MCP server with stdio transport
   - Integrate Hime parser
   - Create basic tool infrastructure
5. Build first tool (`luax_parse`) end-to-end
   - Implement parse functionality
   - Test locally with MCP client
6. Test with Claude Code
   - Configure MCP server in Claude Code
   - Verify tool discovery and execution

---

## Summary: Beyond Grammar - Comprehensive LuaX Support

This MCP server goes far beyond basic syntax parsing to provide:

### 1. **Language Purpose & Context**
- Resources explaining LuaX's cross-compilation goals
- Best practices for portable code
- Standard library reference
- Code examples and patterns

### 2. **Attribute System Support**
- Extract and query @Doc* documentation attributes
- Find attribute usage across projects
- Generate documentation templates

### 3. **Cross-Compilation Awareness**
- Portability checking
- Translation guidance for target languages (C#, Java, JavaScript, Go, Python)
- Platform-specific recommendations

### 4. **Code Generation**
- Class templates with proper structure
- Method stubs and override generation
- Documentation attribute scaffolding
- Respect LuaX conventions and patterns

### 5. **Semantic Understanding**
- Full inheritance chain analysis
- Constructor initialization order
- Method resolution and overriding
- Package organization

### 6. **Intent-Based Assistance**
- Code explanation in LuaX context
- Identify language features used
- Suggest refactoring opportunities
- Relate code to LuaX design principles

### Key Differentiators from Simple Parser:

| Basic Grammar Parser | Enhanced MCP Server |
|---------------------|---------------------|
| Validates syntax | Understands purpose and intent |
| Lists symbols | Explains relationships and semantics |
| Parses AST | Generates code following patterns |
| Reports errors | Suggests improvements and alternatives |
| Structure-only | Context-aware with cross-compilation knowledge |

### Implementation Priority:

1. **Phase 1**: Core parsing + basic tools (MVP)
2. **Phase 2**: Navigation + project structure
3. **Phase 3**:
   - Resources (intro, stdlib, examples) - **High priority**
   - Attribute support - **High priority**
   - Code generation - **Medium priority**
   - Cross-compilation helpers - **Medium priority**
   - Semantic analysis - **Nice to have**

---

**Document Version**: 2.0
**Created**: 2025-11-03
**Last Updated**: 2025-11-04
