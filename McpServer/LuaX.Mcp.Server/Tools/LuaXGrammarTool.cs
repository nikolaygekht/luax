using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace LuaX.Mcp.Server.Tools;

/// <summary>
/// MCP tool that returns the LuaX grammar definition.
/// </summary>
[McpServerToolType]
public static class LuaXGrammarTool
{
    private const string GrammarResourceName = "LuaX.Mcp.Server.Resources.luax.gram";

    [McpServerTool, Description("Get the complete LuaX grammar definition in Hime format")]
    public static GrammarResponse GetGrammar()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(GrammarResourceName);

            if (stream == null)
            {
                return new GrammarResponse
                {
                    Success = false,
                    Error = "Grammar resource not found",
                    Grammar = string.Empty
                };
            }

            using var reader = new StreamReader(stream);
            var grammarContent = reader.ReadToEnd();

            return new GrammarResponse
            {
                Success = true,
                Grammar = grammarContent,
                Error = null
            };
        }
        catch (Exception ex)
        {
            return new GrammarResponse
            {
                Success = false,
                Error = $"Failed to read grammar: {ex.Message}",
                Grammar = string.Empty
            };
        }
    }
}

/// <summary>
/// Response from the GetGrammar tool.
/// </summary>
public record GrammarResponse
{
    public bool Success { get; init; }
    public string Grammar { get; init; } = string.Empty;
    public string? Error { get; init; }
}
