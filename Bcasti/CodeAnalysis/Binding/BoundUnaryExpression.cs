using System;

namespace Bcasti.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryOperator Operator { get; }
        public BoundExpression Operand { get; }
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override Type Type => Operator.ResultType;
        
        public BoundUnaryExpression(BoundUnaryOperator @operator, BoundExpression operand)
        {
            Operator = @operator;
            Operand = operand;
        }
    }
}