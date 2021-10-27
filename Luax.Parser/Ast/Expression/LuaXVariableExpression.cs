﻿namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// The variable expression.
    /// </summary>
    public class LuaXVariableExpression : LuaXExpression
    {
        public string Name { get; }

        internal LuaXVariableExpression(string name, LuaXTypeDefinition type, LuaXElementLocation location)
            : base(type, location)
        {
            Name = name;
        }
    }
}
