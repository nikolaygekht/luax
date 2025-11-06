# LuaX Project - Claude Code Instructions

This is a **LuaX programming language** project. LuaX is an object-oriented language designed for cross-compilation to C#, TypeScript, Java, Go, and Python.

## 🚀 Getting Started - REQUIRED First Steps

When you first start working with this LuaX project, **you MUST call these MCP tools in order**:

### 1. Learn the Language (CRITICAL)
```
Call: get_language_info
```
**Why:** LuaX has specific syntax that differs from Lua and other languages:
- Uses `this` (NOT `self`) for instance references
- Uses `super` for parent class references
- Has specific operator precedence and scoping rules
- **Calling this first prevents 80% of syntax mistakes**

### 2. Understand Available Tools
```
Call: get_info
```
**Why:** Shows all available MCP tools, recommended workflow, and best practices for LuaX development.

### 3. Explore Standard Library (Optional)
```
Call: get_standard_library
```
**Why:** See all 26+ stdlib classes (file, string, list, map, etc.) organized by category.
- For details on a specific class: `get_stdlib_class("className")`

---

## 📋 Workflow for Modifying .luax Files

**BEFORE making ANY changes to .luax files:**

### Step 1: Parse the File First
```
Call: parse_file(filePath: "/absolute/path/to/file.luax")
```
**Why:**
- Validates syntax and shows you the current structure
- Shows all classes, methods, properties, and their types
- Identifies any existing parse errors
- **Use `parse_file` (not `parse`) to avoid permission dialogs with large file content**

### Step 2: Make Your Changes
- Edit the .luax file based on the parsed AST structure
- Follow LuaX syntax rules from `get_language_info`
- Use proper types, visibility modifiers, and conventions

### Step 3: Validate Your Changes
```
Call: parse_file(filePath: "/absolute/path/to/file.luax")
```
**Why:** Verify your changes are syntactically correct before presenting them to the user.

---

## 🎯 Quick Reference - Common Syntax

### Instance Reference
```luax
this.propertyName     -- Access property
this.methodName()     -- Call method
```
⚠️ Use `this`, NOT `self`!

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

---

## 🛠️ Available MCP Tools

### Language & Documentation
- `get_language_info` - Complete language reference (syntax, operators, patterns)
- `get_grammar` - Full grammar definition in Hime format
- `get_info` - Server capabilities and workflow guidance

### Parsing & Validation
- `parse_file(filePath)` - Parse a .luax file (preferred - no permission dialog)
- `parse(sourceCode, sourceName?)` - Parse inline source code

### Standard Library
- `get_standard_library` - Overview of all 26+ stdlib classes
- `get_stdlib_class(className)` - Detailed methods for specific class

### Prompts (User-Selected Guidance)
- **Workflow Guidance** - Complete development workflow
- **Parse Reminder** - Reminder to parse before changes
- **Syntax Quick Reference** - Common syntax patterns
- **Error Debugging Guide** - Help with parse errors

---

## ✅ Best Practices

1. **Always parse first** - Use `parse_file` before and after changes
2. **Use correct keywords** - `this` (not `self`), `super` (not `base`)
3. **Explicit types** - Declare types for all variables and parameters
4. **Documentation** - Use `@DocBrief` and `@DocDescription` attributes
5. **Portability** - Keep code portable for cross-compilation targets

---

## 📝 Example Workflow

**Scenario:** User asks to add a new method to `calculator.luax`

```
1. parse_file(filePath: "/full/path/to/calculator.luax")
   → Review existing structure, classes, methods

2. Identify where to add the new method
   → Based on parsed AST

3. Make changes to the file
   → Follow LuaX syntax from get_language_info

4. parse_file(filePath: "/full/path/to/calculator.luax")
   → Verify changes are syntactically correct

5. Present changes to user
   → Show parse results to confirm validity
```

---

## 🎓 Learning Resources

- **Language Reference:** Call `get_language_info`
- **Grammar Details:** Call `get_grammar`
- **Stdlib Reference:** Call `get_standard_library`
- **Code Examples:** Check `SyntaxDetails.CommonPatterns` in `get_language_info`

---

## ⚠️ Common Mistakes to Avoid

❌ **DON'T:**
- Use `self` instead of `this`
- Use `base` instead of `super`
- Forget to parse before making changes
- Use `parse` for existing files (causes large permission dialogs)
- Skip type declarations

✅ **DO:**
- Call `get_language_info` first when starting
- Use `parse_file` before and after changes
- Use `this` for instance members
- Use `super` for parent class members
- Declare explicit types everywhere

---

**Remember:** LuaX is designed for cross-platform portability. Your code should work consistently when compiled to C#, TypeScript, Java, Go, or Python!
