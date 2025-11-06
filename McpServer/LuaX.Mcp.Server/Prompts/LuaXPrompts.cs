using System.ComponentModel;
using ModelContextProtocol.Server;

namespace LuaX.Mcp.Server.Prompts;

/// <summary>
/// MCP prompts that provide context-aware guidance when working with LuaX files.
/// Prompts are displayed to users in Claude Code and other MCP clients to help
/// them follow best practices and use the right tools at the right time.
/// </summary>
[McpServerPromptType]
public static class LuaXPrompts
{
    /// <summary>
    /// Provides guidance when starting to work with LuaX files.
    /// This prompt helps ensure the user understands the workflow and available tools.
    /// </summary>
    [McpServerPrompt, Description("Get started with LuaX development workflow and best practices")]
    public static string WorkflowGuidance()
    {
        var content = @"# LuaX Development Workflow

When working with LuaX files, follow this recommended workflow:

## 1. Understand the Language
- Call `get_language_info` to learn about LuaX syntax, semantics, and features
- Important: LuaX uses `this` (not `self`) for instance references
- Use `super` to call parent class methods
- Review operator precedence and scoping rules

## 2. Parse Before Modifying
- **ALWAYS** call `parse_file` on existing .luax files BEFORE suggesting any changes
- Use `parse_file` (not `parse`) to avoid large permission dialogs
- This validates syntax and shows you the current structure (classes, methods, properties)
- Parse errors will show exact line numbers and messages

## 3. Explore Standard Library
- Call `get_standard_library` to see all available stdlib classes and categories
- Use `get_stdlib_class(className)` for detailed method signatures
- Categories: Core, Collections, I/O, Data Formats, Text Processing, HTTP, and more

## 4. Validate Changes
- After making changes, call `parse_file` again to verify correctness
- Check that types, method signatures, and syntax are all valid

## 5. Best Practices
- Use explicit type declarations for all variables and parameters
- Add @DocBrief and @DocDescription attributes to public classes/methods
- Keep code portable - LuaX cross-compiles to C#, TypeScript, Java, Go, and Python
- Use proper visibility modifiers (public, private, internal)

Use these tools proactively to ensure high-quality LuaX code!";

        return content;
    }

    /// <summary>
    /// Provides a quick reminder about parsing .luax files before making changes.
    /// </summary>
    [McpServerPrompt, Description("Reminder to parse LuaX files before making changes")]
    public static string ParseReminder()
    {
        var content = @"# Remember: Parse First!

Before suggesting any changes to .luax files:

1. Call `parse_file` with the file path (preferred - no permission dialog)
2. Review the AST structure (classes, methods, properties, types)
3. Check for any existing parse errors
4. Understand the current code structure

This ensures your suggestions are syntactically correct and respect the existing codebase structure.

Example:
```
parse_file(filePath: ""/absolute/path/to/file.luax"")
```

Or for inline code:
```
parse(sourceCode: ""<code here>"", sourceName: ""filename.luax"")
```

The parser will show you:
- All classes and their inheritance hierarchy
- Methods with parameters and return types
- Properties with types and visibility
- Attributes (@DocBrief, @DocInclude, etc.)
- Any syntax errors with line numbers";

        return content;
    }

    /// <summary>
    /// Explains key LuaX syntax differences that are often confused.
    /// </summary>
    [McpServerPrompt, Description("Quick reference for common LuaX syntax patterns")]
    public static string SyntaxQuickReference()
    {
        var content = @"# LuaX Syntax Quick Reference

## Common Syntax Patterns

### Instance Reference
```luax
this.propertyName     -- Access property
this.methodName()     -- Call method
```
⚠️ Use `this`, NOT `self`

### Parent Class Reference
```luax
super()               -- Call parent constructor
super.methodName()    -- Call parent method
```

### Class Declaration
```luax
class ClassName : ParentClass
    public var property: int;

    public function ClassName(): void
        super();
        this.property = 0;
    end

    public function method(param: string): boolean
        return true;
    end
end
```

### Type Declarations
```luax
var name: string;           -- String
var count: int;             -- Integer
var price: real;            -- Float/Double
var items: string[];        -- Array
var obj: ClassName;         -- Class instance
```

### String Concatenation
```luax
var message = ""Hello"" .. "" World"";   -- Use '..' operator
```

### Array Initialization
```luax
var numbers = new int[] { 1, 2, 3, 4, 5 };
```

### Exception Handling
```luax
if value < 0 then
    throw ""Value must be positive"";
end
```

Call `get_language_info` for complete documentation including operator precedence, scoping rules, and code patterns!";

        return content;
    }

    /// <summary>
    /// Provides guidance for fixing parse errors in LuaX code.
    /// </summary>
    [McpServerPrompt, Description("Help with debugging LuaX parse errors")]
    public static string ErrorDebuggingGuide([Description("The parse error message")] string? errorMessage = null)
    {
        var content = @"# Debugging LuaX Parse Errors

When you encounter parse errors, follow these steps:

## 1. Identify the Error Location
Parse errors include line numbers pointing to the problem location.

## 2. Common Parse Error Causes

### Missing 'end' keyword
Classes, functions, if/while blocks must close with `end`

### Incorrect type syntax
✗ `var x = 10;`
✓ `var x: int;` or `var x: int = 10;`

### Wrong instance reference
✗ `self.property`
✓ `this.property`

### Missing semicolons
Required after variable declarations and statements

### Incorrect string concatenation
✗ `""hello"" + ""world""`
✓ `""hello"" .. ""world""`

## 3. Validate Step-by-Step
1. Parse the original code to see the error
2. Fix the identified issue
3. Parse again to verify
4. Repeat until clean

## 4. Check Against Grammar
If still stuck, call `get_grammar` to see the complete grammar definition.";

        if (!string.IsNullOrEmpty(errorMessage))
        {
            content += $@"

---

## Your Error Message:
```
{errorMessage}
```

Analyze this error and check the line number to identify the issue.";
        }

        return content;
    }
}
