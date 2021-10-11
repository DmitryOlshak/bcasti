using System;

namespace Bcasti.CodeAnalysis
{
    public sealed class Evaluator
    {
        private readonly ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax root)
        {
            _root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionSyntax node)
        {
            if (node is NumberExpressionSyntax number)
                return (int)number.NumberToken.Value;

            if (node is ParenthesizedExpressionSyntax parentheses)
                return EvaluateExpression(parentheses.Expression);

            if (node is BinaryExpressionSyntax binary)
            {
                var left = EvaluateExpression(binary.Left);
                var right = EvaluateExpression(binary.Right);
                switch (binary.OperatorToken.Kind)
                {
                    case SyntaxKind.PlusToken:
                        return left + right;
                    case SyntaxKind.MinusToken:
                        return left - right;
                    case SyntaxKind.StarToken:
                        return left * right;
                    case SyntaxKind.SlashToken:
                        return left / right;
                    default:
                        throw new Exception($"Unexpected binary operator {binary.OperatorToken.Kind}");
                }
            }

            throw new Exception($"Unexpected node kind {node.Kind}");
        }
    }
}