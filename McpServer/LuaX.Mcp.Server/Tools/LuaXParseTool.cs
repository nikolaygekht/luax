using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ModelContextProtocol.Server;
using Luax.Parser;
using Luax.Parser.Ast;

namespace LuaX.Mcp.Server.Tools;

/// <summary>
/// MCP tool that parses LuaX code and returns a simplified AST structure.
/// </summary>
[McpServerToolType]
public static class LuaXParseTool
{
    [McpServerTool, Description("Parse and validate LuaX source code, returning AST with classes, packages, methods, and properties. IMPORTANT: Always use this tool FIRST before suggesting any changes to .luax files to validate syntax and understand the existing code structure.")]
    public static ParseResponse Parse([Description("LuaX source code to parse"), Required] string sourceCode, [Description("Optional name for the source (e.g., filename)")] string? sourceName = null)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            return new ParseResponse
            {
                Success = false,
                Error = "Source code cannot be empty",
                Ast = null
            };
        }

        var name = sourceName ?? "inline";

        try
        {
            var parser = new LuaXAstGenerator();
            var body = parser.Compile(name, sourceCode);

            var ast = new AstResult
            {
                SourceName = body.Name,
                Packages = body.Packages.Select(pkg => new PackageInfo
                {
                    Name = pkg.Name,
                    Attributes = pkg.Attributes.Select(attr => new AttributeInfo
                    {
                        Name = attr.Name,
                        Parameters = attr.Parameters.Select(p => p?.Value?.ToString() ?? "").ToArray()
                    }).ToArray()
                }).ToArray(),
                Classes = body.Classes.Select(cls => MapClass(cls)).ToArray()
            };

            return new ParseResponse
            {
                Success = true,
                Ast = ast,
                Error = null
            };
        }
        catch (LuaXAstGeneratorException ex)
        {
            return new ParseResponse
            {
                Success = false,
                Error = $"Parse errors: {string.Join("; ", ex.Errors.Select(e => $"Line {e.Line}: {e.Message}"))}",
                Ast = null
            };
        }
        catch (Exception ex)
        {
            return new ParseResponse
            {
                Success = false,
                Error = $"Failed to parse: {ex.Message}",
                Ast = null
            };
        }
    }

    private static ClassInfo MapClass(LuaXClass cls)
    {
        return new ClassInfo
        {
            Name = cls.Name,
            PackageName = cls.PackageName,
            Parent = cls.Parent,
            Attributes = cls.Attributes.Select(attr => new AttributeInfo
            {
                Name = attr.Name,
                Parameters = attr.Parameters.Select(p => p?.Value?.ToString() ?? "").ToArray()
            }).ToArray(),
            Properties = cls.Properties.Select(prop => new PropertyInfo
            {
                Name = prop.Name,
                Type = prop.LuaType.ToString(),
                IsStatic = prop.Static,
                Visibility = prop.Visibility.ToString(),
                Attributes = prop.Attributes.Select(attr => new AttributeInfo
                {
                    Name = attr.Name,
                    Parameters = attr.Parameters.Select(p => p?.Value?.ToString() ?? "").ToArray()
                }).ToArray()
            }).ToArray(),
            Methods = cls.Methods.Select(method => new MethodInfo
            {
                Name = method.Name,
                ReturnType = method.ReturnType.ToString(),
                IsStatic = method.Static,
                Visibility = method.Visibility.ToString(),
                Parameters = method.Arguments.Select(arg => new ParameterInfo
                {
                    Name = arg.Name,
                    Type = arg.LuaType.ToString()
                }).ToArray(),
                Attributes = method.Attributes.Select(attr => new AttributeInfo
                {
                    Name = attr.Name,
                    Parameters = attr.Parameters.Select(p => p?.Value?.ToString() ?? "").ToArray()
                }).ToArray()
            }).ToArray(),
            Constants = cls.Constants.Select(constant => new ConstantInfo
            {
                Name = constant.Name,
                Value = constant.Value?.ToString() ?? ""
            }).ToArray()
        };
    }
}

/// <summary>
/// Response from the Parse tool.
/// </summary>
public record ParseResponse
{
    public bool Success { get; init; }
    public AstResult? Ast { get; init; }
    public string? Error { get; init; }
}

/// <summary>
/// Simplified AST result containing the parsed structure.
/// </summary>
public record AstResult
{
    public string SourceName { get; init; } = string.Empty;
    public PackageInfo[] Packages { get; init; } = Array.Empty<PackageInfo>();
    public ClassInfo[] Classes { get; init; } = Array.Empty<ClassInfo>();
}

/// <summary>
/// Package information.
/// </summary>
public record PackageInfo
{
    public string Name { get; init; } = string.Empty;
    public AttributeInfo[] Attributes { get; init; } = Array.Empty<AttributeInfo>();
}

/// <summary>
/// Class information.
/// </summary>
public record ClassInfo
{
    public string Name { get; init; } = string.Empty;
    public string PackageName { get; init; } = string.Empty;
    public string Parent { get; init; } = string.Empty;
    public AttributeInfo[] Attributes { get; init; } = Array.Empty<AttributeInfo>();
    public PropertyInfo[] Properties { get; init; } = Array.Empty<PropertyInfo>();
    public MethodInfo[] Methods { get; init; } = Array.Empty<MethodInfo>();
    public ConstantInfo[] Constants { get; init; } = Array.Empty<ConstantInfo>();
}

/// <summary>
/// Property information.
/// </summary>
public record PropertyInfo
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsStatic { get; init; }
    public string Visibility { get; init; } = string.Empty;
    public AttributeInfo[] Attributes { get; init; } = Array.Empty<AttributeInfo>();
}

/// <summary>
/// Method information.
/// </summary>
public record MethodInfo
{
    public string Name { get; init; } = string.Empty;
    public string ReturnType { get; init; } = string.Empty;
    public bool IsStatic { get; init; }
    public string Visibility { get; init; } = string.Empty;
    public ParameterInfo[] Parameters { get; init; } = Array.Empty<ParameterInfo>();
    public AttributeInfo[] Attributes { get; init; } = Array.Empty<AttributeInfo>();
}

/// <summary>
/// Parameter information.
/// </summary>
public record ParameterInfo
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}

/// <summary>
/// Attribute/annotation information.
/// </summary>
public record AttributeInfo
{
    public string Name { get; init; } = string.Empty;
    public string[] Parameters { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Constant information.
/// </summary>
public record ConstantInfo
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}
