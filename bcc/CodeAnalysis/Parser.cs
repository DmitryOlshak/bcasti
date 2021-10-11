using System.Collections.Generic;
using System.Linq;

namespace Bcasti.CodeAnalysis
{
    internal sealed class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private readonly List<string> _diagnostics = new List<string>();
        private int _position;
        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();
            
            var lexer = new Lexter(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();
                if (token.Kind != SyntaxKind.BadToken && token.Kind != SyntaxKind.WhiteSpaceToken)
                    tokens.Add(token);

            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        public IEnumerable<string> Diagnostics => _diagnostics;
        private SyntaxToken Current => Peek(0);

        public SyntaxTree Parse()
        {
            var expression = ParseTerm();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(_diagnostics, expression, endOfFileToken);
        }

        private ExpressionSyntax ParseTerm()
        {
            var operators = new []
            {
                SyntaxKind.PlusToken, 
                SyntaxKind.MinusToken, 
            };
            
            var left = ParseFactor();
            while (operators.Contains(Current.Kind))
            {
                var operatorToken = NextToken();
                var right = ParseFactor();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }
        
        private ExpressionSyntax ParseFactor()
        {
            var operators = new []
            {
                SyntaxKind.StarToken, 
                SyntaxKind.SlashToken
            };
            
            var left = ParsePrimaryExpression();
            while (operators.Contains(Current.Kind))
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }
        
        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                var openToken = NextToken();
                var expression = ParseTerm();
                var closeToken = Match(SyntaxKind.CloseParenthesisToken);
                return new ParenthesizedExpressionSyntax(openToken, expression, closeToken);
            }
            
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            index = index >= _tokens.Length ? _tokens.Length - 1 : index;
            return _tokens[index];
        }

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken Match(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.Add($"ERROR: unexpected token <{Current.Kind}>, expected <{kind}>");
            return new SyntaxToken(kind, Current.Position, null, null);
        }
    }
}