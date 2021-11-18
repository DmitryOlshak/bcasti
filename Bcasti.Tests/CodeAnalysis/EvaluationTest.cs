using System.Collections.Generic;
using Bcasti.CodeAnalysis;
using Bcasti.CodeAnalysis.Syntax;
using Xunit;

namespace Bcasti.Tests.CodeAnalysis
{
    public class EvaluationTest
    {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("+1", 1)]
        [InlineData("-1", -1)]
        [InlineData("1 + 2", 3)]
        [InlineData("1 - 2", -1)]
        [InlineData("1 * 2", 2)]
        [InlineData("6 / 2", 3)]
        [InlineData("(4)", 4)]
        [InlineData("10 == 4", false)]
        [InlineData("10 == 10", true)]
        [InlineData("10 != 10", false)]
        [InlineData("10 != 4", true)]
        [InlineData("false == false", true)]
        [InlineData("true == false", false)]
        [InlineData("true != false", true)]
        [InlineData("true != true", false)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        [InlineData("(a = 10) * a", 100)]
        public void Evaluation_Evaluate_Result(string expression, object expectedValue)
        {
            var compilation = new Compilation(SyntaxTree.Parse(expression));
            var variables = new Dictionary<VariableSymbol, object>();

            var evaluationResult = compilation.Evaluate(variables);
            
            Assert.Empty(evaluationResult.Diagnostics);
            Assert.Equal(expectedValue, evaluationResult.Value);
        }
    }
}