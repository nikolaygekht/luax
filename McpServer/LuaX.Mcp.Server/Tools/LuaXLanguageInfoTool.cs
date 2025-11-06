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
    [McpServerTool, Description("Get comprehensive information about the LuaX language, its purpose, design goals, and cross-compilation capabilities. IMPORTANT: Call this FIRST when working with .luax files to understand critical syntax like 'this', 'super', operators, and scoping rules - prevents common mistakes like using 'self' instead of 'this'.")]
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
            },

            SyntaxDetails = new SyntaxDetailsInfo
            {
                Keywords = new KeywordInfo[]
                {
                    new KeywordInfo
                    {
                        Keyword = "this",
                        Description = "References the current instance of the class. Use this to access instance properties and methods.",
                        Usage = "this.propertyName or this.methodName()",
                        Example = "this.x = 10; var result = this.calculate();"
                    },
                    new KeywordInfo
                    {
                        Keyword = "super",
                        Description = "References the parent/base class. Use super to call parent class constructors or methods.",
                        Usage = "super.methodName() or super(args) in constructor",
                        Example = "super(x, y); super.initialize();"
                    },
                    new KeywordInfo
                    {
                        Keyword = "new",
                        Description = "Creates a new instance of a class or initializes an array.",
                        Usage = "new ClassName() or new Type[] { values }",
                        Example = "var obj = new Calculator(); var arr = new int[] { 1, 2, 3 };"
                    },
                    new KeywordInfo
                    {
                        Keyword = "throw",
                        Description = "Throws an exception with the specified message.",
                        Usage = "throw \"error message\"",
                        Example = "if x < 0 then throw \"Value must be positive\"; end"
                    },
                    new KeywordInfo
                    {
                        Keyword = "return",
                        Description = "Returns a value from a function.",
                        Usage = "return value",
                        Example = "return this.x + this.y;"
                    }
                },

                Operators = new OperatorInfo[]
                {
                    new OperatorInfo { Operator = "==", Description = "Equality comparison", Precedence = 3, Example = "if a == b then" },
                    new OperatorInfo { Operator = "!=", Description = "Inequality comparison", Precedence = 3, Example = "if a != 0 then" },
                    new OperatorInfo { Operator = "<", Description = "Less than", Precedence = 3, Example = "if x < 10 then" },
                    new OperatorInfo { Operator = "<=", Description = "Less than or equal", Precedence = 3, Example = "if x <= max then" },
                    new OperatorInfo { Operator = ">", Description = "Greater than", Precedence = 3, Example = "if x > 0 then" },
                    new OperatorInfo { Operator = ">=", Description = "Greater than or equal", Precedence = 3, Example = "if x >= min then" },
                    new OperatorInfo { Operator = "+", Description = "Addition", Precedence = 4, Example = "var sum = a + b;" },
                    new OperatorInfo { Operator = "-", Description = "Subtraction", Precedence = 4, Example = "var diff = a - b;" },
                    new OperatorInfo { Operator = "*", Description = "Multiplication", Precedence = 5, Example = "var product = a * b;" },
                    new OperatorInfo { Operator = "/", Description = "Division", Precedence = 5, Example = "var quotient = a / b;" },
                    new OperatorInfo { Operator = "..", Description = "String concatenation", Precedence = 4, Example = "var msg = \"Hello\" .. \" World\";" },
                    new OperatorInfo { Operator = "and", Description = "Logical AND", Precedence = 2, Example = "if x > 0 and x < 10 then" },
                    new OperatorInfo { Operator = "or", Description = "Logical OR", Precedence = 1, Example = "if x < 0 or x > 10 then" },
                    new OperatorInfo { Operator = "not", Description = "Logical NOT", Precedence = 6, Example = "if not isValid then" }
                },

                ScopingRules = new[]
                {
                    "Variables declared with 'var' are local to the current scope (function or block)",
                    "Class properties declared with 'public var' are accessible from outside the class",
                    "Class properties declared with 'private var' are only accessible within the class",
                    "Use 'this' to explicitly reference instance members to avoid ambiguity",
                    "Parameters shadow outer scope variables with the same name",
                    "Packages create namespaces; classes in packages are referenced as PackageName.ClassName"
                },

                CommonPatterns = new CodePatternInfo[]
                {
                    new CodePatternInfo
                    {
                        Name = "Class with Constructor",
                        Description = "Define a class with properties and initialize them in the constructor",
                        Example = @"class Point
    public var x: real;
    public var y: real;

    public function Point(): void
        this.x = 0.0;
        this.y = 0.0;
    end
end"
                    },
                    new CodePatternInfo
                    {
                        Name = "Inheritance and Super Call",
                        Description = "Extend a base class and call parent constructor",
                        Example = @"class ColoredPoint : Point
    public var color: string;

    public function ColoredPoint(): void
        super();  -- Call parent constructor
        this.color = ""black"";
    end
end"
                    },
                    new CodePatternInfo
                    {
                        Name = "Method with Parameters",
                        Description = "Define a method that takes parameters and returns a value",
                        Example = @"public function add(a: real, b: real): real
    var result: real;
    result = a + b;
    return result;
end"
                    },
                    new CodePatternInfo
                    {
                        Name = "Error Handling",
                        Description = "Validate input and throw exceptions for invalid cases",
                        Example = @"public function divide(a: real, b: real): real
    if b == 0.0 then
        throw ""Division by zero"";
    end
    return a / b;
end"
                    },
                    new CodePatternInfo
                    {
                        Name = "Using Standard Library",
                        Description = "Call standard library functions for common operations",
                        Example = @"public function squareRoot(x: real): real
    if x < 0.0 then
        throw ""Cannot calculate square root of negative number"";
    end
    return stdlib.sqrt(x);
end"
                    },
                    new CodePatternInfo
                    {
                        Name = "Array Initialization and Access",
                        Description = "Create and work with arrays",
                        Example = @"var numbers: int[];
numbers = new int[] { 1, 2, 3, 4, 5 };
var first = numbers[0];
numbers[1] = 10;"
                    }
                }
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
    public SyntaxDetailsInfo? SyntaxDetails { get; init; }
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

/// <summary>
/// Detailed syntax information including keywords, operators, and patterns.
/// </summary>
public record SyntaxDetailsInfo
{
    public KeywordInfo[] Keywords { get; init; } = Array.Empty<KeywordInfo>();
    public OperatorInfo[] Operators { get; init; } = Array.Empty<OperatorInfo>();
    public string[] ScopingRules { get; init; } = Array.Empty<string>();
    public CodePatternInfo[] CommonPatterns { get; init; } = Array.Empty<CodePatternInfo>();
}

/// <summary>
/// Information about a language keyword.
/// </summary>
public record KeywordInfo
{
    public string Keyword { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Usage { get; init; } = string.Empty;
    public string Example { get; init; } = string.Empty;
}

/// <summary>
/// Information about an operator.
/// </summary>
public record OperatorInfo
{
    public string Operator { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Precedence { get; init; }
    public string Example { get; init; } = string.Empty;
}

/// <summary>
/// Common code pattern with example.
/// </summary>
public record CodePatternInfo
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Example { get; init; } = string.Empty;
}
