using System.Collections.Generic;
using System.Linq;
using Bcasti.CodeAnalysis.Syntax;
using Xunit;

namespace Bcasti.Tests.CodeAnalysis.Syntax
{
    public class LexterTest
    {
        [Theory]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Tokens(SyntaxKind kind, string text)
        {
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }
        
        [Theory]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_TokenPairs(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)
        {
            var text = t1Text + t2Text;

            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.Equal(t1Kind, tokens[0].Kind);
            Assert.Equal(t1Text, tokens[0].Text);
            Assert.Equal(t2Kind, tokens[1].Kind);
            Assert.Equal(t2Text, tokens[1].Text);
        }
        
        [Theory]
        [MemberData(nameof(GetTokenPairsWithSeparatorData))]
        public void Lexer_Lexes_TokenPairsWithSeparator(SyntaxKind t1Kind, string t1Text, 
                                                        SyntaxKind separatorKind, string separatorText, 
                                                        SyntaxKind t2Kind, string t2Text)
        {
            var text = t1Text + separatorText + t2Text;

            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(3, tokens.Length);
            Assert.Equal(t1Kind, tokens[0].Kind);
            Assert.Equal(t1Text, tokens[0].Text);
            Assert.Equal(separatorKind, tokens[1].Kind);
            Assert.Equal(separatorText, tokens[1].Text);
            Assert.Equal(t2Kind, tokens[2].Kind);
            Assert.Equal(t2Text, tokens[2].Text);
        }

        public static IEnumerable<object[]> GetTokensData()
        {
            return GetTokens().Concat(GetSeparators()).Select(tuple => new object[] { tuple.kind, tuple.text });
        }
        
        public static IEnumerable<object[]> GetTokenPairsData()
        {
            return GetTokenPairs().Select(tuple => new object[] { tuple.t1Kind, tuple.t1Text, tuple.t2Kind, tuple.t2Text });
        }
        
        public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
        {
            return GetTokenPairsWithSeparator().Select(tuple => new object[]
                {
                    tuple.t1Kind, tuple.t1Text, tuple.separatorKind, tuple.separatorText, tuple.t2Kind, tuple.t2Text
                });
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
        {
            return new[]
            {
                (SyntaxKind.IdentifierToken, "a"),
                (SyntaxKind.IdentifierToken, "abc"),
                
                (SyntaxKind.NumberToken, "1"),
                (SyntaxKind.NumberToken, "123"),
                
                (SyntaxKind.PlusToken, "+"),
                (SyntaxKind.MinusToken, "-"),
                (SyntaxKind.StarToken, "*"),
                (SyntaxKind.SlashToken, "/"),
                (SyntaxKind.BangToken, "!"),
                (SyntaxKind.EqualsToken, "="),
                (SyntaxKind.AmpersandAmpersandToken, "&&"),
                (SyntaxKind.PipePipeToken, "||"),
                (SyntaxKind.EqualsEqualsToken, "=="),
                (SyntaxKind.BangEqualsToken, "!="),
                (SyntaxKind.OpenParenthesisToken, "("),
                (SyntaxKind.CloseParenthesisToken, ")"),
                (SyntaxKind.TrueKeyword, "true"),
                (SyntaxKind.FalseKeyword, "false")
            };
        }
        
        private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
        {
            return new[]
            {
                (SyntaxKind.WhiteSpaceToken, " "),
                (SyntaxKind.WhiteSpaceToken, "  "),
                (SyntaxKind.WhiteSpaceToken, "\r"),
                (SyntaxKind.WhiteSpaceToken, "\n"),
                (SyntaxKind.WhiteSpaceToken, "\r\n")
            };
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (RequiredSeparator(t1.kind, t2.kind))
                        continue;
                    
                    yield return (t1.kind, t1.text, t2.kind, t2.text);
                }
            }
        }
        
        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, 
                                    SyntaxKind separatorKind, string separatorText,
                                    SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (!RequiredSeparator(t1.kind, t2.kind)) 
                        continue;
                    
                    foreach (var separator in GetSeparators())
                        yield return (t1.kind, t1.text, separator.kind, separator.text, t2.kind, t2.text);
                }
            }
        }

        private static bool RequiredSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
        {
            var t1IsKeyword = t1Kind.ToString().EndsWith("Keyword");
            var t2IsKeyword = t2Kind.ToString().EndsWith("Keyword");
            
            if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.IdentifierToken)
                return true;

            if (t1IsKeyword && t2IsKeyword)
                return true;
            
            if (t1IsKeyword && t2Kind == SyntaxKind.IdentifierToken)
                return true;
            
            if (t1Kind == SyntaxKind.IdentifierToken && t2IsKeyword)
                return true;
            
            if (t1Kind == SyntaxKind.NumberToken && t2Kind == SyntaxKind.NumberToken)
                return true;
            
            if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken)
                return true;
            
            if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsEqualsToken)
                return true;
            
            if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsToken)
                return true;
            
            if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsEqualsToken)
                return true;

            return false;
        }
    }
}