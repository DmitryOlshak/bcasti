using System.Collections.Generic;

namespace Bcasti.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly string _text;
        private int _position;
        private readonly List<string> _diagnostics = new List<string>();

        public Lexer(string text)
        {
            _text = text;
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

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
                    _diagnostics.Add($"ERROR: the number {text} isn't valid Int");
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
                    return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+");
                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-");
                case '*':
                    return new SyntaxToken(SyntaxKind.StarToken, _position++, "*");
                case '/':
                    return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/");
                case '(':
                    return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(");
                case ')':
                    return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")");
                case '&' when Lookahead == '&':
                    return new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, _position += 2, "&&");
                case '|' when Lookahead == '|':
                    return new SyntaxToken(SyntaxKind.PipePipeToken, _position += 2, "||");
                case '=' when Lookahead == '=':
                    return new SyntaxToken(SyntaxKind.EqualsEqualsToken, _position += 2, "==");
                case '!':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.BangEqualsToken, _position += 2, "!=");
                    return new SyntaxToken(SyntaxKind.BangToken, _position++, "!");
                default:
                    _diagnostics.Add($"ERROR: bad character input: {Current}");
                    return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
            }
        }

        private void Next() => _position++;

        private char Peek(int offset)
        {
            var index = _position + offset;
            return index >= _text.Length ? '\0' : _text[index];
        }
    }
}