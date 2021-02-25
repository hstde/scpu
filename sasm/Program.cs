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

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                var tokens = tokenizer.Tokenize(line);
                var tree = parser.ParseTokenList(tokens);

                PrintParseTree(line, tree);
                Console.WriteLine();
            }
        }

        private static void PrintParseTree(string line, Parsing.ParseTree.ParseTree tree)
        {
            var currentNode = tree.root;
            int currentIndentation = 0;

            if (tree.HasErrors)
                PrintError(line, tree);
            else
                PrintNode(currentNode, currentIndentation);
        }

        private static void PrintError(string line, ParseTree tree)
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
                Console.Write(formattedLine);
                Console.WriteLine(line);
                Console.WriteLine(marker);
                Console.WriteLine(e.ErrorMessage);
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
