namespace Sasm.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Transactions;
    using Sasm.Parsing.ParseTree;
    using Sasm.Tokenizing;

    public class Parser
    {
        // takes an array of tokens and produces a ParseTree
        public ParseTree.ParseTree ParseTokenList(IEnumerable<Token> tokens)
        {
            var tokenArray = tokens.ToArray();
            int position = 0;

            var root = ParseStart(tokenArray, ref position);

            var tree = new ParseTree.ParseTree(root);
            return tree;
        }

        private ParseTreeNode ParseStart(Token[] tokens, ref int position)
        {
            var lines = new List<ParseTreeNode>();

            while (!IsToken(tokens, position, TokenType.EndOfFile))
            {
                if (!TryParseLine(tokens, ref position, out var line))
                {
                    line = CreateErrorAndRecover("line", tokens, ref position);
                    position++;
                }
                lines.Add(line);
            }

            if (!IsToken(tokens, position, TokenType.EndOfFile))
            {
                throw new Exception("Did not parse the entire program! Found another token: " + tokens[position]);
            }

            var root = new StartNode(new SourceReference(0, 0, 0), lines);
            return root;
        }

        private bool TryParseLine(Token[] tokens, ref int position, out ParseTreeNode node)
        {
            if (!TryParseConstExpr(tokens, ref position, out node))
            {
                node = CreateErrorAndRecover("constant expression", tokens, ref position);
            }

            if (!IsToken(tokens, position, TokenType.EndOfLine))
            {
                // recoveres to eol
                node = CreateErrorAndRecover("end of line", tokens, ref position);
            }

            position++;

            return true;
        }

        private bool TryParseConstExpr(Token[] tokens, ref int position, out ParseTreeNode node)
        {
            var hasTerm1 = TryParseAddTerm(tokens, ref position, out node);

            if (!hasTerm1)
                return false;

            while (IsToken(tokens, position, TokenType.AddOp))
            {
                if (!TryParseOpToken(tokens[position], out var op))
                    node = CreateErrorAndRecover("'+' or '-'", tokens, ref position);
                else
                {
                    position++;
                    if (!TryParseAddTerm(tokens, ref position, out var term2))
                        node = CreateErrorAndRecover("addition term", tokens, ref position);
                    else
                        node = new BinaryOperation(
                            op,
                            tokens[position].Source,
                            node,
                            term2
                        );
                }
            }
            return true;
        }

        private bool TryParseOpToken(Token opToken, out BinaryOperation.OperationType op)
        {
            switch (opToken.Content)
            {
                case "+":
                    op = BinaryOperation.OperationType.Add;
                    return true;
                case "-":
                    op = BinaryOperation.OperationType.Sub;
                    return true;
                case "*":
                    op = BinaryOperation.OperationType.Mul;
                    return true;
                case "/":
                    op = BinaryOperation.OperationType.Div;
                    return true;
                default:
                    op = 0;
                    return false;
            }
        }

        private bool TryParseAddTerm(Token[] tokens, ref int position, out ParseTreeNode node)
        {
            var hasTerm1 = TryParseConstant(tokens, ref position, out node);

            if (!hasTerm1)
                return false;

            while (IsToken(tokens, position, TokenType.MulOp))
            {
                if (!TryParseOpToken(tokens[position], out var op))
                    node = CreateErrorAndRecover("'*' or '/'", tokens, ref position);
                else
                {
                    position++;
                    if (!TryParseConstant(tokens, ref position, out var term2))
                        node = CreateErrorAndRecover("constant", tokens, ref position);
                    else
                        node = new BinaryOperation(
                            op,
                            tokens[position].Source,
                            node,
                            term2
                        );
                }
            }

            return true;
        }

        private bool TryParseConstant(Token[] tokens, ref int position, out ParseTreeNode node)
        {
            if (HasMoreTokens(tokens, position))
            {
                var currentToken = tokens[position];
                switch (currentToken.TokenType)
                {
                    case TokenType.BinNumber:
                        {
                            var value = ParseNumber(currentToken.Content, 2, 2);
                            node = new Number(currentToken.Source, value);
                            position++;
                            return true;
                        }
                    case TokenType.OctNumber:
                        {
                            var value = ParseNumber(currentToken.Content, 1, 8);
                            node = new Number(currentToken.Source, value);
                            position++;
                            return true;
                        }
                    case TokenType.DecNumber:
                        {
                            var value = ParseNumber(currentToken.Content, 0, 10);
                            node = new Number(currentToken.Source, value);
                            position++;
                            return true;
                        }
                    case TokenType.HexNumber:
                        {
                            var value = ParseNumber(currentToken.Content, 2, 16);
                            node = new Number(currentToken.Source, value);
                            position++;
                            return true;
                        }
                    case TokenType.Identifier:
                        {
                            node = new NamedReference(currentToken.Content, currentToken.Source);
                            position++;
                            return true;
                        }
                    case TokenType.Char:
                        {
                            var value = ParseCharacter(currentToken.Content);
                            node = new Number(currentToken.Source, value);
                            position++;
                            return true;
                        }
                }

                if (IsToken(tokens, position, TokenType.LParen))
                {
                    position++;
                    if (TryParseConstExpr(tokens, ref position, out node))
                    {
                        if (ErrorHelper.IsError(node))
                            return true;
                        if (!IsToken(tokens, position, TokenType.RParen))
                            node = CreateErrorAndRecover("')'", tokens, ref position);
                        else
                            position++;
                    }
                    else
                        node = CreateErrorAndRecover("constant expression", tokens, ref position);
                    return true;
                }
            }
            node = null;
            return false;
        }

        private char ParseCharacter(string content)
        {
            if (content.Length == 1)
            {
                return content[0];
            }
            else if (content.Length == 2)
            {
                // some escapism
                return '\0';
            }
            else
            {
                throw new Exception("Unexpected character string of length " + content.Length);
            }
        }

        private ParseTreeNode CreateErrorAndRecover(
            string acceptedString,
            Token[] tokens,
            ref int position)
        {
            var currentToken = tokens[position];
            var message = $"Expected {acceptedString} but found {tokens[position].TokenType}";

            // move to after next eol
            while (position < tokens.Length && tokens[position].TokenType != TokenType.EndOfLine)
            {
                position++;
            }

            return new ErrorNode(message, currentToken.Source);
        }

        private long ParseNumber(string text, int prefixSize, int _base)
        {
            text = text.Replace("_", "");
            text = text.Remove(0, prefixSize);
            return Convert.ToInt64(text, _base);
        }

        private bool IsToken(Token[] tokens, int position, TokenType expected)
            => HasMoreTokens(tokens, position) && tokens[position].TokenType == expected;
        private bool IsToken(Token[] tokens, int position, params TokenType[] expected)
            => HasMoreTokens(tokens, position) && expected.Contains(tokens[position].TokenType);
        private bool HasMoreTokens(Token[] tokens, int position, int count = 1)
            => position + count <= tokens.Length;
    }
}