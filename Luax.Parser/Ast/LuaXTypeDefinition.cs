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
        public static LuaXTypeDefinition Datetime { get; } = new LuaXTypeDefinition() { TypeId = LuaXType.Datetime };
        public static LuaXTypeDefinition Boolean { get; } = new LuaXTypeDefinition() { TypeId = LuaXType.Boolean };
        public static LuaXTypeDefinition Void { get; } = new LuaXTypeDefinition() { TypeId = LuaXType.Void };

        /// <summary>
        /// The lua type.
        /// </summary>
        public LuaXType TypeId { get; init; }

        /// <summary>
        /// The flag indicating whether the variable is an array
        /// </summary>
        public bool Array { get; init; }
        /// <summary>
        /// The class name if LuaType is a class
        /// </summary>
        public string Class { get; init; }

        public bool IsArrayOf(LuaXType type, string className = null)
        {
            if (TypeId != type || !Array)
                return false;
            if (!string.IsNullOrEmpty(className) && className != Class)
                return false;
            return true;
        }

        public bool IsString() => TypeId == LuaXType.String && !Array;

        public bool IsObject() => TypeId == LuaXType.Object && !Array;

        public bool IsDate() => TypeId == LuaXType.Datetime && !Array;

        public bool IsInteger() => TypeId == LuaXType.Integer && !Array;

        public bool IsVoid() => TypeId == LuaXType.Void && !Array;

        public bool IsReal() => TypeId == LuaXType.Real && !Array;

        public bool IsNumeric() => IsInteger() || IsReal();

        public bool IsBoolean() => TypeId == LuaXType.Boolean && !Array;

        public bool IsTheSame(LuaXTypeDefinition otherType) => TypeId == otherType.TypeId &&
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

        public bool IsNullable() => Array || TypeId == LuaXType.String || TypeId == LuaXType.Object;

        public object DefaultValue()
        {
            if (Array)
                return null;

            if (TypeId == LuaXType.Integer)
                return 0;
            if (TypeId == LuaXType.Real)
                return 0.0;
            if (TypeId == LuaXType.Boolean)
                return false;
            if (TypeId == LuaXType.Datetime)
                return new DateTime(1900, 1, 1);
            return null;
        }

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

