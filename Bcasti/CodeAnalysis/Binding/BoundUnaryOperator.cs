using System;
using System.Linq;
using Bcasti.CodeAnalysis.Syntax;

namespace Bcasti.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryOperator
    {
        private static BoundUnaryOperator[] _operators = new[]
        {
            new BoundUnaryOperator(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, typeof(bool)),
            new BoundUnaryOperator(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, typeof(int)),
            new BoundUnaryOperator(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, typeof(int)),
        };
        
        public SyntaxKind SyntaxKind { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public Type OperandType { get; }
        public Type ResultType { get; }

        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType)
            : this(syntaxKind, kind, operandType, operandType)
        {
        }
        
        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType, Type resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            OperandType = operandType;
            ResultType = resultType;
        }

        public static BoundUnaryOperator Bind(SyntaxKind syntaxKind, Type operandType)
            => _operators.FirstOrDefault(op => op.SyntaxKind == syntaxKind && op.OperandType == operandType);
    }
}