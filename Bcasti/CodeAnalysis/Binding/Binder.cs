using System;
using System.Collections.Generic;
using Bcasti.CodeAnalysis.Syntax;

namespace Bcasti.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticCollection _diagnostics = new DiagnosticCollection();

        public IEnumerable<Diagnostic> Diagnostics => _diagnostics;

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
                case SyntaxKind.ParenthesizedExpression:
                    return Bind(((ParenthesizedExpressionSyntax)syntax).Expression);
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
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, left.Type, right.Type);
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
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, operand.Type);
                return operand;
            }
            
            return new BoundUnaryExpression(op, operand);
        }
    }
}