using System;
using System.Text;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// The definition of the Lua type
    /// </summary>
    public class LuaXTypeDefinition
    {
        public static LuaXTypeDefinition String { get; } = new LuaXTypeDefinition() { TypeId = LuaXType.String };
        public static LuaXTypeDefinition Integer { get; } = new LuaXTypeDefinition() { TypeId = LuaXType.Integer };
        public static LuaXTypeDefinition Real { get; } = new LuaXTypeDefinition() { TypeId = LuaXType.Real };
        public static LuaXTypeDefinition Boolean { get; } = new LuaXTypeDefinition() { TypeId = LuaXType.Boolean };

        /// <summary>
        /// The lua type.
        /// </summary>
        public LuaXType TypeId { get; internal set; }

        /// <summary>
        /// The flag indicating whether the variable is an array
        /// </summary>
        public bool Array { get; internal set; }
        /// <summary>
        /// The class name if LuaType is a class
        /// </summary>
        public string Class { get; internal set; }

        internal bool IsArrayOf(LuaXType type, string className = null)
        {
            if (TypeId != type || !Array)
                return false;
            if (!string.IsNullOrEmpty(className) && className != Class)
                return false;
            return true;
        }

        internal bool IsString() => TypeId == LuaXType.String && !Array;

        internal bool IsDate() => TypeId == LuaXType.Datetime && !Array;

        internal bool IsInteger() => TypeId == LuaXType.Integer && !Array;

        internal bool IsReal() => TypeId == LuaXType.Real && !Array;

        internal bool IsNumeric() => IsInteger() || IsReal();

        internal bool IsBoolean() => TypeId == LuaXType.Boolean && !Array;

        public LuaXTypeDefinition ArrayElementType() => new LuaXTypeDefinition()
        {
            TypeId = this.TypeId,
            Class = this.Class,
            Array = false
        };

        public LuaXTypeDefinition ArrayOf() => new LuaXTypeDefinition()
        {
            TypeId = this.TypeId,
            Class = this.Class,
            Array = true
        };
    }
}

