using System;

namespace Sasm
{
    using System.Collections.Generic;
    using System.Linq;
    using Sasm.Parsing;
    using Sasm.Tokenizing;

    class Program
    {
        static void Main(string[] args)
        {
            var tokenizer = new Tokenizer();
            var parser = new Parser();

            while(true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                var tokens = tokenizer.Tokenize(line);
                var tree = parser.ParseTokenList(tokens);

                PrintParseTree(tree);
                Console.WriteLine();
            }
        }

        private static void PrintParseTree(Parsing.ParseTree.ParseTree tree)
        {
            var currentNode = tree.root;
            int currentIndentation = 0;

            PrintNode(currentNode, currentIndentation);
        }

        private static void PrintNode(Parsing.ParseTree.ParseTreeNode currentNode, int currentIndentation)
        {
            Console.Write(new string(' ', currentIndentation));
            Console.WriteLine(currentNode);
            if(currentNode.Children != null)
            {
                foreach(var c in currentNode.Children)
                    PrintNode(c, currentIndentation + 2);
            }
        }
    }
}
