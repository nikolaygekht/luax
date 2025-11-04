# LuaX MCP Server

Model Context Protocol (MCP) server for LuaX language support in Claude Code and other MCP clients.

## Project Structure

```
McpServer/
├── MCP_plan.md                      # Detailed development plan
├── README.md                         # This file
├── LuaX.Mcp.Server/                 # Main server project
│   ├── LuaX.Mcp.Server.csproj       # Project file
│   ├── Program.cs                    # Entry point with Serilog and MCP setup
│   ├── Tools/                       # MCP tool implementations
│   │   └── LuaXInfoTool.cs          # Placeholder info tool
│   ├── Services/                    # Future: Parser, symbol extraction services
│   ├── Resources/                   # Future: luax:// resource providers
│   └── Models/                      # Future: Data models
├── LuaX.Mcp.Server.Test/           # Unit test project
│   ├── LuaX.Mcp.Server.Test.csproj  # Test project file
│   ├── Tools/                        # Tool tests
│   │   └── LuaXInfoToolTest.cs      # Tests for info tool
│   ├── Integration/                 # Future: Integration tests
│   ├── TestSources/                 # Test .luax files
│   │   ├── Valid/
│   │   ├── Invalid/
│   │   └── Projects/
│   ├── Fixtures/                    # Test fixtures
│   └── Utilities/                   # Test utilities
└── LuaX.Mcp.Server.E2ETest/        # End-to-end test project
    ├── LuaX.Mcp.Server.E2ETest.csproj  # E2E test project
    ├── Fixtures/
    │   └── McpServerFixture.cs      # Server lifecycle management
    └── Tests/
        └── ServerE2ETest.cs         # E2E tests via StdioClientTransport
```

## Technologies

- **.NET 8.0**: Target framework
- **ModelContextProtocol SDK**: v0.4.0-preview.3
- **Serilog**: Logging to rolling files
- **xUnit**: Testing framework
- **FluentAssertions**: Assertion library (v7.x)
- **Moq**: Mocking framework

## Build and Run

### Build
```bash
cd McpServer
dotnet build
```

### Run Server
```bash
cd LuaX.Mcp.Server
dotnet run
```

### Run Tests

**Unit Tests:**
```bash
cd LuaX.Mcp.Server.Test
dotnet test
```

**E2E Tests (launches server and tests via MCP client):**
```bash
cd LuaX.Mcp.Server.E2ETest
dotnet test
```

## Current Status

✅ **Phase 0: Project Skeleton (Complete)**
- Project structure created
- .csproj files configured with correct dependencies
- Basic Program.cs with Serilog and MCP ServerBuilder
- Placeholder tool (luax_info) to verify setup
- Unit test infrastructure with xUnit and FluentAssertions 7.x
- **E2E test infrastructure with StdioClientTransport**
- All projects build successfully
- Unit tests pass (2/2) ✅
- **E2E tests pass (5/5) ✅**

📋 **Next Steps (from MCP_plan.md):**
- Implement Phase 1.2: Core Infrastructure
  - Integrate Hime parser
  - Create AST wrapper
  - File system utilities
- Implement Phase 1.3: Basic Tools (MVP)
  - luax_parse
  - luax_validate
  - luax_list_symbols

## Logging

Logs are written to `logs/luax-mcp-server-YYYYMMDD.log` with 7-day retention.

## Test Resources

The project has access to:
- **~70 existing .luax test files** from LuaX.Parser.Test and LuaX.Interpreter.Test
- **1,368 .luax files** from WebTS-API production codebase at `/mnt/d/develop/work/projects/WebTS-API/core/`

## Development Notes

- MCP tools use `[McpServerToolType]` and `[McpServerTool]` attributes
- Tools are auto-discovered via `.WithToolsFromAssembly()`
- Server communicates via stdio transport
- FluentAssertions version locked to 7.x (avoid v8) per requirement

## References

- [MCP Plan](./MCP_plan.md) - Detailed development plan with phases
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP Specification](https://spec.modelcontextprotocol.io/)
