using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using Serilog;
using Serilog.Events;

namespace LuaX.Mcp.Server;

/// <summary>
/// LuaX MCP Server - Model Context Protocol server for LuaX language support
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Configure Serilog for rolling file logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: "logs/luax-mcp-server-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("=== LuaX MCP Server Starting ===");
            Log.Information("Version: {Version}", typeof(Program).Assembly.GetName().Version);

            // Build and run MCP server using Host builder
            var builder = Host.CreateApplicationBuilder(args);

            // Configure Serilog
            builder.Services.AddSerilog();

            // Configure MCP server with stdio transport
            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly(); // Auto-discover tools with [McpServerTool] attribute

            // TODO: Register additional services
            // builder.Services.AddSingleton<ILuaXParserService, LuaXParserService>();
            // builder.Services.AddSingleton<ISymbolExtractor, SymbolExtractor>();

            Log.Information("Building MCP server host...");
            var host = builder.Build();

            Log.Information("LuaX MCP Server started successfully");
            Log.Information("Listening on stdio for MCP requests...");

            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during server startup");
            return 1;
        }
        finally
        {
            Log.Information("=== LuaX MCP Server Shutting Down ===");
            Log.CloseAndFlush();
        }
    }
}
