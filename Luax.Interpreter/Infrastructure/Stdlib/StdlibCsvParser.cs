using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Luax.Parser.Ast;

#pragma warning disable S125                // Sections of code should not be commented out

namespace Luax.Interpreter.Infrastructure.Stdlib
{
    internal static class StdlibCsvParser
    {
        //public extern splitLine(line : string) : string[];
        [LuaXExternMethod("csvParser", "splitLine")]
        public static object SplitLine(LuaXObjectInstance @this, string value)
        {
            var valueSeparator = (string)@this.Properties["valueSeparator"].Value;
            var commentPrefix = (string)@this.Properties["commentPrefix"].Value;
            var allowStrings = (bool)@this.Properties["allowStrings"].Value;
            var allowComments = (bool)@this.Properties["allowComments"].Value;

            if (valueSeparator?.Length != 1)
                throw new ArgumentException("The value separator must be exactly one character", nameof(@this));

            var valueSeparatorChar = valueSeparator[0];

            var values = new List<string>();

            if (allowComments && value.StartsWith(commentPrefix))
                return null;

            StringBuilder sb = new StringBuilder();
            bool inString = false;
            for (int i = 0; i < value.Length; i++)
            {
                var cc = value[i];
                if (cc == valueSeparatorChar && !inString)
                {
                    values.Add(sb.ToString());
                    sb.Clear();
                }
                else if (cc == '"' && allowStrings)
                    inString = !inString;
                else
                {
                    sb.Append(cc);
                }
            }
            values.Add(sb.ToString());

            LuaXVariableInstanceArray arr = new LuaXVariableInstanceArray(LuaXTypeDefinition.String.ArrayOf(), values.Count);
            for (int i = 0; i < arr.Length; i++)
                arr[i].Value = values[i];
            return arr;
        }
     }
}
