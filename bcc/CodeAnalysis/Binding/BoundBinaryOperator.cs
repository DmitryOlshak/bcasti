using System;
using System.Linq;
using Bcasti.CodeAnalysis.Syntax;

namespace Bcasti.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private static BoundBinaryOperator[] _operators = new[]
        {
            new BoundBinaryOperator(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool)),
        };
        
        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type ResultType { get; }

        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type type)
            : this(syntaxKind, kind, type, type, type)
        {
        }
        
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type leftType, Type rightType, Type resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }

        public static BoundBinaryOperator Bind(SyntaxKind syntaxKind, Type leftType, Type rightType)
        {
            return _operators.FirstOrDefault(@operator => @operator.SyntaxKind == syntaxKind 
                                                        && @operator.LeftType == leftType
                                                        && @operator.RightType == rightType);
        }
    }
}