using System;
using System.Collections;
using System.Collections.Generic;
using Bcasti.CodeAnalysis.Syntax;

namespace Bcasti.CodeAnalysis
{
    internal sealed class DiagnosticCollection : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(IEnumerable<Diagnostic> diagnostics)
        {
            _diagnostics.AddRange(diagnostics);
        }
        
        public void ReportInvalidNumber(TextSpan span, string text, Type type)
        {
            Report(span, $"The number {text} isn't valid {type}");
        }
        
        public void ReportBadCharacter(int position, char badChar)
        {
            Report(new TextSpan(position, 1), $"Bad character input {badChar}");
        }
        
        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            Report(span, $"Unexpected token <{actualKind}>, expected <{expectedKind}>");
        }
        
        public void ReportUndefinedBinaryOperator(TextSpan span, string text, Type leftType, Type rightType)
        {
            Report(span, $"Binary operator '{text}' is not define for types {leftType} and {rightType}");
        }
        
        public void ReportUndefinedUnaryOperator(TextSpan span, string text, Type operandType)
        {
            Report(span, $"Unary operator '{text}' is not define for type {operandType}");
        }
        
        public void ReportUndefinedIdentifier(TextSpan span, string name)
        {
            Report(span, $"Variable '{name}' doesn't exist");
        }
        
        private void Report(TextSpan span, string message)
        {
            _diagnostics.Add(new Diagnostic(span, message));
        }

        
    }
}