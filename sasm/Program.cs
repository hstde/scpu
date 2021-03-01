using System;

namespace Sasm
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Irony.Parsing;
    using Parser = Sasm.Parsing.Parser;

    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser();
            var lines = new StringBuilder();

            string line = "lea bc, [hl+constant]";
            int lineNumber = 1;
            while (true)
            {
                Console.Write($"{lineNumber++} ");
                line = Console.ReadLine();
                if (line == ".")
                    break;
                lines.AppendLine(line);
            }
            var tree = parser.Parse(lines.ToString());

            Console.WriteLine($"Parsing done, took {tree.ParseTimeMilliseconds} ms");
            PrintParseTree(tree, lines.ToString().Split(parser.NewLine));
            Console.WriteLine();
        }

        private static void PrintParseTree(ParseTree tree, string[] lines)
        {
            if (tree.HasErrors())
                PrintError(tree, lines);
            else
                PrintNode(tree.Root, 0);
        }

        private static void PrintError(ParseTree tree, string[] lines)
        {
            const string lineString = "in line {0}: ";
            var errors = tree.ParserMessages.Where(e => e.Level == Irony.ErrorLevel.Error).ToList();

            if (errors.Count > 1)
                Console.WriteLine($"Programm has {errors.Count} errors.");
            else
                Console.WriteLine($"Programm has 1 error.");

            foreach (var e in errors)
            {
                string formattedLine = string.Format(lineString, e.Location.Line + 1);
                var startOfMarker = e.Location.Column + formattedLine.Length;
                var markerLength = Math.Max(1, 1);

                var marker = string.Concat(
                    new string(' ', startOfMarker),
                    new string('^', markerLength));
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.Write(formattedLine);
                if (e.Location.Line < lines.Length)
                    Console.WriteLine(lines[e.Location.Line]);
                else
                    Console.WriteLine("???");
                Console.WriteLine(marker);
            }
        }

        private static void PrintNode(ParseTreeNode currentNode, int currentIndentation)
        {
            Console.Write(new string('|', currentIndentation));
            Console.Write('-');

            string designator = currentNode.Term.Name;

            Console.WriteLine(designator);
            if (currentNode.ChildNodes != null)
            {
                foreach (var c in currentNode.ChildNodes)
                    PrintNode(c, currentIndentation + 1);
            }
        }
    }
}
