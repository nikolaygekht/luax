using Luax.Parser.Ast;
using Luax.Parser.Ast.LuaExpression;

namespace Luax.Interpreter.Infrastructure
{
    /// <summary>
    /// An array of LuaX interpreter variables
    /// </summary>
    public class LuaXVariableInstanceArray
    {
        public LuaXTypeDefinition ArrayType { get; }
        public LuaXTypeDefinition ElementType { get; }
        public int Length { get; }
        private readonly LuaXVariableInstance[] mVariables;

        public LuaXVariableInstanceArray(LuaXTypeDefinition arrayType, int size)
        {
            ArrayType = arrayType;
            ElementType = ArrayType.ArrayElementType();
            mVariables = new LuaXVariableInstance[size];
            for (int i = 0; i < size; i++)
                mVariables[i] = new LuaXVariableInstance("", ElementType);
            Length = size;
        }

        public LuaXVariableInstanceArray(LuaXTypeDefinition arrayType, object[] objects)
        {
            Length = objects.Length;
            ArrayType = arrayType;
            ElementType = ArrayType.ArrayElementType();
            mVariables = new LuaXVariableInstance[Length];
            for (int i = 0; i < Length; i++)
            {
                mVariables[i] = new LuaXVariableInstance("", ElementType);
                mVariables[i].Value = objects[i];
            }
        }

        /// <summary>
        /// Gets or sets the array item
        ///
        /// Returns `null` if there is no variable with such name is defined.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LuaXVariableInstance this[int index]
        {
            get => mVariables[index];
            set => mVariables[index] = value;
        }
    }
}
