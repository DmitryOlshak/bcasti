using System;
using System.Linq;
using Bcasti.CodeAnalysis;
using Bcasti.CodeAnalysis.Binding;
using Bcasti.CodeAnalysis.Syntax;

namespace Bcasti
{
    internal static class Program
    {
        private static void Main()
        {
            var showTree = false;
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    return;

                if (line == "#showTree")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing parsed trees" : "Not showing parsed trees");
                    continue;
                }
                
                var syntaxTree = SyntaxTree.Parse(line);
                var binder = new Binder();
                var boundExpression = binder.Bind(syntaxTree.Root);
                var diagnostics = syntaxTree.Diagnostics.Concat(binder.Diagnostics).ToArray();

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrettyPrint(syntaxTree.Root);
                    Console.ResetColor();
                }
                
                if (!diagnostics.Any())
                {
                    var evaluator = new Evaluator(boundExpression);
                    Console.WriteLine(evaluator.Evaluate());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diagnostic in syntaxTree.Diagnostics)
                        Console.WriteLine(diagnostic);
                    Console.ResetColor();
                }
            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var nodeChar = isLast ? "└── " : "├── ";

            Console.Write(indent + nodeChar);
            Console.Write(node.Kind);

            if (node is SyntaxToken { Value: { } } token)
                Console.Write($" {token.Value}");

            Console.WriteLine();
            
            indent += isLast ? "    " : "│   ";

            using var enumerator = node.GetChildren().GetEnumerator();
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
}
