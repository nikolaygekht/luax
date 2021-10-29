using System.Collections.Generic;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// A set of LuaX interpreter variables
    /// </summary>
    public class LuaXVariableInstanceSet
    {
        private readonly Dictionary<string, LuaXVariableInstance> mVariables = new Dictionary<string, LuaXVariableInstance>();

        /// <summary>
        /// Gets the variable by its name
        ///
        /// Returns `null` if there is no variable with such name is defined.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LuaXVariableInstance this[string name]
        {
            get
            {
                if (mVariables.TryGetValue(name, out var v))
                    return v;
                return null;
            }
        }

        /// <summary>
        /// Adds a variable
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public void Add(LuaXTypeDefinition type, string name)
        {
            mVariables.Add(name, new LuaXVariableInstance(name, type));
        }

        /// <summary>
        /// Adds a variable
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public void Add(LuaXTypeDefinition type, string name, object value)
        {
            mVariables.Add(name, new LuaXVariableInstance(name, type) { Value = value });
        }

        /// <summary>
        /// Checks whether the set contains a variable
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name) => mVariables.ContainsKey(name);
    }
}
