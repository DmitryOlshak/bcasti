using System;
using System.Collections.Generic;
using System.Linq;
using Bcasti.CodeAnalysis.Syntax;
using Xunit;

namespace Bcasti.Tests.CodeAnalysis.Syntax
{
    public class ParserTest
    {
        private sealed class AssertingEnumerator : IDisposable
        {
            private readonly IEnumerator<SyntaxNode> _enumerator;
            private bool _hasError = false;

            public AssertingEnumerator(SyntaxNode node)
            {
                _enumerator = Flatten(node).GetEnumerator();
            }

            public void AssertToken(SyntaxKind kind, string text)
            {
                try
                {
                    Assert.True(_enumerator.MoveNext());
                    Assert.Equal(kind, _enumerator.Current?.Kind);
                    var token = Assert.IsType<SyntaxToken>(_enumerator.Current);
                    Assert.Equal(text, token.Text);
                }
                catch
                {
                    _hasError = true;
                    throw;
                }
            }
            
            public void AssertNode(SyntaxKind kind)
            {
                try
                {
                    Assert.True(_enumerator.MoveNext());
                    Assert.Equal(kind, _enumerator.Current?.Kind);
                    Assert.IsNotType<SyntaxToken>(_enumerator.Current);
                }
                catch
                {
                    _hasError = true;
                    throw;
                }
            }

            private static IEnumerable<SyntaxNode> Flatten(SyntaxNode node)
            {
                var stack = new Stack<SyntaxNode>();
                stack.Push(node);

                while (stack.Count > 0)
                {
                    var n = stack.Pop();
                    yield return n;

                    foreach (var child in n.GetChildren().Reverse())
                        stack.Push(child);
                }
            }

            public void Dispose()
            {
                if (!_hasError)
                    Assert.False(_enumerator.MoveNext());
                _enumerator?.Dispose();
            }
        }
        
        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parser_BinaryExpression_HonorsPrecedences(SyntaxKind op1, SyntaxKind op2)
        {
            var op1Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op1);
            var op2Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op2);
            var op1Text = SyntaxFacts.GetText(op1);
            var op2Text = SyntaxFacts.GetText(op2);
            var text = $"a {op1Text} b {op2Text} c";
            var expression = SyntaxTree.Parse(text).Root;
            
            if (op1Precedence >= op2Precedence)
            {
                using (var enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(op1, op1Text);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                    enumerator.AssertToken(op2, op2Text);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
            }
            else
            {
                using (var enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(op1, op1Text);
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                    enumerator.AssertToken(op2, op2Text);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
            }

        }
        
        [Theory]
        [MemberData(nameof(GetUnaryOperatorPairsData))]
        public void Parser_UnaryExpression_HonorsPrecedences(SyntaxKind unaryKind, SyntaxKind binaryKind)
        {
            var unaryPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(unaryKind);
            var binaryPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(binaryKind);
            var unaryText = SyntaxFacts.GetText(unaryKind);
            var binaryText = SyntaxFacts.GetText(binaryKind);
            var text = $"{unaryText} a {binaryText} b";
            var expression = SyntaxTree.Parse(text).Root;
            
            if (unaryPrecedence >= binaryPrecedence)
            {
                using (var enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.UnaryExpression);
                    enumerator.AssertToken(unaryKind, unaryText);
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(binaryKind, binaryText);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
            }
            else
            {
                using (var enumerator = new AssertingEnumerator(expression))
                {
                    enumerator.AssertNode(SyntaxKind.BinaryExpression);
                    enumerator.AssertNode(SyntaxKind.UnaryExpression);
                    enumerator.AssertToken(unaryKind, unaryText);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
                    enumerator.AssertToken(binaryKind, binaryText);
                    enumerator.AssertNode(SyntaxKind.NameExpression);
                    enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
                }
            }

        }

        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            foreach (var op1 in SyntaxFacts.GetBinaryOperatorKinds())
            {
                foreach (var op2 in SyntaxFacts.GetBinaryOperatorKinds())
                {
                    yield return new object[] { op1, op2 };
                }
            }
        }
        
        public static IEnumerable<object[]> GetUnaryOperatorPairsData()
        {
            foreach (var unaryKind in SyntaxFacts.GetUnaryOperatorKinds())
            {
                foreach (var binaryKind in SyntaxFacts.GetBinaryOperatorKinds())
                {
                    yield return new object[] { unaryKind, binaryKind };
                }
            }
        }
    }
}