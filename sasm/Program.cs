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
            var lines =
                "LD SP, 0xFFFF\n"
                + "JL MAIN\n"
                + "J $\n"
                + "\n"
                + "MSG: DB \"HELLO WORLD\", 0\n"
                + "SCREENBUFFER: DW 0x4000\n"
                + "SCREENOFFSET: DW 0\n"
                + "\n"
                + "MAIN:\n"
                + "    PUSH LR\n"
                + "    LD HL, MSG\n"
                + "    PUSH HL\n"
                + "    JL PRINT\n"
                + "    POP LR\n"
                + "    RET\n"
                + "\n"
                + "PRINT:\n"
                + "    LD IY, (SCREENBUFFER)\n"
                + "    LD HL, (SCREENOFFSET)\n"
                + "\n"
                + "    POP BC\n"
                + "\n"
                + "    LD B, 0x07\n"
                + "    .LOOP:\n"
                + "        LD A, (BC)\n"
                + "        INC BC\n"
                + "        TEST 0\n"
                + "            JZ .ENDLOOP ; while *ix != 0\n"
                + "        LEA IX, (IY + HL)\n"
                + "        LD (IX), A\n"
                + "        LD (IX + 1), B\n"
                + "        ADD HL, 2\n"
                + "        J .LOOP\n"
                + "    .ENDLOOP:\n"
                + "\n"
                + "    LD (SCREENOFFSET), HL\n"
                + "    RET";

            var tokenizer = new Tokenizer();

            var tokens = tokenizer.Tokenize(lines);
            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case TokenType.EndOfLine:
                        Console.WriteLine();
                        break;
                    case TokenType.Identifier:
                    case TokenType.LabelDefinition:
                    case TokenType.Char:
                    case TokenType.String:
                        Console.Write(token.TokenType + ":" + token.Content + " ");
                        break;

                    default:
                        Console.Write(token.Content + " ");
                        break;
                }
            }
        }
    }
}
