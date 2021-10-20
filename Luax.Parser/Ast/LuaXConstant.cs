using System;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// LuaX constant
    /// </summary>
    public class LuaXConstant
    {
        /// <summary>
        /// The data type of the constant
        /// </summary>
        public LuaXType ConstantType { get; internal init; }

        /// <summary>
        /// The constant value.
        /// </summary>
        public object Value { get; internal init; }

        /// <summary>
        /// Checks whether the constant is a `nil` value.
        /// </summary>
        public bool IsNil => ConstantType == LuaXType.Class && Value == null;

        /// <summary>
        /// A `nil` value constant
        /// </summary>
        public static LuaXConstant Nil { get; } = new LuaXConstant(LuaXType.Class, null);

        /// <summary>
        /// A `true` boolean constant
        /// </summary>
        public static LuaXConstant True { get; } = new LuaXConstant(LuaXType.Boolean, true);

        /// <summary>
        /// A `false` boolean constant
        /// </summary>
        public static LuaXConstant False { get; } = new LuaXConstant(LuaXType.Boolean, false);

        internal LuaXConstant(LuaXType type, object value)
        {
            ConstantType = type;
            Value = value;
        }

        internal LuaXConstant(int value) : this(LuaXType.Integer, value)
        {
        }

        internal LuaXConstant(double value) : this(LuaXType.Real, value)
        {
        }

        internal LuaXConstant(string value) : this(LuaXType.String, value)
        {
        }

        internal LuaXConstant(bool value) : this(LuaXType.Boolean, value)
        {
        }

        /// <summary>
        /// Gets the constant value as an integer value.
        /// </summary>
        /// <returns></returns>
        public int AsInteger()
        {
            if (ConstantType == LuaXType.Integer)
                return (int)Value;
            throw new InvalidOperationException("Constant is expected to be an integer");
        }

        /// <summary>
        /// Gets the constant value as a real value.
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            if (ConstantType == LuaXType.Real)
                return (double)Value;
            throw new InvalidOperationException("Constant is expected to be a real value");
        }

        /// <summary>
        /// Gets the constant value as a string value.
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            if (ConstantType == LuaXType.String)
                return (string)Value;

            throw new InvalidOperationException("Constant is expected to be a string value");
        }

        /// <summary>
        /// Gets the constant value as a boolean value.
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            if (ConstantType == LuaXType.String)
                return (bool)Value;

            throw new InvalidOperationException("Constant is expected to be a boolean value");
        }
    }
}
