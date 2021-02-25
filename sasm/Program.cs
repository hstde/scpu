using System;

namespace Sasm
{
    using System.Collections.Generic;
    using System.Linq;
    using Sasm.Parsing;
    using Sasm.Parsing.ParseTree;
    using Sasm.Tokenizing;

    class Program
    {
        static void Main(string[] args)
        {
            var tokenizer = new Tokenizer();
            var parser = new Parser();
            var lines = new List<string>();

            string line = "";
            int lineNumber = 1;
            while (line != ".")
            {
                Console.Write($"{lineNumber++} ");
                line = Console.ReadLine();
                lines.Add(line);
            }


            var tokens = tokenizer.Tokenize(string.Join("\n", lines));
            var tree = parser.ParseTokenList(tokens);

            PrintParseTree(lines, tree);
            Console.WriteLine();
        }

        private static void PrintParseTree(IReadOnlyList<string> lines, Parsing.ParseTree.ParseTree tree)
        {
            var currentNode = tree.root;
            int currentIndentation = 0;

            if (tree.HasErrors)
                PrintError(lines, tree);
            else
                PrintNode(currentNode, currentIndentation);
        }

        private static void PrintError(IReadOnlyList<string> lines, ParseTree tree)
        {
            const string lineString = "in line {0}: ";
            var errors = ErrorHelper.CollectErrors(tree).ToArray();

            if (errors.Length > 1)
                Console.WriteLine($"Programm has {errors.Length} errors.");
            else
                Console.WriteLine($"Programm has 1 error.");

            foreach (var e in errors)
            {
                string formattedLine = string.Format(lineString, e.sourceReference.lineNumber + 1);
                var startOfMarker = e.sourceReference.start + formattedLine.Length;
                var markerLength = Math.Max(e.sourceReference.length, 1);

                var marker = string.Concat(
                    new string(' ', startOfMarker),
                    new string('^', markerLength));
                Console.WriteLine();
                Console.WriteLine(e.ErrorMessage);
                Console.Write(formattedLine);
                Console.WriteLine(lines[e.sourceReference.lineNumber]);
                Console.WriteLine(marker);
            }
        }

        private static void PrintNode(Parsing.ParseTree.ParseTreeNode currentNode, int currentIndentation)
        {
            Console.Write(new string(' ', currentIndentation));
            Console.WriteLine(currentNode);
            if (currentNode.Children != null)
            {
                foreach (var c in currentNode.Children)
                    PrintNode(c, currentIndentation + 2);
            }
        }
    }
}
