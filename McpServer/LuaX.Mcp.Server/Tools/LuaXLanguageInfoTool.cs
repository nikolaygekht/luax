using System.ComponentModel;
using ModelContextProtocol.Server;

namespace LuaX.Mcp.Server.Tools;

/// <summary>
/// MCP tool that returns comprehensive information about the LuaX language,
/// its purpose, design goals, and intended use cases.
/// </summary>
[McpServerToolType]
public static class LuaXLanguageInfoTool
{
    [McpServerTool, Description("Get comprehensive information about the LuaX language, its purpose, design goals, and cross-compilation capabilities")]
    public static LanguageInfoResponse GetLanguageInfo()
    {
        return new LanguageInfoResponse
        {
            Name = "LuaX",
            Description = "Object-oriented programming language based on Lua, designed for cross-platform code generation and portability",

            Purpose = "LuaX is designed to enable writing code once and generating equivalent implementations in multiple target languages (C#, TypeScript, Java, Go, Python), ensuring consistency and reducing maintenance overhead for multi-platform projects.",

            DesignGoals = new[]
            {
                "Cross-compilation: Write once, compile to multiple languages",
                "Type safety: Strong static typing with type declarations",
                "Object-oriented: Classes, inheritance, encapsulation",
                "Simplicity: Clean, readable syntax based on Lua",
                "Portability: Code that works consistently across all target platforms",
                "Documentation-first: Built-in documentation attributes (@DocBrief, @DocDescription, etc.)",
                "Maintainability: Single source of truth for multi-platform codebases"
            },

            TargetLanguages = new[]
            {
                "C#",
                "TypeScript/JavaScript",
                "Java",
                "Go",
                "Python"
            },

            KeyFeatures = new[]
            {
                "Classes and inheritance",
                "Strong static typing (int, real, boolean, string, datetime)",
                "Arrays and collections",
                "Exception handling (try/catch/throw)",
                "Attributes/annotations (@DocBrief, @DocDescription, @Cast, @DocInclude, etc.)",
                "Packages for code organization",
                "Public/private/internal visibility modifiers",
                "Static members and methods",
                "Custom type casting with @Cast attribute",
                "Extern declarations for platform-specific implementations",
                "Standard library (file, string, buffer, assert, csvParser)"
            },

            UseCases = new[]
            {
                "Cross-platform business logic implementation",
                "API data models shared across frontend and backend",
                "Algorithms that need consistent behavior across platforms",
                "Code generation for microservices in different languages",
                "Documentation-driven development with executable specs"
            },

            TypeSystem = new TypeSystemInfo
            {
                PrimitiveTypes = new[] { "int", "real", "boolean", "string", "datetime", "void" },
                ArraySupport = true,
                ClassSupport = true,
                Description = "Strong static typing with compile-time type checking. Arrays are declared with [] suffix (e.g., int[], string[]). Classes are user-defined types with properties and methods."
            },

            StandardLibrary = new[]
            {
                "file: File I/O operations",
                "string: String manipulation utilities",
                "buffer: Buffer and data handling",
                "assert: Assertion and validation functions",
                "csvParser: CSV file parsing"
            },

            BestPractices = new[]
            {
                "Use @DocBrief and @DocDescription attributes for all public classes and methods",
                "Organize code into packages for better structure",
                "Declare explicit types for all variables and parameters",
                "Use @Cast attribute when custom type conversions are needed",
                "Keep code portable - avoid platform-specific constructs unless using extern",
                "Use @DocInclude to mark classes for documentation generation",
                "Leverage inheritance and composition for code reuse",
                "Handle exceptions appropriately with try/catch blocks"
            },

            SyntaxHighlights = new
            {
                PackageDeclaration = "package PackageName ... end",
                ClassDeclaration = "class ClassName : ParentClass ... end",
                FunctionDeclaration = "function methodName(param1: int, param2: string): boolean ... end",
                VariableDeclaration = "var myVar: int;",
                ArrayDeclaration = "var items: string[];",
                AttributeUsage = "@DocBrief(\"Description\") @DocInclude()",
                Instantiation = "new ClassName()",
                ArrayInitialization = "new int[] { 1, 2, 3 }"
            }
        };
    }
}

/// <summary>
/// Comprehensive information about the LuaX language.
/// </summary>
public record LanguageInfoResponse
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
    public string[] DesignGoals { get; init; } = Array.Empty<string>();
    public string[] TargetLanguages { get; init; } = Array.Empty<string>();
    public string[] KeyFeatures { get; init; } = Array.Empty<string>();
    public string[] UseCases { get; init; } = Array.Empty<string>();
    public TypeSystemInfo TypeSystem { get; init; } = new();
    public string[] StandardLibrary { get; init; } = Array.Empty<string>();
    public string[] BestPractices { get; init; } = Array.Empty<string>();
    public object? SyntaxHighlights { get; init; }
}

/// <summary>
/// Information about LuaX type system.
/// </summary>
public record TypeSystemInfo
{
    public string[] PrimitiveTypes { get; init; } = Array.Empty<string>();
    public bool ArraySupport { get; init; }
    public bool ClassSupport { get; init; }
    public string Description { get; init; } = string.Empty;
}
