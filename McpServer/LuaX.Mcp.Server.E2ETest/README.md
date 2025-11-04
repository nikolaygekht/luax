# LuaX MCP Server - End-to-End Tests

This project contains E2E tests that verify the MCP server works correctly by launching it as a subprocess and communicating via the MCP protocol over stdio.

## How It Works

The E2E tests use **`StdioClientTransport`** from the ModelContextProtocol SDK to:
1. Launch the MCP server as a subprocess (`dotnet LuaX.Mcp.Server.dll`)
2. Connect to it via stdio (standard input/output)
3. Send MCP protocol requests (list tools, call tools, etc.)
4. Verify the responses

This simulates exactly how Claude Code or other MCP clients would interact with the server.

## Architecture

### McpServerFixture
An xUnit collection fixture that manages the server lifecycle:
- **Starts once** before all tests in the collection
- **Shared** across all tests (efficient)
- **Automatically discovers** server path based on test assembly location
- **Supports any build configuration** (Debug/Release) and target framework
- **Properly disposes** resources after all tests complete

### Path Resolution Strategy

The fixture automatically determines the server executable path using:
```csharp
var testAssemblyPath = typeof(McpServerFixture).Assembly.Location;
// Extract configuration (Debug/Release) and framework (net8.0) from path
// Build server path: ../LuaX.Mcp.Server/bin/{Configuration}/{Framework}/
```

**Benefits:**
- ✅ Works with Debug and Release builds
- ✅ Adapts to future .NET versions automatically
- ✅ No hardcoded paths
- ✅ Reliable `Assembly.Location` instead of `AppDomain.BaseDirectory`

## Running Tests

### Debug Build (default)
```bash
cd LuaX.Mcp.Server.E2ETest
dotnet test
```

### Release Build
```bash
cd LuaX.Mcp.Server.E2ETest
dotnet test -c Release
```

**Note:** The server must be built in the same configuration before running E2E tests.

## Test Cases

All tests use the shared `McpServerFixture`:

1. **`Server_Starts_Successfully`** - Verifies the server process starts and client connects
2. **`ListTools_ReturnsAvailableTools`** - Tests tool discovery via MCP protocol
3. **`CallTool_LuaXInfo_ReturnsServerInfo`** - Tests tool invocation with parameters
4. **`Server_RespondsToPing`** - Verifies server is responsive
5. **`ListTools_ToolsHaveDescriptions`** - Validates tool metadata

## Test Collection

Tests use xUnit's collection fixture pattern:
```csharp
[Collection("E2E Server Collection")]
public class ServerE2ETest
{
    private readonly McpServerFixture _fixture;

    public ServerE2ETest(McpServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MyTest()
    {
        var tools = await _fixture.Client.ListToolsAsync();
        // ...
    }
}
```

This ensures:
- Server starts only once
- All tests share the same server instance
- Faster test execution
- Proper cleanup after all tests

## Dependencies

- **xUnit**: Test framework
- **FluentAssertions 7.x**: Readable assertions
- **ModelContextProtocol SDK**: MCP client functionality

## Troubleshooting

### "Server executable not found"
Make sure the server is built before running E2E tests:
```bash
dotnet build ../LuaX.Mcp.Server
dotnet test
```

### Tests hang or timeout
The server might not be starting correctly. Check:
- Server builds without errors
- No port conflicts (stdio doesn't use ports, but check server logs)
- Server logs in `logs/luax-mcp-server-*.log`

### Different configuration
If running tests in Release but server is only built in Debug:
```bash
# Build both in the same configuration
dotnet build ../LuaX.Mcp.Server -c Release
dotnet test -c Release
```
