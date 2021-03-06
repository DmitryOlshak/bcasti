using System.Collections.Generic;
using System.Linq;

namespace Bcasti.CodeAnalysis.Syntax
{
    public sealed class SyntaxToken : SyntaxNode
    {
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }
        public override SyntaxKind Kind { get; }
        public TextSpan Span { get; }

        public SyntaxToken(SyntaxKind kind, int position, string text, object value = null)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
            Span = new TextSpan(position, text?.Length ?? 0);
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }
}