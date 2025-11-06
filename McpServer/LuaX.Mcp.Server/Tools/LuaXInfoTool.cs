using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace LuaX.Mcp.Server.Tools;

/// <summary>
/// Tool that returns information about the LuaX MCP Server and its capabilities.
/// </summary>
[McpServerToolType]
public static class LuaXInfoTool
{
    [McpServerTool, Description("Get information about the LuaX MCP Server, including version, available tools, and capabilities")]
    public static InfoResponse GetInfo()
    {
        var response = new InfoResponse
        {
            ServerName = "LuaX MCP Server",
            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Description = "Model Context Protocol server for LuaX language support - provides grammar, parsing, standard library documentation, and language information",
            Status = "Running",
            AvailableTools = new[]
            {
                "get_info - Get server information and capabilities",
                "get_grammar - Get the complete LuaX grammar definition in Hime format",
                "get_language_info - Get comprehensive information about LuaX language design and features",
                "parse_file - Parse a .luax file by path and return simplified AST structure (preferred - avoids permission dialogs)",
                "parse - Parse LuaX source code from string and return simplified AST structure",
                "get_standard_library - Get overview of all standard library classes with names, descriptions, and categories",
                "get_stdlib_class - Get detailed method information for a specific standard library class"
            },
            Features = new[]
            {
                "LuaX grammar access (Hime format)",
                "Source code parsing to AST",
                "Language design and best practices information",
                "Complete standard library documentation (26+ classes)",
                "Cross-compilation context (C#, TypeScript, Java, Go, Python)",
                "Attribute and annotation support (@DocBrief, @DocInclude, etc.)",
                "Type system information (int, real, boolean, string, datetime, arrays, classes)",
                "Package and class structure analysis"
            },
            SupportedFileTypes = new[] { ".luax" },
            StandardLibraryClasses = 26,
            RecommendedWorkflow = new[]
            {
                "1. FIRST TIME: Call 'get_language_info' to learn LuaX syntax ('this' not 'self', 'super', operators, scoping)",
                "2. BEFORE CHANGES: Always use 'parse_file' to validate syntax and understand current structure",
                "3. FOR STDLIB: Use 'get_standard_library' for overview, then 'get_stdlib_class(name)' for method details",
                "4. AFTER CHANGES: Use 'parse_file' again to verify your modifications are syntactically correct",
                "5. IF STUCK: Consult 'get_grammar' for detailed syntax rules or use prompts for guidance"
            },
            BestPractices = new[]
            {
                "Always validate existing .luax code with 'parse_file' before suggesting modifications",
                "Use 'parse_file' instead of 'parse' for existing files to avoid large permission dialogs",
                "Use 'this' keyword to reference current instance properties (not 'self')",
                "Use 'super' keyword for parent class references",
                "Check SyntaxDetails in 'get_language_info' for operator precedence and scoping rules",
                "Verify method signatures and types match the parsed AST structure",
                "Use explicit type declarations for all variables and parameters"
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
    public string[] AvailableTools { get; init; } = Array.Empty<string>();
    public string[] Features { get; init; } = Array.Empty<string>();
    public string[] SupportedFileTypes { get; init; } = Array.Empty<string>();
    public int StandardLibraryClasses { get; init; }
    public string[] RecommendedWorkflow { get; init; } = Array.Empty<string>();
    public string[] BestPractices { get; init; } = Array.Empty<string>();
}
