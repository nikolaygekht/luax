using System.ComponentModel;
using ModelContextProtocol.Server;
using Luax.Parser;
using Luax.Parser.Ast;

namespace LuaX.Mcp.Server.Tools;

/// <summary>
/// MCP tool that returns information about LuaX standard library classes and functions.
/// </summary>
[McpServerToolType]
public static class LuaXStdLibTool
{
    private static StdLibInfo? _cachedStdLibInfo;
    private static readonly object _lock = new object();

    [McpServerTool, Description("Get an overview of the LuaX standard library with class names, descriptions, and categories. For detailed method information on a specific class, use get_stdlib_class tool.")]
    public static StdLibResponse GetStandardLibrary()
    {
        try
        {
            // Use cached info if available
            if (_cachedStdLibInfo == null)
            {
                lock (_lock)
                {
                    if (_cachedStdLibInfo == null)
                    {
                        _cachedStdLibInfo = LoadStandardLibrary();
                    }
                }
            }

            // Return summary version (without detailed methods)
            var summary = CreateSummary(_cachedStdLibInfo);

            return new StdLibResponse
            {
                Success = true,
                StandardLibrary = summary,
                Error = null,
                Hint = "Use get_stdlib_class(className) to get detailed method information for a specific class"
            };
        }
        catch (Exception ex)
        {
            return new StdLibResponse
            {
                Success = false,
                Error = $"Failed to load standard library: {ex.Message}",
                StandardLibrary = null
            };
        }
    }

    [McpServerTool, Description("Get detailed information about a specific standard library class, including all methods, parameters, and documentation")]
    public static StdLibClassResponse GetStdLibClass([Description("Name of the standard library class (e.g., 'stdlib', 'file', 'string_map')"), System.ComponentModel.DataAnnotations.Required] string className)
    {
        try
        {
            // Use cached info if available
            if (_cachedStdLibInfo == null)
            {
                lock (_lock)
                {
                    if (_cachedStdLibInfo == null)
                    {
                        _cachedStdLibInfo = LoadStandardLibrary();
                    }
                }
            }

            var classInfo = _cachedStdLibInfo.Classes.FirstOrDefault(c =>
                c.Name.Equals(className, StringComparison.OrdinalIgnoreCase));

            if (classInfo == null)
            {
                var availableClasses = string.Join(", ", _cachedStdLibInfo.Classes.Select(c => c.Name).OrderBy(n => n));
                return new StdLibClassResponse
                {
                    Success = false,
                    Error = $"Class '{className}' not found in standard library. Available classes: {availableClasses}",
                    Class = null
                };
            }

            return new StdLibClassResponse
            {
                Success = true,
                Class = classInfo,
                Error = null
            };
        }
        catch (Exception ex)
        {
            return new StdLibClassResponse
            {
                Success = false,
                Error = $"Failed to get class information: {ex.Message}",
                Class = null
            };
        }
    }

    private static StdLibInfo CreateSummary(StdLibInfo fullInfo)
    {
        // Create summary with class count but without detailed methods
        var summaryClasses = fullInfo.Classes.Select(cls => new StdLibClassInfo
        {
            Name = cls.Name,
            Description = cls.Description,
            Category = cls.Category,
            Methods = Array.Empty<StdLibMethodInfo>(), // Empty - details available via get_stdlib_class
            Constants = cls.Constants.Select(c => new StdLibConstantInfo
            {
                Name = c.Name,
                Value = c.Value,
                Description = "" // Keep constants but without descriptions to save space
            }).ToArray(),
            MethodCount = cls.Methods.Length,
            ConstantCount = cls.Constants.Length
        }).ToArray();

        return new StdLibInfo
        {
            PackageName = fullInfo.PackageName,
            Description = fullInfo.Description,
            Classes = summaryClasses,
            Categories = fullInfo.Categories,
            TotalClasses = fullInfo.Classes.Length
        };
    }

    private static StdLibInfo LoadStandardLibrary()
    {
        var body = StdlibHeader.ReadStdlib();

        var classes = body.Classes.Select(cls => new StdLibClassInfo
        {
            Name = cls.Name,
            Description = GetDescription(cls.Attributes),
            Category = CategorizeClass(cls.Name),
            Methods = cls.Methods.Select(method => new StdLibMethodInfo
            {
                Name = method.Name,
                Description = GetDescription(method.Attributes),
                ReturnType = method.ReturnType.ToString(),
                Parameters = method.Arguments.Select(arg => new StdLibParameterInfo
                {
                    Name = arg.Name,
                    Type = arg.LuaType.ToString(),
                    Description = GetParameterDescription(method.Attributes, arg.Name)
                }).ToArray(),
                IsStatic = method.Static,
                IsExtern = method.Extern
            }).ToArray(),
            Constants = cls.Constants.Select(constant => new StdLibConstantInfo
            {
                Name = constant.Name,
                Value = constant.Value?.Value?.ToString() ?? "",
                Description = GetDescription(cls.Attributes) // Constants don't have individual attributes in the current structure
            }).ToArray()
        }).ToArray();

        var categories = classes
            .GroupBy(c => c.Category)
            .Select(g => new CategoryInfo
            {
                Name = g.Key,
                Description = GetCategoryDescription(g.Key),
                Classes = g.Select(c => c.Name).ToArray()
            }).ToArray();

        return new StdLibInfo
        {
            PackageName = "LuaxStdlib",
            Description = "The LuaX standard library provides essential functionality for string manipulation, math operations, I/O, collections, and more",
            Classes = classes,
            Categories = categories
        };
    }

    private static string GetDescription(LuaXAttributeCollection attributes)
    {
        var docBrief = attributes.FirstOrDefault(a => a.Name == "DocBrief");
        if (docBrief != null && docBrief.Parameters.Count > 0)
        {
            return docBrief.Parameters[0]?.Value?.ToString() ?? "";
        }
        return "";
    }

    private static string GetParameterDescription(LuaXAttributeCollection attributes, string paramName)
    {
        var docParam = attributes.FirstOrDefault(a =>
            a.Name == "DocParameter" &&
            a.Parameters.Count > 0 &&
            a.Parameters[0]?.Value?.ToString() == paramName);

        if (docParam != null && docParam.Parameters.Count > 1)
        {
            return docParam.Parameters[1]?.Value?.ToString() ?? "";
        }
        return "";
    }

    private static string CategorizeClass(string className)
    {
        return className switch
        {
            "stdlib" => "Core",
            "match" or "regexp" => "Text Processing",
            "io" or "file" => "I/O",
            "buffer" => "Data Structures",
            "cryptography" => "Security",
            "csvParser" or "xmlParser" or "xmlNode" or "jsonParser" or "jsonNode" => "Data Formats",
            "list" or "int_map" or "string_map" or "queue" or "stack" or "sorted_list" => "Collections",
            "object_comparer" => "Utilities",
            "action" => "Functional",
            "assert" => "Testing",
            "bitwise" => "Math",
            "logger" => "Logging",
            "httpCommunicator" or "httpResponseCallback" => "HTTP",
            "scheduler" => "Concurrency",
            _ => "Other"
        };
    }

    private static string GetCategoryDescription(string category)
    {
        return category switch
        {
            "Core" => "Core standard library with string, math, and datetime functions",
            "Text Processing" => "Regular expression matching and text manipulation",
            "I/O" => "File and stream I/O operations",
            "Data Structures" => "Buffer and binary data handling",
            "Security" => "Cryptographic functions",
            "Data Formats" => "Parsing and serialization for CSV, XML, and JSON",
            "Collections" => "Generic collection types (lists, maps, queues, stacks)",
            "Utilities" => "Helper utilities and comparers",
            "Functional" => "Functional programming primitives",
            "Testing" => "Assertion and testing utilities",
            "Math" => "Bitwise operations",
            "Logging" => "Logging functionality",
            "HTTP" => "HTTP client and request handling",
            "Concurrency" => "Asynchronous task scheduling",
            _ => "Other standard library components"
        };
    }
}

/// <summary>
/// Response from the GetStandardLibrary tool.
/// </summary>
public record StdLibResponse
{
    public bool Success { get; init; }
    public StdLibInfo? StandardLibrary { get; init; }
    public string? Error { get; init; }
    public string? Hint { get; init; }
}

/// <summary>
/// Response from the GetStdLibClass tool.
/// </summary>
public record StdLibClassResponse
{
    public bool Success { get; init; }
    public StdLibClassInfo? Class { get; init; }
    public string? Error { get; init; }
}

/// <summary>
/// Information about the LuaX standard library.
/// </summary>
public record StdLibInfo
{
    public string PackageName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public StdLibClassInfo[] Classes { get; init; } = Array.Empty<StdLibClassInfo>();
    public CategoryInfo[] Categories { get; init; } = Array.Empty<CategoryInfo>();
    public int TotalClasses { get; init; }
}

/// <summary>
/// Category information grouping related classes.
/// </summary>
public record CategoryInfo
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Classes { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Information about a standard library class.
/// </summary>
public record StdLibClassInfo
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public StdLibMethodInfo[] Methods { get; init; } = Array.Empty<StdLibMethodInfo>();
    public StdLibConstantInfo[] Constants { get; init; } = Array.Empty<StdLibConstantInfo>();
    public int MethodCount { get; init; }
    public int ConstantCount { get; init; }
}

/// <summary>
/// Information about a standard library method.
/// </summary>
public record StdLibMethodInfo
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ReturnType { get; init; } = string.Empty;
    public StdLibParameterInfo[] Parameters { get; init; } = Array.Empty<StdLibParameterInfo>();
    public bool IsStatic { get; init; }
    public bool IsExtern { get; init; }
}

/// <summary>
/// Information about a method parameter.
/// </summary>
public record StdLibParameterInfo
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Information about a constant.
/// </summary>
public record StdLibConstantInfo
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
