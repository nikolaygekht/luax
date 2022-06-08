using Hime.Redist;

namespace Luax.Parser.Ast
{
    /// <summary>
    /// Wrapper for Hime AST Node
    /// </summary>
    public class AstNodeWrapper : IAstNode
    {
        public string Symbol { get; internal set; }

        public string Value { get; internal set; }

        public int Line { get; internal set; }

        public int Column { get; internal set; }

        public AstNodeCollection Children { get; } = new AstNodeCollection();

        public AstNodeWrapper()
        {
        }

        internal AstNodeWrapper(int line, int column, string symbol, string value)
        {
            Line = line;
            Column = column;
            Symbol = symbol;
            Value = value;
        }

        internal AstNodeWrapper(ASTNode node)
            : this(node.Position.Line, node.Position.Column, node.Symbol.Name, node.Value)
        {
            for (int i = 0; i < node.Children.Count; i++)
                Children.Add(new AstNodeWrapper(node.Children[i]));
        }

        internal void Add(IAstNode child) => Children.Add(child);
    }
}
