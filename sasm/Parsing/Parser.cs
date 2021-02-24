namespace Sasm.Parsing
{
    using System;
    using System.Collections.Generic;
    using Sasm.Parsing.ParseTree;
    using Sasm.Tokenizing;

    public class Parser
    {
        // takes an array of tokens and produces a ParseTree
        // grammar:

        // start = line*

        // line = LabelDef? instruction? Comment? Eol

        // instruction = operation
        // instruction = directive

        // operation = op
        // operation = op operand
        // operation = op operand Separator operand

        // op = Mnemonic
        // op = Identifier

        // operand = Register
        // operand = Char
        // operand = constant
        // operand = absolute
        // operand = indirect
        // operand = displacement
        // operand = offset

        // constant = Number
        // constant = Identifier
        // constant = constExpr -> ToDo

        // absolute = LParen constant RParen

        // indirect = LParen Register RParen
        // indirect = LParen Indentifier RParen

        // displacement = LParen Register operator constant RParen
        // displacement = LParen Identifier operator constant RParen
        // displacement = LParen constant operator Register RParen
        // displacement = LParen constant operator Identifier RParen

        // offset = LParen Register operator Register RParen
        // offset = LParen Register operator Identifier RParen
        // offset = LParen Identifier operator Register RParen
        // offset = LParen Identifier oparetor Identifier RParen

        // directive = 

        public ParseTreeNode ParseTokenList(IEnumerable<Token> tokens)
        {
            return null;
        }
    }
}