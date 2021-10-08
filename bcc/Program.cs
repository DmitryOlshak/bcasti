using System;
using System.Collections.Generic;
using System.Linq;

namespace bcc
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;

                var parser = new Parser(line);
                var expression = parser.Parse();
                PrettyPrint(expression);
            }
        }

        static void PrettyPrint(SyntaxNote note, string indent = "", bool isLast = true)
        {
            var nodeChar = isLast ? "└── " : "├── ";
            
            if (string.IsNullOrEmpty(indent))
                Console.Write(nodeChar);
            else
                Console.Write(indent + nodeChar);
            
            Console.Write(note.Kind);

            if (note is SyntaxToken { Value: { } } token)
                Console.Write($" {token.Value}");

            Console.WriteLine();
            
            indent += isLast ? "    " : "│   ";

            using var enumerator = note.GetChildren().GetEnumerator();
            enumerator.MoveNext();
            var current = enumerator.Current;
            while (true)
            {
                var completed = !enumerator.MoveNext();
                
                if (current != null)
                    PrettyPrint(current, indent, isLast: completed);

                current = enumerator.Current;
                
                if (completed) break;
            }
        }
    }

    enum SyntaxKind
    {
        NumberToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        BadToken,
        EndOfFileToken,
        NumberExpression,
        BinaryExpression
    }

    class SyntaxToken : SyntaxNote
    {
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }
        public override SyntaxKind Kind { get; }

        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }
        
        public override IEnumerable<SyntaxNote> GetChildren()
        {
            return Enumerable.Empty<SyntaxNote>();
        }
    }

    class Lexter
    {
        private readonly string _text;
        private int _position;
        
        public Lexter(string text)
        {
            _text = text;
        }

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
                int.TryParse(text, out var value);
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

            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
        }

        private void Next() => _position++;
    }

    abstract class SyntaxNote
    {
        public abstract SyntaxKind Kind { get; }
        public abstract IEnumerable<SyntaxNote> GetChildren();
    }

    abstract class ExpressionSyntax : SyntaxNote
    { }

    sealed class NumberExpressionSyntax : ExpressionSyntax
    {
        public SyntaxToken NumberToken { get; }
        public override SyntaxKind Kind => SyntaxKind.NumberExpression;

        public NumberExpressionSyntax(SyntaxToken numberToken)
        {
            NumberToken = numberToken;
        }
        
        public override IEnumerable<SyntaxNote> GetChildren()
        {
            yield return NumberToken;
        }
    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Right { get; }
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }
        
        public override IEnumerable<SyntaxNote> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }
    
    class Parser
    {
        private readonly SyntaxToken[] _tokens;
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
        }

        private SyntaxToken Current => Peek(0);

        public ExpressionSyntax Parse()
        {
            var left = ParsePrimaryExpression();
            while (Current.Kind == SyntaxKind.PlusToken || Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
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

            return new SyntaxToken(kind, Current.Position, null, null);
        }
    }
}
