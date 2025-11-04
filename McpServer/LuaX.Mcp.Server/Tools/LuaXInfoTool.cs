using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace LuaX.Mcp.Server.Tools;

/// <summary>
/// Placeholder tool that returns information about the LuaX MCP Server.
/// This is used to verify the server is working correctly.
/// </summary>
[McpServerToolType]
public static class LuaXInfoTool
{
    [McpServerTool, Description("Get information about the LuaX MCP Server")]
    public static InfoResponse GetInfo()
    {
        var response = new InfoResponse
        {
            ServerName = "LuaX MCP Server",
            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Model Context Protocol server for LuaX language support",
            Status = "Running",
            Features = new[]
            {
                "Parser integration",
                "Symbol extraction",
                "Code navigation",
                "Documentation support",
                "Code generation"
            }
        };

        return response;
    }
}

public record InfoResponse
{
    public string ServerName { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string[] Features { get; init; } = Array.Empty<string>();
}
