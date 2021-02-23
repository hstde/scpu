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
            var lines = new string[]
            {
                ".include \".\\stuff.inc\"",
                "main:",
                "   ld a, 1",
                "   ld b, 2",
                "   ld hl, data",
                "   add a, b",
                "   ld (hl), a",
                "loop: j loop; just idle for ever",
                "data: db 0"
            };

            var tokenizer = new Tokenizer();

            foreach (var line in lines)
            {
                var tokens = tokenizer.TokenizeLine(line);
                Console.WriteLine(string.Join(" ", tokens.Select(e => e.TokenType + ":" + e.Content)));
            }

            while(true)
            {
                Console.Write(">");
                var input = Console.ReadLine();
                var tokens = tokenizer.TokenizeLine(input);
                foreach(var token in tokens)
                    Console.WriteLine(token);
            }
        }
    }
}
