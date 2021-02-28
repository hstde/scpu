using System;

namespace Sasm
{
    using System.Collections.Generic;
    using System.Linq;
    using Sasm.Parsing;

    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser();
            var lines = new List<string>();

            string line = "lea bc, [hl+constant]";
            int lineNumber = 1;
            while (line != ".")
            {
                Console.Write($"{lineNumber++} ");
                line = Console.ReadLine();
                lines.Add(line);
            }
            var tree = parser.Parse(lines);

            Console.WriteLine($"Parsing done, took {tree.ParseTime.TotalMilliseconds} ms");
            PrintParseTree(tree);
            Console.WriteLine();
        }

        private static void PrintParseTree(ParseTree tree)
        {
            if (tree.HasErrors)
                PrintError(tree);
            else
                PrintNode(tree.Root, 0);
        }

        private static void PrintError(ParseTree tree)
        {
            const string lineString = "in line {0}: ";
            var errors = tree.Messages;
            var lines = tree.SourceLines;

            if (errors.Count > 1)
                Console.WriteLine($"Programm has {errors.Count} errors.");
            else
                Console.WriteLine($"Programm has 1 error.");

            foreach (var e in errors)
            {
                string formattedLine = string.Format(lineString, e.Source.lineNumber + 1);
                var startOfMarker = e.Source.start + formattedLine.Length;
                var markerLength = Math.Max(e.Source.length, 1);

                var marker = string.Concat(
                    new string(' ', startOfMarker),
                    new string('^', markerLength));
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.Write(formattedLine);
                Console.WriteLine(lines[e.Source.lineNumber]);
                Console.WriteLine(marker);
            }
        }

        private static void PrintNode(ParseTreeNode currentNode, int currentIndentation)
        {
            Console.Write(new string('|', currentIndentation));
            Console.Write('-');

            string designator = currentNode.NodeType == ParseTreeNodeType.Terminal ?
                currentNode.Token.TokenType.ToString() :
                currentNode.NodeType.ToString();

            Console.WriteLine(designator);
            if (currentNode.Children != null)
            {
                foreach (var c in currentNode.Children)
                    PrintNode(c, currentIndentation + 1);
            }
        }
    }
}
