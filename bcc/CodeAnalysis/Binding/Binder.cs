using System;
using System.Collections.Generic;
using Bcasti.CodeAnalysis.Syntax;

namespace Bcasti.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly List<string> _diagnostics = new List<string>();

        public IEnumerable<string> Diagnostics => _diagnostics;

        public BoundExpression Bind(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }
        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var left = Bind(syntax.Left);
            var right = Bind(syntax.Right);
            var op = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, left.Type, right.Type);
            
            if (op == null)
            {
                _diagnostics.Add($"Binary operator '{syntax.OperatorToken.Text}' is not define for types {left.Type} and {right.Type}");
                return left;
            }

            return new BoundBinaryExpression(left, op, right);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var operand = Bind(syntax.Operand);
            var op = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, operand.Type);
            
            if (op == null)
            {
                _diagnostics.Add($"Unary operator '{syntax.OperatorToken.Text}' is not define for type {operand.Type}");
                return operand;
            }
            
            return new BoundUnaryExpression(op, operand);
        }
    }
}