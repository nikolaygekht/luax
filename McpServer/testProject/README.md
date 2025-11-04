# LuaX MCP Server Test Project

This folder contains a test project configured to use the LuaX MCP Server with Claude Code.

## What's Included

### MCP Configuration
- `.mcp.json` - Claude Code MCP server configuration (in project root)
- The server is configured to run via `dotnet run` from the parent LuaX.Mcp.Server project

### Sample LuaX Files
- `calculator.luax` - A calculator class demonstrating basic LuaX syntax, methods, and documentation attributes
- `geometry.luax` - A package with geometric shapes (Point, Circle, Rectangle) showing package structure and inheritance

## How to Use

### 1. Open in Claude Code
```bash
# From the testProject directory
code .
```

### 2. Verify MCP Server Connection
Claude Code should automatically connect to the LuaX MCP server when you open this project. You can verify by:
- Opening the MCP panel
- Looking for the "luax" server
- Checking that it shows as "Running"

### 3. Available MCP Tools

Once connected, Claude Code can use these tools:

#### `get_info`
Get information about the LuaX MCP Server
```
Use the get_info tool to see server capabilities
```

#### `get_grammar`
Get the complete LuaX grammar definition
```
Use the get_grammar tool to see the Hime grammar
```

#### `get_language_info`
Get comprehensive information about the LuaX language
```
Use the get_language_info tool to learn about LuaX design goals and features
```

#### `parse`
Parse LuaX source code and get AST
```
Use the parse tool with the calculator.luax file content
```

#### `get_standard_library`
Get documentation for all standard library classes
```
Use the get_standard_library tool to see all stdlib functions
```

## Recommended Workflow

When working with LuaX files, follow this workflow for best results:

### 1. Always Parse First
Before making any changes to .luax files, use the `parse` tool to:
- Validate the current syntax
- Understand the existing code structure
- See what classes, methods, and properties exist

```
Parse calculator.luax and show me its structure
```

### 2. Learn the Syntax
Use `get_language_info` to understand LuaX syntax, especially:
- **Keywords**: Use `this` (not `self`) for instance references, `super` for parent class
- **Operators**: Check operator precedence and usage
- **Scoping Rules**: Understand variable scope and visibility
- **Common Patterns**: See examples of constructors, inheritance, error handling

```
Show me the SyntaxDetails from get_language_info, especially the 'this' and 'super' keywords
```

### 3. Discover Standard Library
Use `get_standard_library` to find available functions:
```
What string manipulation functions are available in the standard library?
```

### 4. Make Changes
When suggesting code changes:
- Use correct keywords (`this`, not `self`)
- Follow scoping rules and type declarations
- Use standard library functions where appropriate

### 5. Validate Changes
After making changes, parse again to verify correctness:
```
Parse the modified code to verify it's syntactically correct
```

### Key Best Practices

- ✅ **Always use `this`** to reference instance properties (e.g., `this.x`, `this.calculate()`)
- ✅ **Use `super`** to reference parent class (e.g., `super()` in constructors)
- ✅ **Parse before modifying** any .luax file to understand its structure
- ✅ **Declare explicit types** for all variables and parameters
- ✅ **Check operator precedence** in SyntaxDetails when writing complex expressions
- ❌ **Don't use `self`** - this is not valid LuaX syntax
- ❌ **Don't guess syntax** - use parse and get_language_info to verify

## Example Prompts to Try

### Understanding the Language
- "What is LuaX and what are its design goals?"
- "Show me the LuaX grammar"
- "What standard library functions are available for string manipulation?"
- "What are the primitive types in LuaX?"

### Working with Files
- "Parse the calculator.luax file and show me its structure"
- "What classes are defined in geometry.luax?"
- "List all methods in the Calculator class"
- "Show me the attributes used in these files"

### Getting Help
- "What can the LuaX MCP server do?"
- "Show me examples of LuaX syntax"
- "How do I use the datetime functions in stdlib?"
- "What collections are available in the standard library?"

## Sample Files Description

### calculator.luax
A simple calculator class that demonstrates:
- Class definition with `@DocBrief` and `@DocInclude` attributes
- Private properties
- Public methods with parameters and return types
- Exception handling (`throw`)
- Static methods
- Using stdlib functions (`sqrt`)

### geometry.luax
A geometry package that demonstrates:
- Package structure
- Multiple classes in one file
- Object composition (Circle contains Point)
- Mathematical calculations
- Boolean logic
- Using stdlib constants (`PI`)

## Troubleshooting

### Server Not Starting
If the MCP server doesn't start:
1. Make sure the LuaX.Mcp.Server project builds successfully:
   ```bash
   cd ../LuaX.Mcp.Server
   dotnet build
   ```
2. Check the MCP logs in Claude Code
3. Verify the path in `.mcp.json` is correct
4. Try reloading the window in Claude Code

### Server Errors
Check the server logs at:
```
../LuaX.Mcp.Server/logs/luax-mcp-server-*.log
```

## Testing the Tools

You can test the MCP tools directly through Claude Code prompts:

1. **Test get_info**:
   - "What tools are available in the LuaX MCP server?"

2. **Test get_grammar**:
   - "Show me the grammar rules for class declarations"

3. **Test get_language_info**:
   - "What are LuaX's target compilation languages?"

4. **Test parse**:
   - "Parse calculator.luax and tell me what methods it has"

5. **Test get_standard_library**:
   - "What methods are in the file I/O standard library?"

## Next Steps

- Modify the sample files and ask Claude Code to parse them
- Create your own LuaX files and test parsing
- Explore the standard library documentation
- Ask Claude Code to help you write new LuaX code using the language information

## Project Structure

```
testProject/
├── .mcp.json                  # MCP server configuration
├── calculator.luax            # Sample: basic class
├── geometry.luax              # Sample: package with multiple classes
└── README.md                  # This file
```
