namespace Luax.Parser.Ast
{
    /// <summary>
    /// Body of a LuaX source file
    /// </summary>
    public class LuaXBody
    {
        /// <summary>
        /// The name of the source
        /// </summary>
        public string Name { get; internal init; }

        /// <summary>
        /// The collection of classes defined in the source file.
        /// </summary>
        public LuaXClassCollection Classes { get; } = new LuaXClassCollection();

        /// <summary>
        /// The collection of packages defined in the source file.
        /// </summary>
        public LuaXAstNamedCollection<LuaXPackage> Packages { get; } = new LuaXAstNamedCollection<LuaXPackage>();

        internal LuaXBody(string name)
        {
            Name = name;
        }
    }
}
