using System;
using System.Collections.Generic;
using System.Linq;
using Bcasti.CodeAnalysis.Binding;
using Bcasti.CodeAnalysis.Syntax;

namespace Bcasti.CodeAnalysis
{
    public class Compilation
    {
        public SyntaxTree SyntaxTree { get; }

        public Compilation(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var binder = new Binder(variables);
            var boundExpression = binder.Bind(SyntaxTree.Root);
            var diagnostics = SyntaxTree.Diagnostics.Concat(binder.Diagnostics).ToArray();

            if (diagnostics.Any()) 
                return new EvaluationResult(diagnostics, null);
            
            var evaluator = new Evaluator(boundExpression, variables);
            return new EvaluationResult(Array.Empty<Diagnostic>(), evaluator.Evaluate());
        }
    }
}