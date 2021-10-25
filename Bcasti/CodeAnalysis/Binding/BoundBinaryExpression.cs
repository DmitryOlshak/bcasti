using System;

namespace Bcasti.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundExpression Left { get; }
        public BoundBinaryOperator Operator { get; }
        public BoundExpression Right { get; }
        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
        public override Type Type => Operator.ResultType;
        
        public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator @operator, BoundExpression right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }
    }
}