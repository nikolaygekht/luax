using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Luax.Parser;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// The delegate for LuaX extern methods
    /// </summary>
    /// <param name="this"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate object LuaXExternMethodImplementationDelegate(LuaXObjectInstance @this, object[] args);
}
