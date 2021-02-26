namespace Sasm.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Sasm.Parsing.Tokenizing;

    public class Parser
    {
        public ParseTree Parse(string input)
        {
            return Parse(new[] { input });
        }

        public ParseTree Parse(IReadOnlyList<string> lines)
        {
            return Parse(lines, "<input>");
        }

        public ParseTree Parse(IReadOnlyList<string> lines, string filename)
        {
            var tokens = new Tokenizer().Tokenize(lines);
            var context = new ParseContext(lines, filename, tokens);

            context.ParseWatch.Start();
            ParseStart(context);
            context.ParseWatch.Stop();

            return new ParseTree(context);
        }

        private void ParseStart(ParseContext context)
        {
            var startNode = new ParseTreeNode(new Token(TokenType.Start, 0, "", 0));

            while (!IsToken(context, TokenType.EndOfFile))
            {
                context.State = ParserState.Parsing;
                if (!TryParseLine(context))
                {
                    if (!IsRecovering(context))
                        CreateErrorAndRecover("line", context);
                    context.MoveNextToken();
                }
                else
                    startNode.AddChild(context.CurrentNode);
            }

            if (!IsToken(context, TokenType.EndOfFile))
            {
                throw new Exception("Did not parse the entire program! Found another token: " + context.CurrentToken);
            }

            context.State = ParserState.Done;
            context.CurrentNode = startNode;
        }

        private bool TryParseLine(ParseContext context)
        {
            if (!TryParseConstExpr(context))
            {
                if (!IsRecovering(context))
                    CreateErrorAndRecover("constant expression", context);
                return false;

            }

            if (!IsToken(context, TokenType.EndOfLine))
            {
                // recoveres to eol
                CreateErrorAndRecover("end of line", context);
                return false;
            }

            context.MoveNextToken();

            return true;
        }

        private bool TryParseConstExpr(ParseContext context)
        {
            if (!TryParseAddTerm(context))
                return false;

            while (IsToken(context, TokenType.AddOp))
            {
                var addNode = new ParseTreeNode(context.CurrentToken);
                addNode.AddChild(context.CurrentNode);

                context.MoveNextToken();

                if (!TryParseAddTerm(context))
                {
                    if (!IsRecovering(context))
                        CreateErrorAndRecover("addition term", context);
                    return false;
                }
                else
                {
                    addNode.AddChild(context.CurrentNode);
                    context.CurrentNode = addNode;
                }
            }

            return true;
        }

        private bool TryParseAddTerm(ParseContext context)
        {
            if (!TryParseConstant(context))
                return false;

            while (IsToken(context, TokenType.MulOp))
            {
                var mulNode = new ParseTreeNode(context.CurrentToken);
                mulNode.AddChild(context.CurrentNode);

                context.MoveNextToken();

                if (!TryParseConstant(context))
                {
                    if (!IsRecovering(context))
                        CreateErrorAndRecover("constant", context);
                    return false;
                }
                else
                    mulNode.AddChild(context.CurrentNode);
                context.CurrentNode = mulNode;
            }

            return true;
        }

        private bool TryParseConstant(ParseContext context)
        {
            if (context.HasCurrentToken)
            {
                switch (context.CurrentToken.TokenType)
                {
                    case TokenType.BinNumber:
                    case TokenType.OctNumber:
                    case TokenType.DecNumber:
                    case TokenType.HexNumber:
                    case TokenType.Identifier:
                    case TokenType.Char:
                        {
                            context.CurrentNode = new ParseTreeNode(context.CurrentToken);
                            context.MoveNextToken();
                            return true;
                        }
                }

                if (IsToken(context, TokenType.LParen))
                {
                    context.MoveNextToken();
                    if (TryParseConstExpr(context))
                    {
                        if (!IsToken(context, TokenType.RParen))
                        {
                            CreateErrorAndRecover("')'", context);
                            return false;
                        }
                        else
                        {
                            context.MoveNextToken();
                            return true;
                        }
                    }
                    else
                    {
                        if (!IsRecovering(context))
                            CreateErrorAndRecover("constant expression", context);
                        return false;
                    }
                }
            }
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

        private void CreateErrorAndRecover(
            string acceptedString,
            ParseContext context)
        {
            var currentToken = context.CurrentToken;
            var message = $"Expected {acceptedString} but found {currentToken.TokenType}";

            context.State = ParserState.Recovering;

            // move to after next eol
            while (context.MoveNextToken() && context.CurrentToken.TokenType != TokenType.EndOfLine)
            {
            }

            context.AddError(currentToken.Source, message);
        }

        private long ParseNumber(string text, int prefixSize, int _base)
        {
            text = text.Replace("_", "");
            text = text.Remove(0, prefixSize);
            return Convert.ToInt64(text, _base);
        }

        public bool IsRecovering(ParseContext context) => context.State == ParserState.Recovering;
        private bool IsToken(ParseContext context, TokenType expected)
            => context.CurrentToken.TokenType == expected;
        private bool IsToken(ParseContext context, params TokenType[] expected)
            => context.HasCurrentToken && expected.Contains(context.CurrentToken.TokenType);
    }
}