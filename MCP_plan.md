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
- [ ] Create new .NET console project: `LuaX.Mcp.Server/`
- [ ] Add NuGet package: `ModelContextProtocol`
- [ ] Reference existing LuaX projects (parser, interpreter)
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

### Unit Tests
- Parser integration tests
- AST transformation tests
- Symbol extraction tests
- Type checking logic tests

### Integration Tests
- Full MCP protocol communication tests
- Multi-file project analysis
- Performance tests with large codebases
- Error recovery scenarios

### User Acceptance Tests
- Test with Claude Code on real LuaX projects
- Verify code navigation works correctly
- Test validation and error reporting
- Check performance with workspace caching

### Test Projects
Use existing test files from:
- `/Luax.Interpreter.Test/TestSources/*.luax`
- `/LuaX.Apps.Test/TestSources/*.luax`

## Performance Targets

- Parse file: < 100ms for typical file
- Validate file: < 150ms
- Find symbol: < 50ms with cache
- Project scan: < 2s for 100 files
- Memory: < 100MB for typical project

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
- Grammar file: `/grammar/luax.gram`
- Existing parser: (identify location in codebase)
- Test cases: `/Luax.Interpreter.Test/TestSources/`

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
