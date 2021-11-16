using System;
using System.Collections.Generic;
using System.Linq;
using Bcasti.CodeAnalysis.Syntax;
using Xunit;

namespace Bcasti.Tests.CodeAnalysis.Syntax
{
    public class SyntaxFactTest
    {
        [Theory]
        [MemberData(nameof(GetSyntaxKindData))]
        public void SyntaxFact_GetText_RoundTrips(SyntaxKind kind)
        {
            var text = SyntaxFacts.GetText(kind);
            if (text is null) return;

            var tokens = SyntaxTree.ParseTokens(text);
            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        public static IEnumerable<object[]> GetSyntaxKindData()
        {
            var kinds = Enum.GetValues(typeof(SyntaxKind)) as SyntaxKind[];
            return kinds!.Select(kind => new object[] { kind });
        }
    }
}