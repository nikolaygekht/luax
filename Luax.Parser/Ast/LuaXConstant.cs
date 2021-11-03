using System;
using Luax.Parser.Ast.Builder;

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

        public LuaXTypeDefinition ConstantTypeFull => new LuaXTypeDefinition() { TypeId = ConstantType };

        /// <summary>
        /// The constant value.
        /// </summary>
        public object Value { get; internal init; }

        /// <summary>
        /// Checks whether the constant is a `nil` value.
        /// </summary>
        public bool IsNil => ConstantType == LuaXType.Object && Value == null;

        /// <summary>
        /// The location of the element in the source
        /// </summary>
        public LuaXElementLocation Location { get; }

        internal LuaXConstant(LuaXType type, object value, LuaXElementLocation location)
        {
            ConstantType = type;
            Value = value;
            Location = location;
        }

        internal LuaXConstant(int value, LuaXElementLocation location) : this(LuaXType.Integer, value, location)
        {
        }

        internal LuaXConstant(double value, LuaXElementLocation location) : this(LuaXType.Real, value, location)
        {
        }

        internal LuaXConstant(string value, LuaXElementLocation location) : this(LuaXType.String, value, location)
        {
        }

        internal LuaXConstant(bool value, LuaXElementLocation location) : this(LuaXType.Boolean, value, location)
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
