﻿using System.Collections.Generic;

namespace Bcasti.CodeAnalysis
{
    class Lexter
    {
        private readonly string _text;
        private int _position;
        private readonly List<string> _diagnostics = new List<string>();

        public Lexter(string text)
        {
            _text = text;
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

        private char Current => _position >= _text.Length ? '\0' : _text[_position];

        public SyntaxToken NextToken()
        {
            if (_position >= _text.Length)
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
            
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
                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, null);
            }

            if (Current == '+')
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
            
            if (Current == '-')
                return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
            
            if (Current == '*')
                return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null);
            
            if (Current == '/')
                return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);
            
            if (Current == '(')
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
            
            if (Current == ')') 
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null);

            _diagnostics.Add($"ERROR: bad character input: {Current}");
            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
        }

        private void Next() => _position++;
    }
}