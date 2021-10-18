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
            var expression = ParseExpression();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(_diagnostics, expression, endOfFileToken);
        }

        private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
        {
            var left = ParsePrimaryExpression();

            while (true)
            {
                var precedence = GetBinaryOperatorPrecedence(Current.Kind);
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                var operatorToken = NextToken();
                var right = ParseExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private static int GetBinaryOperatorPrecedence(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 2;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 1;
                default:
                    return 0;
            }
        }
        
        private ExpressionSyntax ParsePrimaryExpression()
        {
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                var openToken = NextToken();
                var expression = ParseExpression();
                var closeToken = Match(SyntaxKind.CloseParenthesisToken);
                return new ParenthesizedExpressionSyntax(openToken, expression, closeToken);
            }
            
            var numberToken = Match(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
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