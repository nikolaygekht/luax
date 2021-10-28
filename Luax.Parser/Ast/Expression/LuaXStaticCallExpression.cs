namespace Luax.Parser.Ast.LuaExpression
{
    /// <summary>
    /// Call of a static method
    /// </summary>
    public class LuaXStaticCallExpression : LuaXCallExpression
    {
        /// <summary>
        /// The name of the class to call
        /// </summary>
        public string ClassName { get; }

        internal LuaXStaticCallExpression(LuaXTypeDefinition returnType, string className, string methodName, LuaXElementLocation location)
            : base(methodName, returnType, location)
        {
            ClassName = className;
        }

        public override string ToString() => $"call:{ClassName}::{MethodName}({Arguments.ToString()})";
    }
}
