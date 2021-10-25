using System.Collections.Generic;

namespace Bcasti.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly string _text;
        private int _position;
        private readonly DiagnosticCollection _diagnostics = new DiagnosticCollection();

        public Lexer(string text)
        {
            _text = text;
        }

        public IEnumerable<Diagnostic> Diagnostics => _diagnostics;

        private char Current => Peek(0);
        private char Lookahead => Peek(1);

        public SyntaxToken NextToken()
        {
            if (_position >= _text.Length)
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0");
            
            if (char.IsDigit(Current))
            {
                var start = _position;
                while (char.IsDigit(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                if (!int.TryParse(text, out var value))
                {
                    _diagnostics.ReportInvalidNumber(new TextSpan(start, length), text, typeof(int));
                }
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;
                while (char.IsWhiteSpace(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text);
            }

            if (char.IsLetter(Current))
            {
                var start = _position;
                while (char.IsLetter(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                var syntaxKind = SyntaxFacts.GetKeywordKind(text);
                return new SyntaxToken(syntaxKind, start, text);
            }

            switch (Current)
            {
                case '+':
                    return new SyntaxToken(SyntaxKind.PlusToken, Next(), "+");
                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, Next(), "-");
                case '*':
                    return new SyntaxToken(SyntaxKind.StarToken, Next(), "*");
                case '/':
                    return new SyntaxToken(SyntaxKind.SlashToken, Next(), "/");
                case '(':
                    return new SyntaxToken(SyntaxKind.OpenParenthesisToken, Next(), "(");
                case ')':
                    return new SyntaxToken(SyntaxKind.CloseParenthesisToken, Next(), ")");
                case '&' when Lookahead == '&':
                    return new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, Next(2), "&&");
                case '|' when Lookahead == '|':
                    return new SyntaxToken(SyntaxKind.PipePipeToken, Next(2), "||");
                case '=' when Lookahead == '=':
                    return new SyntaxToken(SyntaxKind.EqualsEqualsToken, Next(2), "==");
                case '!':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.BangEqualsToken, Next(2), "!=");
                    return new SyntaxToken(SyntaxKind.BangToken, Next(), "!");
                default:
                    _diagnostics.ReportBadCharacter(_position, Current);
                    return new SyntaxToken(SyntaxKind.BadToken, Next(), _text.Substring(_position - 1, 1), null);
            }
        }

        private int Next(int shift = 1)
        {
            var position = _position;
            _position += shift;
            return position;
        }

        private char Peek(int offset)
        {
            var index = _position + offset;
            return index >= _text.Length ? '\0' : _text[index];
        }
    }
}