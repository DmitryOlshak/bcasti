using System.Collections.Generic;

namespace Bcasti.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private readonly DiagnosticCollection _diagnostics = new DiagnosticCollection();
        private int _position;
        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();
            
            var lexer = new Lexer(text);
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

        public IEnumerable<Diagnostic> Diagnostics => _diagnostics;
        private SyntaxToken Current => Peek(0);

        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(_diagnostics, expression, endOfFileToken);
        }

        private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(Current.Kind);
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence > parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseExpression();
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true)
            {
                var precedence = SyntaxFacts.GetBinaryOperatorPrecedence(Current.Kind);
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                var operatorToken = NextToken();
                var right = ParseExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
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
            
            if (Current.Kind is SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword)
            {
                var booleanToken = NextToken();
                var value = booleanToken.Kind == SyntaxKind.TrueKeyword;
                return new LiteralExpressionSyntax(booleanToken, value);
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

            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }
    }
}