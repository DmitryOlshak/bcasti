using System;
using Bcasti.CodeAnalysis.Binding;

namespace Bcasti.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundExpression _root;

        public Evaluator(BoundExpression root)
        {
            _root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression number)
                return (int)number.Value;

            if (node is BoundUnaryExpression unary)
            {
                var operand = EvaluateExpression(unary.Operand);
                
                if (unary.OperatorKind == BoundUnaryOperatorKind.Negation)
                    return -operand;
                
                if (unary.OperatorKind == BoundUnaryOperatorKind.Identity)
                    return operand;
                
                throw new Exception($"Unexpected unary operator {unary.OperatorKind}");
            }

            if (node is BoundBinaryExpression binary)
            {
                var left = EvaluateExpression(binary.Left);
                var right = EvaluateExpression(binary.Right);
                switch (binary.OperatorKind)
                {
                    case BoundBinaryOperatorKind.Addition:
                        return left + right;
                    case BoundBinaryOperatorKind.Subtraction:
                        return left - right;
                    case BoundBinaryOperatorKind.Multiplication:
                        return left * right;
                    case BoundBinaryOperatorKind.Division:
                        return left / right;
                    default:
                        throw new Exception($"Unexpected binary operator {binary.OperatorKind}");
                }
            }

            throw new Exception($"Unexpected node kind {node.Kind}");
        }
    }
}