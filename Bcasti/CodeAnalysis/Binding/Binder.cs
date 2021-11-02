using System;
using System.Collections.Generic;
using Bcasti.CodeAnalysis.Syntax;

namespace Bcasti.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly Dictionary<string, object> _variables;
        private readonly DiagnosticCollection _diagnostics = new DiagnosticCollection();

        public Binder(Dictionary<string,object> variables)
        {
            _variables = variables;
        }

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
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax)syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var expression = Bind(syntax.Expression);

            object defaultValue = null;
            
            if (expression.Type == typeof(int))
                defaultValue = 0;
            else if (expression.Type == typeof(bool))
                defaultValue = false;

            if (defaultValue == null)
                throw new Exception($"Unsupported variable type: {expression.Type}");

            _variables[name] = defaultValue;
            
            return new BoundAssignmentExpression(name, expression);
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            if (!_variables.TryGetValue(name, out var value))
            {
                _diagnostics.ReportUndefinedIdentifier(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpression(0);
            }

            var type = value.GetType();
            return new BoundVariableExpression(name, type);
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