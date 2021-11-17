using System.Collections;
using System.Collections.Generic;
using Luax.Parser.Ast;

namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// A set of LuaX interpreter variables
    /// </summary>
    public class LuaXVariableInstanceSet : IEnumerable<LuaXVariableInstance>
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
        /// Default constructor
        /// </summary>
        public LuaXVariableInstanceSet()
        {
        }

        /// <summary>
        /// Construct variable for the method
        /// </summary>
        /// <param name="method"></param>
        public LuaXVariableInstanceSet(LuaXMethod method)
        {
            foreach (var arg in method.Arguments)
                Add(arg.LuaType, arg.Name);
            foreach (var var in method.Variables)
                Add(var.LuaType, var.Name);
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

        /// <summary>
        /// IEnumerable implementation to be able to enumerate through elements of LuaXVariableInstanceSet
        /// </summary>
        /// <returns></returns>
        public IEnumerator<LuaXVariableInstance> GetEnumerator()
        {
            return mVariables.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
