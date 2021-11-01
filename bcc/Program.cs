using System;
using System.Collections.Generic;
using System.Linq;
using Bcasti.CodeAnalysis;
using Bcasti.CodeAnalysis.Syntax;

namespace Bcasti
{
    internal static class Program
    {
        private static void Main()
        {
            var variables = new Dictionary<string, object>();
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
                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(variables);

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrintNode(syntaxTree.Root);
                    Console.ResetColor();
                }
                
                if (!result.Diagnostics.Any())
                {
                    Console.WriteLine(result.Value);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Console.WriteLine();
                        Console.WriteLine(diagnostic);

                        Console.ResetColor();
                        var prefix = line.Substring(0, diagnostic.Span.Start);
                        Console.Write("    " + prefix);
                        
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        var error = line.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                        Console.Write(error);
                        
                        Console.ResetColor();
                        var postfix = line.Substring(diagnostic.Span.End);
                        Console.WriteLine(postfix);
                    }
                    
                    Console.WriteLine();
                    Console.ResetColor();
                }
            }
        }

        static void PrintNode(SyntaxNode node, string indent = "", bool isLast = true)
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
                    PrintNode(current, indent, isLast: completed);

                current = enumerator.Current;
                
                if (completed) break;
            }
        }
    }
}
