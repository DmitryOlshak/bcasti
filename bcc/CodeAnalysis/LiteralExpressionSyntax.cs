﻿using System.Collections.Generic;

namespace Bcasti.CodeAnalysis
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken LiteralToken { get; }
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

        public LiteralExpressionSyntax(SyntaxToken literalToken)
        {
            LiteralToken = literalToken;
        }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LiteralToken;
        }
    }
}