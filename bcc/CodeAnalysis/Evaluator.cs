using System;
using Bcasti.CodeAnalysis.Syntax;

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
            if (node is LiteralExpressionSyntax number)
                return (int)number.LiteralToken.Value;

            if (node is ParenthesizedExpressionSyntax parentheses)
                return EvaluateExpression(parentheses.Expression);

            if (node is UnaryExpressionSyntax unary)
            {
                var operand = EvaluateExpression(unary.Operand);
                
                if (unary.OperatorToken.Kind == SyntaxKind.MinusToken)
                    return -operand;
                
                if (unary.OperatorToken.Kind == SyntaxKind.PlusToken)
                    return operand;
                
                throw new Exception($"Unexpected unary operator {unary.OperatorToken.Kind}");
            }

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