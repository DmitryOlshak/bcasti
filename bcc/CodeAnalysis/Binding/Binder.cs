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
            var value = syntax.LiteralToken.Value as int? ?? 0;
            return new BoundLiteralExpression(value);
        }
        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var left = Bind(syntax.Left);
            var right = Bind(syntax.Right);
            var operatorKind = BindBinaryOperatorKind(syntax.OperatorToken.Kind, left.Type, right.Type);
            
            if (operatorKind == null)
            {
                _diagnostics.Add($"Binary operator '{syntax.OperatorToken.Text}' is not define for types {left.Type} and {right.Type}");
                return left;
            }
            
            return new BoundBinaryExpression(left, operatorKind.Value, right);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var operand = Bind(syntax.Operand);
            var operatorKind = BindUnaryOperatorKind(syntax.OperatorToken.Kind, operand.Type);
            
            if (operatorKind == null)
            {
                _diagnostics.Add($"Unary operator '{syntax.OperatorToken.Text}' is not define for type {operand.Type}");
                return operand;
            }
            
            return new BoundUnaryExpression(operatorKind.Value, operand);
        }

        private static BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type leftType, Type rightType)
        {
            if (leftType != typeof(int) || rightType != typeof(int))
                return null;
            
            return kind switch
            {
                SyntaxKind.PlusToken => BoundBinaryOperatorKind.Addition,
                SyntaxKind.MinusToken => BoundBinaryOperatorKind.Subtraction,
                SyntaxKind.StarToken => BoundBinaryOperatorKind.Multiplication,
                SyntaxKind.SlashToken => BoundBinaryOperatorKind.Division,
                _ => throw new Exception($"Unexpected binary operator {kind}"),
            };
        }
        
        private static BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operandType)
        {
            if (operandType != typeof(int))
                return null;
            
            return kind switch
            {
                SyntaxKind.PlusToken => BoundUnaryOperatorKind.Identity,
                SyntaxKind.MinusToken => BoundUnaryOperatorKind.Negation,
                _ => throw new Exception($"Unexpected unary operator {kind}"),
            };
        }
    }
}