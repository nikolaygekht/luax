using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// An instance of a LuaX interpreter variable
    /// </summary>
    public class LuaXVariableInstance
    {
        /// <summary>
        /// The variable name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// LuaX variable type
        /// </summary>
        public LuaXTypeDefinition LuaType { get; }

        /// <summary>
        /// Variable value
        /// </summary>
        public object Value { get; set; }

        public LuaXVariableInstance(string name, LuaXTypeDefinition luaType)
        {
            Name = name;
            LuaType = luaType;
            Value = luaType.DefaultValue();
        }

        public int AsInteger() => (int)Value;
        public double AsReal() => (double)Value;
        public bool AsBoolean() => (bool)Value;
        public string AsString() => (string)Value;
        public DateTime AsDateTime() => (DateTime)Value;
        public LuaXObjectInstance AsObject() => (LuaXObjectInstance)Value;
        public LuaXVariableInstanceArray AsArray() => (LuaXVariableInstanceArray)Value;
    }
}
