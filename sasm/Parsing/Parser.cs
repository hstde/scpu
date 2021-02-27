namespace Sasm.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
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
            var startNode = new ParseTreeNode(
                new Token(TokenType.Start, 0, "", 0),
                ParseTerm.Start);

            while (!IsToken(context, TokenType.EndOfFile))
            {
                context.State = ParserState.Parsing;
                if (!TryParseLine(context))
                {
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
            TryParseInstruction(context);

            if (!IsToken(context, TokenType.EndOfLine))
            {
                // recoveres to eol
                CreateErrorAndRecover("end of line", context);
                return false;
            }

            context.MoveNextToken();

            return true;
        }

        private bool TryParseInstruction(ParseContext context)
        {
            if (TryParseOperation(context))
                return true;

            return false;
        }

        private bool TryParseConstExpr(ParseContext context)
        {
            if (!TryParseAddTerm(context))
                return false;

            while (IsToken(context, TokenType.AddOp))
            {
                var addNode = new ParseTreeNode(context.CurrentToken, ParseTerm.AddTerm);
                addNode.AddChild(context.CurrentNode);

                context.MoveNextToken();

                if (!TryParseAddTerm(context))
                {
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
            if (!TryParseMulTerm(context))
                return false;

            while (IsToken(context, TokenType.MulOp))
            {
                var mulNode = new ParseTreeNode(context.CurrentToken, ParseTerm.MulTerm);
                mulNode.AddChild(context.CurrentNode);

                context.MoveNextToken();

                if (!TryParseMulTerm(context))
                {
                    CreateErrorAndRecover("constant", context);
                    return false;
                }
                else
                    mulNode.AddChild(context.CurrentNode);
                context.CurrentNode = mulNode;
            }

            return true;
        }

        private bool TryParseMulTerm(ParseContext context)
        {
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
                    CreateErrorAndRecover("constant expression", context);
                    return false;
                }
            }
            else if (TryParseLiteral(context))
            {
                return true;
            }

            return false;
        }

        private bool TryParseOperation(ParseContext context)
        {
            if (TryParseOp(context))
            {
                var op = context.CurrentNode;
                if (!TryParseOperand(context))
                    return true;

                op.AddChild(context.CurrentNode);
                context.CurrentNode = op;

                if (!IsToken(context, TokenType.Separator))
                    return true;

                context.MoveNextToken();
                if (TryParseOperand(context))
                {
                    op.AddChild(context.CurrentNode);
                    context.CurrentNode = op;
                    return true;
                }
                else
                {
                    CreateErrorAndRecover("operand", context);
                    return false;
                }
            }

            return false;
        }

        private bool TryParseOp(ParseContext context)
        {
            switch (context.CurrentToken.TokenType)
            {
                case TokenType.Mnemonic:
                case TokenType.Identifier:
                    context.ConsumeToken(ParseTerm.Op);
                    return true;
                default: return false;
            }
        }

        private bool TryParseOperand(ParseContext context)
        {
            context.StoreTokenPosition();
            if (TryParseConstant(context))
            {
                context.DropStoredTokenPosition();
                return true;
            }
            context.RestoreTokenPosition();
            context.StoreTokenPosition();
            if (TryParseRegisterAlias(context))
            {
                context.DropStoredTokenPosition();
                return true;
            }
            context.RestoreTokenPosition();
            context.StoreTokenPosition();
            if (TryParseAbsolute(context))
            {
                context.DropStoredTokenPosition();
                return true;
            }
            context.RestoreTokenPosition();
            context.StoreTokenPosition();
            if (TryParseIndirect(context))
            {
                context.DropStoredTokenPosition();
                return true;
            }
            context.RestoreTokenPosition();
            context.StoreTokenPosition();
            if (TryParseDisplacement(context))
            {
                context.DropStoredTokenPosition();
                return true;
            }
            context.RestoreTokenPosition();
            context.StoreTokenPosition();
            if (TryParseOffset(context))
            {
                context.DropStoredTokenPosition();
                return true;
            }
            context.DropStoredTokenPosition();
            return false;
        }

        private bool TryParseConstant(ParseContext context)
        {
            if (TryParseConstExpr(context))
            {
                return true;
            }
            else if (TryParseLiteral(context))
            {
                return true;
            }

            return false;
        }

        private bool TryParseLiteral(ParseContext context)
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
                        context.ConsumeToken(ParseTerm.Literal);
                        return true;
                    }
                default: return false;
            }
        }

        private bool TryParseAbsolute(ParseContext context)
        {
            if (IsToken(context, TokenType.LBracket))
            {
                context.MoveNextToken();
                if (TryParseConstant(context))
                {
                    var absolute = new ParseTreeNode(context.CurrentToken, ParseTerm.Offset);
                    absolute.AddChild(context.CurrentNode);
                    if (!IsToken(context, TokenType.RBracket))
                    {
                        CreateErrorAndRecover("']'", context);
                        return false;
                    }
                    else
                    {
                        context.CurrentNode = absolute;
                        context.MoveNextToken();
                        return true;
                    }
                }
                else
                {
                    CreateErrorAndRecover("constant", context);
                    return false;
                }
            }
            return false;
        }

        private bool TryParseDisplacement(ParseContext context)
        {
            if (IsToken(context, TokenType.LBracket))
            {
                context.MoveNextToken();
                if (TryParseRegisterAlias(context))
                {
                    if (IsToken(context, TokenType.AddOp))
                    {
                        var displacement = new ParseTreeNode(context.CurrentToken, ParseTerm.Displacement);
                        displacement.AddChild(context.CurrentNode);
                        context.MoveNextToken();
                        if (TryParseConstant(context))
                        {
                            if (!IsToken(context, TokenType.RBracket))
                            {
                                CreateErrorAndRecover("']'", context);
                                return false;
                            }
                            else
                            {
                                displacement.AddChild(context.CurrentNode);
                                context.CurrentNode = displacement;
                                context.MoveNextToken();
                                return true;
                            }
                        }
                        else
                        {
                            CreateErrorAndRecover("constant", context);
                            return false;
                        }
                    }
                    else
                    {
                        CreateErrorAndRecover("'+' or '-'", context);
                        return false;
                    }
                }
                else if (TryParseConstant(context))
                {
                    if (IsToken(context, TokenType.AddOp))
                    {
                        var displacement = new ParseTreeNode(context.CurrentToken, ParseTerm.Displacement);
                        displacement.AddChild(context.CurrentNode);

                        if (TryParseRegisterAlias(context))
                        {
                            if (!IsToken(context, TokenType.RBracket))
                            {
                                CreateErrorAndRecover("']'", context);
                                return false;
                            }
                            else
                            {
                                displacement.AddChild(context.CurrentNode);
                                context.CurrentNode = displacement;
                                context.MoveNextToken();
                                return true;
                            }
                        }
                        else
                        {
                            CreateErrorAndRecover("register", context);
                            return false;
                        }
                    }
                    else
                    {
                        CreateErrorAndRecover("'+' or '-'", context);
                        return false;
                    }
                }
                else
                {
                    CreateErrorAndRecover("register or constant", context);
                    return false;
                }
            }
            return false;
        }

        private bool TryParseOffset(ParseContext context)
        {
            if (IsToken(context, TokenType.LBracket))
            {
                context.MoveNextToken();
                if (TryParseRegisterAlias(context))
                {
                    if (IsToken(context, TokenType.AddOp))
                    {
                        var offset = new ParseTreeNode(context.CurrentToken, ParseTerm.Offset);
                        offset.AddChild(context.CurrentNode);
                        context.MoveNextToken();
                        if (TryParseRegisterAlias(context))
                        {
                            if (!IsToken(context, TokenType.RBracket))
                            {
                                CreateErrorAndRecover("']'", context);
                                return false;
                            }
                            else
                            {
                                offset.AddChild(context.CurrentNode);
                                context.CurrentNode = offset;
                                context.MoveNextToken();
                                return true;
                            }
                        }
                        else
                        {
                            CreateErrorAndRecover("constant", context);
                            return false;
                        }
                    }
                    else
                    {
                        CreateErrorAndRecover("'+' or '-'", context);
                        return false;
                    }
                }
                else
                {
                    CreateErrorAndRecover("register", context);
                    return false;
                }
            }
            return false;
        }

        private bool TryParseIndirect(ParseContext context)
        {
            if (IsToken(context, TokenType.LBracket))
            {
                context.MoveNextToken();
                if (TryParseRegisterAlias(context))
                {
                    var indirect = new ParseTreeNode(context.CurrentToken, ParseTerm.Offset);
                    indirect.AddChild(context.CurrentNode);
                    if (!IsToken(context, TokenType.RBracket))
                    {
                        CreateErrorAndRecover("']'", context);
                        return false;
                    }
                    else
                    {
                        context.CurrentNode = indirect;
                        context.MoveNextToken();
                        return true;
                    }
                }
                else
                {
                    CreateErrorAndRecover("register", context);
                    return false;
                }
            }
            return false;
        }

        private bool TryParseRegisterAlias(ParseContext context)
        {
            if (IsToken(context, TokenType.Register) 
                || IsToken(context, TokenType.Identifier))
            {
                context.ConsumeToken(ParseTerm.RegisterAlias);
                return true;
            }

            return false;
        }

        private void CreateErrorAndRecover(
            string acceptedString,
            ParseContext context)
        {
            if (context.State is ParserState.Recovering || context.State is ParserState.Trying)
                return;

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

        private bool IsRecovering(ParseContext context) => context.State == ParserState.Recovering;
        private bool IsToken(ParseContext context, TokenType expected)
            => context.CurrentToken.TokenType == expected;
        private bool IsToken(ParseContext context, params TokenType[] expected)
            => expected.Contains(context.CurrentToken.TokenType);
    }
}