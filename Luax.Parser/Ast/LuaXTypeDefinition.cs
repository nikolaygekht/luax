using System;
using System.Text;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// The definition of the Lua type
    /// </summary>
    public sealed class LuaXTypeDefinition
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

        internal bool IsObject() => TypeId == LuaXType.Object && !Array;

        internal bool IsDate() => TypeId == LuaXType.Datetime && !Array;

        internal bool IsInteger() => TypeId == LuaXType.Integer && !Array;

        internal bool IsReal() => TypeId == LuaXType.Real && !Array;

        internal bool IsNumeric() => IsInteger() || IsReal();

        internal bool IsBoolean() => TypeId == LuaXType.Boolean && !Array;

        internal bool Equals(LuaXTypeDefinition otherType) => TypeId == otherType.TypeId &&
            (Class == otherType.Class || (string.IsNullOrEmpty(Class) && string.IsNullOrEmpty(otherType.Class))) &&
            (Array == otherType.Array);

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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            switch (TypeId)
            {
                case LuaXType.Void:
                    sb.Append("void");
                    break;
                case LuaXType.Integer:
                    sb.Append("int");
                    break;
                case LuaXType.Real:
                    sb.Append("real");
                    break;
                case LuaXType.String:
                    sb.Append("string");
                    break;
                case LuaXType.Boolean:
                    sb.Append("boolean");
                    break;
                case LuaXType.Object:
                    sb.Append(Class ?? "object");
                    break;
                case LuaXType.Datetime:
                    sb.Append("datetime");
                    break;
                case LuaXType.ClassName:
                    sb.Append("typeof(").Append(Class ?? "object").Append(')');
                    break;
            }
            if (Array)
                sb.Append("[]");
            return sb.ToString();
        }
    }
}

