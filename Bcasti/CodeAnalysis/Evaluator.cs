using System;
using System.Collections.Generic;
using Bcasti.CodeAnalysis.Binding;

namespace Bcasti.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundExpression _root;
        private readonly Dictionary<string, object> _variables;

        public Evaluator(BoundExpression root, Dictionary<string, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression literal)
                return literal.Value;

            if (node is BoundVariableExpression variable)
                return _variables[variable.Name];

            if (node is BoundAssignmentExpression assignment)
            {
                var value = EvaluateExpression(assignment.Expression);
                _variables[assignment.Name] = value;
                return value;
            }
            
            if (node is BoundUnaryExpression unary)
            {
                var operand = EvaluateExpression(unary.Operand);

                return unary.Operator.Kind switch
                {
                    BoundUnaryOperatorKind.Negation => -(int)operand,
                    BoundUnaryOperatorKind.Identity => operand,
                    BoundUnaryOperatorKind.LogicalNegation => !(bool)operand,
                    _ => throw new Exception($"Unexpected unary operator {unary.Operator}")
                };
            }

            if (node is BoundBinaryExpression binary)
            {
                var left = EvaluateExpression(binary.Left);
                var right = EvaluateExpression(binary.Right);
                switch (binary.Operator.Kind)
                {
                    case BoundBinaryOperatorKind.Addition:
                        return (int)left + (int)right;
                    case BoundBinaryOperatorKind.Subtraction:
                        return (int)left - (int)right;
                    case BoundBinaryOperatorKind.Multiplication:
                        return (int)left * (int)right;
                    case BoundBinaryOperatorKind.Division:
                        return (int)left / (int)right;
                    case BoundBinaryOperatorKind.LogicalAnd:
                        return (bool)left && (bool)right;
                    case BoundBinaryOperatorKind.LogicalOr:
                        return (bool)left || (bool)right;
                    case BoundBinaryOperatorKind.Equals:
                        return Equals(left, right);
                    case BoundBinaryOperatorKind.NotEquals:
                        return !Equals(left, right);
                    default:
                        throw new Exception($"Unexpected binary operator {binary.Operator}");
                }
            }

            throw new Exception($"Unexpected node kind {node.Kind}");
        }
    }
}