namespace Luax.Parser.Ast
{
    /// <summary>
    /// Abstraction of the AST node
    /// </summary>
    public interface IAstNode
    {
        /// <summary>
        /// Element symbol
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// Element value
        /// </summary>
        string Value { get; }

        /// <summary>
        /// The line of the element location in the source
        /// </summary>
        int Line { get; }

        /// <summary>
        /// The column of the element location in the source
        /// </summary>
        int Column { get; }

        /// <summary>
        /// The children nodes
        /// </summary>
        AstNodeCollection Children { get; }
    }
}
