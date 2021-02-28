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
            var startNode = new ParseTreeNode(new Token(TokenType.Start, 0, "", 0));
            context.CurrentNode = startNode;

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
            var line = new ParseTreeNode(new Token(TokenType.Line, context.CurrentToken.Source.lineNumber, "", 0));

            if (IsToken(context, TokenType.LabelDefinition))
            {
                line.AddChild(context.ConsumeToken());
            }

            if (TryParseInstruction(context))
            {
                line.AddChild(context.CurrentNode);
            }

            if (IsToken(context, TokenType.Comment))
            {
                line.AddChild(context.ConsumeToken());
            }

            if (!IsToken(context, TokenType.EndOfLine))
            {
                // recoveres to eol
                CreateErrorAndRecover("end of line", context);
                return false;
            }

            context.CurrentNode = line;

            context.MoveNextToken();

            return true;
        }

        private bool TryParseInstruction(ParseContext context)
        {
            if (TryParseOperation(context))
                return true;
            if (TryParseDirective(context))
                return true;

            return false;
        }

        private bool TryParseDirective(ParseContext context)
        {
            if (TryParseSegmentDirective(context))
            {
                return true;
            }
            if (TryParseOriginDirective(context))
            {
                return true;
            }
            if (TryParseIncludeDirective(context))
            {
                return true;
            }
            if (TryParseTimesDirective(context))
            {
                return true;
            }
            if (TryParseDataDirective(context))
            {
                return true;
            }
            if (TryParseWarningDirective(context))
            {
                return true;
            }
            if (TryParseConstDirective(context))
            {
                return true;
            }

            return false;
        }

        private bool TryParseConstDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.ConstantDeclaration))
                return false;

            var directive = context.ConsumeToken();

            if (!IsToken(context, TokenType.Identifier))
            {
                CreateErrorAndRecover("identifier after constant declaration", context);
                return false;
            }

            directive.AddChild(context.ConsumeToken());

            if (!TryParseDataConstant(context))
            {
                CreateErrorAndRecover("data constant after constant declaration", context);
                return false;
            }

            directive.AddChild(context.CurrentNode);
            context.CurrentNode = directive;

            return true;
        }

        private bool TryParseDataConstant(ParseContext context)
        {
            switch (context.CurrentToken.TokenType)
            {
                case TokenType.String:
                case TokenType.EscapedString:
                    context.ConsumeToken();
                    return true;
            }

            if (TryParseConstant(context))
                return true;

            return false;
        }

        private bool TryParseWarningDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.WarningCommand))
                return false;

            var directive = context.ConsumeToken();
            if (!TryParseDataConstant(context))
            {
                CreateErrorAndRecover("dataconstant in warning directive", context);
                return false;
            }

            directive.AddChild(context.CurrentNode);

            while (IsToken(context, TokenType.Separator))
            {
                directive.AddChild(context.ConsumeToken());
                if (!TryParseDataConstant(context))
                {
                    CreateErrorAndRecover("dataconstant after separator in warning directive", context);
                    return false;
                }

                directive.AddChild(context.CurrentNode);
            }

            context.CurrentNode = directive;
            return true;
        }

        private bool TryParseDataDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.DataDefinition))
                return false;

            var directive = context.ConsumeToken();
            if (!TryParseDataConstant(context))
            {
                CreateErrorAndRecover("data constant in data directive", context);
                return false;
            }

            directive.AddChild(context.CurrentNode);

            while (IsToken(context, TokenType.Separator))
            {
                directive.AddChild(context.ConsumeToken());
                if (!TryParseDataConstant(context))
                {
                    CreateErrorAndRecover("dataconstant after separator in data directive", context);
                    return false;
                }

                directive.AddChild(context.CurrentNode);
            }

            context.CurrentNode = directive;
            return true;
        }

        private bool TryParseTimesDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.TimesStatement))
                return false;

            var directive = context.ConsumeToken();
            if (!TryParseConstant(context))
            {
                CreateErrorAndRecover("constant after times command", context);
                return false;
            }

            directive.AddChild(context.CurrentNode);

            context.StoreTokenPosition();
            if (TryParseDataDirective(context))
            {
                context.DropStoredTokenPosition();
                directive.AddChild(context.CurrentNode);
                context.CurrentNode = directive;
                return true;
            }
            context.RestoreTokenPosition();
            context.StoreTokenPosition();
            if (TryParseOperation(context))
            {
                context.DropStoredTokenPosition();
                directive.AddChild(context.CurrentNode);
                context.CurrentNode = directive;
                return true;
            }
            context.DropStoredTokenPosition();
            CreateErrorAndRecover("data directive or operation after times", context);
            return false;
        }

        private bool TryParseIncludeDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.Include))
            {
                return false;
            }

            var directive = context.ConsumeToken();

            if(!IsToken(context, TokenType.String))
            {
                CreateErrorAndRecover("string after include", context);
                return false;
            }

            directive.AddChild(context.ConsumeToken());

            context.CurrentNode = directive;
            return true;
        }

        private bool TryParseOriginDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.Origin))
            {
                return false;
            }

            var directive = context.ConsumeToken();

            if (!TryParseConstant(context))
            {
                CreateErrorAndRecover("constant after origin", context);
                return false;
            }

            directive.AddChild(context.CurrentNode);

            context.CurrentNode = directive;
            return true;
        }

        private bool TryParseSegmentDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.Segment))
            {
                return false;
            }

            var directive = context.ConsumeToken();

            if (!IsToken(context, TokenType.Identifier))
            {
                CreateErrorAndRecover("segment identifier", context);
                return false;
            }

            directive.AddChild(context.ConsumeToken());

            if(TryParseConstant(context))
            {
                directive.AddChild(context.CurrentNode);
            }

            context.CurrentNode = directive;
            return true;
        }

        private bool TryParseConstExpr(ParseContext context)
        {
            if (!TryParseAddTerm(context))
                return false;

            while (IsToken(context, TokenType.AddOp))
            {
                var term1 = context.CurrentNode;
                var addNode = context.ConsumeToken();
                addNode.AddChild(term1);

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
                var term1 = context.CurrentNode;
                var mulNode = context.ConsumeToken();
                mulNode.AddChild(term1);

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
                var paren = context.ConsumeToken();
                if (TryParseConstExpr(context))
                {
                    paren.AddChild(context.CurrentNode);
                    if (!IsToken(context, TokenType.RParen))
                    {
                        CreateErrorAndRecover("')'", context);
                        return false;
                    }
                    else
                    {
                        paren.AddChild(context.ConsumeToken());
                        context.CurrentNode = paren;
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

                while (IsToken(context, TokenType.Separator))
                {
                    op.AddChild(context.ConsumeToken());

                    if (TryParseOperand(context))
                    {
                        op.AddChild(context.CurrentNode);
                        context.CurrentNode = op;
                    }
                    else
                    {
                        CreateErrorAndRecover("operand", context);
                        return false;
                    }
                }

                return true;
            }
            else
                return false;
        }

        private bool TryParseOp(ParseContext context)
        {
            switch (context.CurrentToken.TokenType)
            {
                case TokenType.Mnemonic:
                case TokenType.Identifier:
                    context.ConsumeToken();
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
            context.RestoreTokenPosition();
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
                        context.ConsumeToken();
                        return true;
                    }
                default: return false;
            }
        }

        private bool TryParseAbsolute(ParseContext context)
        {
            if (IsToken(context, TokenType.LBracket))
            {
                var parent = context.ConsumeToken();

                if (TryParseConstant(context))
                {
                    parent.AddChild(context.CurrentNode);

                    if (!IsToken(context, TokenType.RBracket))
                    {
                        CreateErrorAndRecover("']'", context);
                        return false;
                    }
                    else
                    {
                        parent.AddChild(context.ConsumeToken());
                        context.CurrentNode = parent;
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
                var parent = context.ConsumeToken();

                if (TryParseRegisterAlias(context))
                {
                    if (IsToken(context, TokenType.AddOp))
                    {
                        var reg = context.CurrentNode;
                        var displacement = context.ConsumeToken();
                        displacement.AddChild(reg);
                        parent.AddChild(displacement);

                        if (TryParseConstant(context))
                        {
                            displacement.AddChild(context.CurrentNode);

                            if (!IsToken(context, TokenType.RBracket))
                            {
                                CreateErrorAndRecover("']'", context);
                                return false;
                            }
                            else
                            {
                                parent.AddChild(context.ConsumeToken());
                                context.CurrentNode = parent;
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
                        var constant = context.CurrentNode;
                        var displacement = context.ConsumeToken();
                        displacement.AddChild(constant);
                        parent.AddChild(displacement);

                        if (TryParseRegisterAlias(context))
                        {
                            displacement.AddChild(context.CurrentNode);
                            if (!IsToken(context, TokenType.RBracket))
                            {
                                CreateErrorAndRecover("']'", context);
                                return false;
                            }
                            else
                            {
                                parent.AddChild(context.ConsumeToken());
                                context.CurrentNode = parent;
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
                var parent = context.ConsumeToken();

                if (TryParseRegisterAlias(context))
                {
                    if (IsToken(context, TokenType.AddOp))
                    {
                        var reg = context.CurrentNode;
                        var offset = context.ConsumeToken();
                        offset.AddChild(reg);
                        parent.AddChild(offset);

                        if (TryParseRegisterAlias(context))
                        {
                            offset.AddChild(context.CurrentNode);

                            if (!IsToken(context, TokenType.RBracket))
                            {
                                CreateErrorAndRecover("']'", context);
                                return false;
                            }
                            else
                            {
                                parent.AddChild(context.ConsumeToken());
                                context.CurrentNode = parent;
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
                var parent = context.ConsumeToken();

                if (TryParseRegisterAlias(context))
                {
                    parent.AddChild(context.CurrentNode);

                    if (!IsToken(context, TokenType.RBracket))
                    {
                        CreateErrorAndRecover("']'", context);
                        return false;
                    }
                    else
                    {
                        parent.AddChild(context.ConsumeToken());
                        context.CurrentNode = parent;
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
                context.ConsumeToken();
                return true;
            }

            return false;
        }

        private void CreateErrorAndRecover(
            string acceptedString,
            ParseContext context)
        {
            if (context.State is ParserState.Recovering || context.State is ParserState.Previewing)
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