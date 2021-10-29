namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// The instance of the object
    /// </summary>
    public class LuaXObjectInstance
    {
        /// <summary>
        /// The reference to the type
        /// </summary>
        public LuaXClassInstance Class { get; }

        /// <summary>
        /// The static properties of the class
        /// </summary>
        public LuaXVariableInstanceSet Properties { get; set; }

        internal LuaXObjectInstance(LuaXClassInstance @class)
        {
            Class = @class;
            Properties = LuaXClassInstance.InitializeInstance(@class.LuaType);
        }
    }
}
