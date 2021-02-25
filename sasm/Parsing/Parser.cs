namespace Sasm.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            var root = ParseConstExpr(tokenArray, ref position);
            var tree = new ParseTree.ParseTree(root);
            return tree;
        }

        private ParseTreeNode ParseConstExpr(Token[] tokens, ref int position)
        {
            return ParseAddTerm(tokens, ref position);
        }

        private ParseTreeNode ParseAddTerm(Token[] tokens, ref int position)
        {
            return ParseMulTerm(tokens, ref position);
        }

        private ParseTreeNode ParseMulTerm(Token[] tokens, ref int position)
        {
            return ParseConstant(tokens, ref position);
        }

        private ParseTreeNode ParseConstant(Token[] tokens, ref int position)
        {
            var currentToken = tokens[position];
            position++;
            switch (currentToken.TokenType)
            {
                case TokenType.BinNumber:
                    {
                        var value = ParseNumber(currentToken.Content, 2, 2);
                        return new Number(currentToken.Source, value);
                    }
                case TokenType.OctNumber:
                    {
                        var value = ParseNumber(currentToken.Content, 1, 8);
                        return new Number(currentToken.Source, value);
                    }
                case TokenType.DecNumber:
                    {
                        var value = ParseNumber(currentToken.Content, 0, 10);
                        return new Number(currentToken.Source, value);
                    }
                case TokenType.HexNumber:
                    {
                        var value = ParseNumber(currentToken.Content, 2, 16);
                        return new Number(currentToken.Source, value);
                    }
            }

            // we have to track back
            // ??
            position--;
            return CreateErrorNodeUnexpectedToken(currentToken);
        }

        private ParseTreeNode CreateErrorNodeUnexpectedToken(Token currentToken)
        {
            var message = $"Encountered unexpected token \"{currentToken.TokenType}\"";
            return new ErrorNode(currentToken.Source, message);
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