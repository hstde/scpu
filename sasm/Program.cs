using System;

namespace Sasm
{
    using System.Collections.Generic;
    using System.Linq;
    using Sasm.Tokenizing;

    class Program
    {
        static void Main(string[] args)
        {
            var tokens = TokenizeLine("label: lD h, (IX+127) ; this is a comment").ToArray();
            Console.WriteLine(string.Join(" ", tokens.Select(e => e.TokenType + ":" + e.Content)));
        }

        static IEnumerable<Token> TokenizeLine(string line)
        {
            return new Tokenizer().TokenizeLine(line);
        }
    }
}
