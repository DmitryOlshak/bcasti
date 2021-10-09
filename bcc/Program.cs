using System;
using System.Linq;
using Bcasti.CodeAnalysis;

namespace Bcasti
{
    class Program
    {
        static void Main(string[] args)
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

                if (showTree)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrettyPrint(syntaxTree.Root);
                    Console.ForegroundColor = color;
                }
                
                if (!syntaxTree.Diagnostics.Any())
                {
                    var evaluator = new Evaluator(syntaxTree.Root);
                    Console.WriteLine(evaluator.Evaluate());
                }
                else
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diagnostic in syntaxTree.Diagnostics)
                        Console.WriteLine(diagnostic);
                    Console.ForegroundColor = color;
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
