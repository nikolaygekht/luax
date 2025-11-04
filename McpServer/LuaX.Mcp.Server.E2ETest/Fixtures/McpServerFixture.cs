using ModelContextProtocol.Client;
using Xunit;

namespace LuaX.Mcp.Server.E2ETest.Fixtures;

/// <summary>
/// xUnit fixture that manages the lifecycle of the MCP server for E2E tests.
/// The server is started once and shared across all tests in the collection.
/// </summary>
public class McpServerFixture : IAsyncLifetime
{
    private StdioClientTransport? _transport;
    private McpClient? _client;

    /// <summary>
    /// Gets the MCP client connected to the server via stdio.
    /// </summary>
    public McpClient Client
    {
        get
        {
            if (_client == null)
                throw new InvalidOperationException("Client not initialized. Call InitializeAsync first.");
            return _client;
        }
    }

    /// <summary>
    /// Gets the path to the server executable.
    /// Automatically detects the build configuration (Debug/Release) and target framework
    /// from the current test assembly location.
    /// </summary>
    private string GetServerExecutablePath()
    {
        // Get the current test assembly location
        // Example: .../LuaX.Mcp.Server.E2ETest/bin/Debug/net8.0/LuaX.Mcp.Server.E2ETest.dll
        var testAssemblyPath = typeof(McpServerFixture).Assembly.Location;
        var testAssemblyDir = Path.GetDirectoryName(testAssemblyPath)
            ?? throw new InvalidOperationException($"Unable to get directory from assembly location: {testAssemblyPath}");

        // Extract build configuration (Debug or Release) and target framework (net8.0, etc.)
        var testBinPath = new DirectoryInfo(testAssemblyDir);

        // Path structure: .../bin/{Configuration}/{TargetFramework}/
        var targetFramework = testBinPath.Name;              // e.g., "net8.0"
        var configuration = testBinPath.Parent?.Name;        // e.g., "Debug" or "Release"

        if (string.IsNullOrEmpty(configuration) || string.IsNullOrEmpty(targetFramework))
        {
            throw new InvalidOperationException(
                $"Unable to determine build configuration and target framework from path: {testAssemblyDir}");
        }

        // Build the server path using the same configuration and framework
        var serverPath = Path.Combine(
            testAssemblyDir,
            "..", "..", "..", "..",  // Go up to McpServer folder
            "LuaX.Mcp.Server",
            "bin",
            configuration,
            targetFramework,
            "LuaX.Mcp.Server.dll");

        var fullPath = Path.GetFullPath(serverPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Server executable not found at: {fullPath}. " +
                $"Expected configuration: {configuration}, Target framework: {targetFramework}. " +
                "Make sure LuaX.Mcp.Server is built before running E2E tests.");
        }

        return fullPath;
    }

    /// <summary>
    /// Initializes the server and client before any tests run.
    /// </summary>
    public async Task InitializeAsync()
    {
        var serverPath = GetServerExecutablePath();

        // Create transport that launches the server via dotnet
        _transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "LuaX-MCP-Server",
            Command = "dotnet",
            Arguments = [serverPath]
        });

        // Create and initialize the client
        _client = await McpClient.CreateAsync(_transport);
    }

    /// <summary>
    /// Cleans up the server and client after all tests complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_client != null)
        {
            // Disposing the client should clean up the transport
            await _client.DisposeAsync();
            _client = null;
        }

        _transport = null;

        // Give the server process time to shut down cleanly
        await Task.Delay(500);
    }
}

/// <summary>
/// xUnit collection definition for E2E tests that share the same server instance.
/// </summary>
[CollectionDefinition("E2E Server Collection")]
public class McpServerCollection : ICollectionFixture<McpServerFixture>
{
    // This class is just a marker for xUnit to know about the fixture
}
