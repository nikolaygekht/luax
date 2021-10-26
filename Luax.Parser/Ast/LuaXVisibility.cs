namespace Luax.Parser.Ast
{
    /// <summary>
    /// Visibility of class elements
    /// </summary>
    public enum LuaXVisibility
    {
        /// <summary>
        /// Visible inside the class only
        /// </summary>
        Private,
        /// <summary>
        /// Visible to other LuaX classes
        /// </summary>
        Internal,
        /// <summary>
        /// Exported to target platform
        /// </summary>
        Public,
    }
}
