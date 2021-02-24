namespace Sasm.Tokenizing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Sasm.Tokenizing.TokenType;

    public class Tokenizer
    {
        private static string[] mnemonics = new string[]
        {
            "nop",
            "ld", "mov",
            "and", "or", "xor", "not", "shl", "scl", "shr", "scr",
            "neg", "add", "adc", "sub", "sbc", "inc", "dec",
            "cmp", "test",
            "push", "pop",
            "scf", "ssf", "szf", "sif",
            "ccf", "csf", "czf", "cif",
            "j", "jz", "je", "jnz", "jne", "jc", "jnc", "js", "jns", "jl", "ret",
            "irq", "reti",
            "lea"

        };
        private static string[] registers = new string[]
        {
            "a", "b", "c", "d", "e", "h", "l",
            "bc", "de", "hl", "ix", "iy",
            "sp", "lr", "iv"
        };
        private static char[] operators = new char[]
        {
            '+', '-', '*', '/'
        };
        private static string[] commands = new string[]
        {
            ".define", ".include", ".section", ".org",
            "db", "dw", ".times",
            ".warning"

        };
        private const int MinCharDefinitionLength = 2;
        private const int NormalCharDefinitionLength = 3;
        private const int EscapedCharDefintionLength = 4;
        private const int NormalCharClosingDelimiterOffset = 2;
        private const int EscapedCharClosingDelimiterOffset = 3;

        private const char LParen = '(';
        private const char RParen = ')';
        private const char LabelTerminator = ':';
        private const char CommentStart = ';';
        private const char StringDeliminator = '"';
        private const char CharDeliminator = '\'';
        private const char StringEscapeCharacter = '\\';
        private const char SpecialBasePrefix = '0';
        private const char BinNumberMarker = 'b';
        private const char HexNumberMarker = 'x';
        private const char NumberFormatHelper = '_';
        private const char CommandPrefix = '.';
        private const char ArgumentSeparator = ',';
        private const char CurrentLineAddressMarker = '$';

        public IReadOnlyList<Token> Tokenize(string contents)
        {
            var tokens = new List<Token>();

            var lines = contents.Split('\n');

            var lineNumber = 0;
            foreach (var line in lines)
                TokenizeLine(lineNumber++, line, ref tokens);

            return tokens;
        }

        private void TokenizeLine(int lineNumber, string line, ref List<Token> tokens)
        {
            int position = 0;

            while (position < line.Length)
            {
                ConsumeWhiteSpace(line, ref position);

                if (position >= line.Length)
                    break;

                if (IsComment(lineNumber, line, ref position, out var token))
                    tokens.Add(token.Value);

                else if (IsNumber(lineNumber, line, ref position, out token))
                    tokens.Add(token.Value);

                else if (IsNameOrCommandOrMnemonicOrReg(lineNumber, line, ref position, out token))
                    tokens.Add(token.Value);

                else if (IsSimpleToken(lineNumber, line, ref position, out token))
                    tokens.Add(token.Value);

                else if (IsCharDefinition(lineNumber, line, ref position, out token))
                    tokens.Add(token.Value);

                else if (IsStringDefinition(lineNumber, line, ref position, out token))
                    tokens.Add(token.Value);

                else
                    tokens.Add(ConsumeUnknown(lineNumber, line, ref position));
            }

            // we have reached the end of the line here
            tokens.Add(new Token(EndOfLine, lineNumber, line, position, 0));
        }

        private bool IsCharDefinition(int lineNumber, string line, ref int position, out Token? token)
        {
            token = null;
            if (!(line[position] is CharDeliminator))
                return false;
            if (line.Length < position + MinCharDefinitionLength)
                return false;

            bool hasEscapeCode = line[position + 1] is StringEscapeCharacter;
            int expectedLength = hasEscapeCode ? EscapedCharDefintionLength : NormalCharDefinitionLength;
            int expectedDelimitorOffset = hasEscapeCode ? EscapedCharClosingDelimiterOffset : NormalCharClosingDelimiterOffset;

            if (line.Length < position + expectedLength)
                return false;
            if (!(line[position + expectedDelimitorOffset] is CharDeliminator))
                return false;

            int start = position + 1;

            position += expectedLength;

            token = new Token(TokenType.Char, lineNumber, line, start, expectedLength - 2);
            return true;
        }

        private bool IsStringDefinition(int lineNumber, string line, ref int position, out Token? token)
        {
            token = null;

            if (!(line[position] is StringDeliminator))
                return false;
            if (line.Length < position + 2)
                return false;

            int start = position;

            bool isClosed = false;
            position++;
            while (position < line.Length && !(line[position] is StringDeliminator))
            {
                if (position < line.Length && line[position] is StringEscapeCharacter)
                    position++;
                position++;
            }

            if (position < line.Length && line[position] is StringDeliminator)
                isClosed = true;

            position++;

            if (isClosed)
            {
                token = new Token(TokenType.String, lineNumber, line, start + 1, position - start - 2);
                return true;
            }
            else
            {
                position = start;
                return false;
            }
        }

        private bool IsSimpleToken(int lineNumber, string line, ref int position, out Token? token)
        {
            int start = position;
            token = null;

            switch (line[position])
            {
                case LParen:
                    position++;
                    token = new Token(TokenType.LParen, lineNumber, line, start, 1);
                    break;
                case RParen:
                    position++;
                    token = new Token(TokenType.RParen, lineNumber, line, start, 1);
                    break;
                case ArgumentSeparator:
                    position++;
                    token = new Token(Separator, lineNumber, line, start, 1);
                    break;
                case char c when operators.Contains(c):
                    position++;
                    token = new Token(TokenType.Operator, lineNumber, line, start, 1);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private bool IsComment(int lineNumber, string line, ref int position, out Token? token)
        {
            token = null;
            if (!IsCommentStart(line[position]))
                return false;

            int start = position;
            position = line.Length;

            token = new Token(Comment, lineNumber, line, start, position - start);
            return true;
        }

        private Token ConsumeUnknown(int lineNumber, string line, ref int position)
        {
            position++;
            return new Token(Unknown, lineNumber, line, position - 1, 1);
        }

        private bool IsNameOrCommandOrMnemonicOrReg(int lineNumber, string line, ref int position, out Token? token)
        {
            int start = position;
            token = null;

            if (!IsIdentStart(line[position]))
                return false;
            // first character was confirmed to be a name character, so either IsAlpha
            // or '.' and it is confirmed to not be a command
            bool isPossibleCommand = line[position] is CommandPrefix;
            position++;
            while (position < line.Length
                    && (IsIdent(line[position])))
            {
                position++;
            }
            int length = position - start;

            var type = Identifier;

            string tokenText = line.Substring(start, length);
            string tokenTextLower = tokenText.ToLower();

            if (commands.Contains(tokenTextLower))
                type = Command;
            else if (mnemonics.Contains(tokenTextLower))
                type = Mnemonic;
            else if (registers.Contains(tokenTextLower))
                type = Register;
            else if (position < line.Length && line[position] is LabelTerminator)
            {
                position++;
                type = LabelDefinition;
            }

            token = new Token(type, lineNumber, tokenText, start);
            return true;
        }

        private void ConsumeWhiteSpace(string line, ref int position)
        {
            while (position < line.Length && IsWhitespace(line[position]))
                position++;
        }

        private bool IsNumber(int lineNumber, string line, ref int position, out Token? token)
        {
            token = null;
            if (!IsNumeric(line[position]))
                return false;

            int start = position;
            var type = DecNumber;

            if (line[position] is SpecialBasePrefix)
            {
                position++;

                if (position < line.Length)
                {
                    switch (line[position])
                    {
                        case BinNumberMarker:
                            type = BinNumber;
                            position++;
                            break;
                        case HexNumberMarker:
                            type = HexNumber;
                            position++;
                            break;
                        default:
                            type = OctNumber;
                            break;
                    }
                }
            }

            while (position < line.Length
                && (IsNumberType(line[position], type)
                    || line[position] is NumberFormatHelper))
                position++;

            token = new Token(type, lineNumber, line, start, position - start);
            return true;
        }

        private bool IsCommentStart(char c) => c is CommentStart;
        private bool IsWhitespace(char c) => c is ' ' || c is '\t';
        private bool IsIdentStart(char c) => IsAlpha(c) || c is CommandPrefix || c is NumberFormatHelper || c is CurrentLineAddressMarker;
        private bool IsNumeric(char c) => IsDecNumber(c);
        private bool IsBinNumber(char c) => c is '0' || c is '1';
        private bool IsOctNumber(char c) => (c >= '0' && c <= '7');
        private bool IsDecNumber(char c) => (c >= '0' && c <= '9');
        private bool IsHexNumber(char c) => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        private bool IsAlphaNum(char c) => IsAlpha(c) || IsNumeric(c);
        private bool IsIdent(char c) => IsIdentStart(c) || IsNumeric(c);

        private bool IsNumberType(char c, TokenType numberType)
        {
            return numberType switch
            {
                BinNumber => IsBinNumber(c),
                OctNumber => IsOctNumber(c),
                DecNumber => IsDecNumber(c),
                HexNumber => IsHexNumber(c),
                _ => throw new ArgumentException($"Unexpected tokentype {numberType}")
            };
        }
    }
}