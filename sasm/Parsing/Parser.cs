namespace Sasm.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sasm.Parsing.Tokenizing;
    using NT = Sasm.Parsing.ParseTreeNodeType;

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
            var startNode = context.CreateNonTerminal(NT.Start);
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
            var line = context.CreateNonTerminal(NT.Line);
            context.CurrentNode = line;

            if (IsToken(context, TokenType.LabelDefinition))
            {
                line.AddChild(context.CreateTerminal());
            }

            if (TryParseInstruction(context))
            {
                line.AddChild(context.CurrentNode);
            }

            if (IsToken(context, TokenType.Comment))
            {
                line.AddChild(context.CreateTerminal());
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
            return TryParseDirective(context) || TryParseOperation(context);
        }

        private bool TryParseDirective(ParseContext context)
        {
            return TryParseSegmentDirective(context)
                || TryParseOriginDirective(context)
                || TryParseIncludeDirective(context)
                || TryParseTimesDirective(context)
                || TryParseDataDirective(context)
                || TryParseWarningDirective(context)
                || TryParseConstDirective(context);
        }

        private bool TryParseConstDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.ConstantDeclaration))
                return false;

            var directive = context.CreateNonTerminal(NT.ConstDirective);
            directive.AddChild(context.CreateTerminal());

            if (!IsToken(context, TokenType.Identifier))
            {
                CreateErrorAndRecover("identifier after constant declaration", context);
                return false;
            }

            directive.AddChild(context.CreateTerminal());

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
            var dataconstant = context.CreateNonTerminal(NT.DataConstant);
            context.EnterPreview();

            switch (context.CurrentToken.TokenType)
            {
                case TokenType.String:
                case TokenType.EscapedString:
                    context.AcceptPreview();
                    dataconstant.AddChild(context.CreateTerminal());
                    context.CurrentNode = dataconstant;
                    return true;
            }

            if (TryParseConstant(context))
            {
                context.AcceptPreview();
                dataconstant.AddChild(context.CurrentNode);
                context.CurrentNode = dataconstant;
                return true;
            }

            context.AbortPreview();
            return false;
        }

        private bool TryParseWarningDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.WarningCommand))
                return false;

            var directive = context.CreateNonTerminal(NT.Warning);
            directive.AddChild(context.CreateTerminal());

            if (!TryParseDataConstant(context))
            {
                CreateErrorAndRecover("dataconstant in warning directive", context);
                return false;
            }

            directive.AddChild(context.CurrentNode);

            while (IsToken(context, TokenType.Separator))
            {
                directive.AddChild(context.CreateTerminal());
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

            var directive = context.CreateNonTerminal(NT.DataDirective);
            directive.AddChild(context.CreateTerminal());

            if (!TryParseDataConstant(context))
            {
                CreateErrorAndRecover("data constant in data directive", context);
                return false;
            }

            directive.AddChild(context.CurrentNode);

            while (IsToken(context, TokenType.Separator))
            {
                directive.AddChild(context.CreateTerminal());
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

            var directive = context.CreateNonTerminal(NT.TimesDirective);
            directive.AddChild(context.CreateTerminal());

            if (!TryParseConstant(context))
            {
                CreateErrorAndRecover("constant after times command", context);
                return false;
            }

            directive.AddChild(context.CurrentNode);

            if (TryParseDataDirective(context))
            {
                context.AcceptPreview();
                directive.AddChild(context.CurrentNode);
                context.CurrentNode = directive;
                return true;
            }
            if (TryParseOperation(context))
            {
                context.AcceptPreview();
                directive.AddChild(context.CurrentNode);
                context.CurrentNode = directive;
                return true;
            }

            CreateErrorAndRecover("data directive or operation after times", context);
            return false;
        }

        private bool TryParseIncludeDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.Include))
                return false;

            var directive = context.CreateNonTerminal(NT.IncludeDirective);
            directive.AddChild(context.CreateTerminal());

            if (!IsToken(context, TokenType.String))
            {
                CreateErrorAndRecover("string after include", context);
                return false;
            }

            directive.AddChild(context.CreateTerminal());

            context.CurrentNode = directive;
            return true;
        }

        private bool TryParseOriginDirective(ParseContext context)
        {
            if (!IsToken(context, TokenType.Origin))
            {
                return false;
            }

            var directive = context.CreateNonTerminal(NT.OriginDirective);
            directive.AddChild(context.CreateTerminal());

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
                return false;

            var directive = context.CreateNonTerminal(NT.SegmentDirective);
            directive.AddChild(context.CreateTerminal());

            if (!IsToken(context, TokenType.Identifier))
            {
                CreateErrorAndRecover("segment identifier", context);
                return false;
            }

            directive.AddChild(context.CreateTerminal());

            if (TryParseConstant(context))
            {
                directive.AddChild(context.CurrentNode);
            }

            context.CurrentNode = directive;
            return true;
        }

        private bool TryParseConstExpr(ParseContext context)
        {
            var constExpr = context.CreateNonTerminal(NT.ConstExpression);
            context.EnterPreview();

            if (!TryParseAddTerm(context))
            {
                context.AbortPreview();
                return false;
            }

            constExpr.AddChild(context.CurrentNode);

            if (!IsToken(context, TokenType.AddOp))
            {
                context.AcceptPreview();
                context.CurrentNode = constExpr;
                return true;
            }

            constExpr.AddChild(context.CreateTerminal());

            if (!TryParseAddTerm(context))
            {
                context.AbortPreview();
                return false;
            }

            context.AcceptPreview();
            constExpr.AddChild(context.CurrentNode);
            context.CurrentNode = constExpr;

            return true;
        }

        private bool TryParseAddTerm(ParseContext context)
        {
            var addTerm = context.CreateNonTerminal(NT.AddTerm);

            context.EnterPreview();
            if (!TryParseMulTerm(context))
            {
                context.AbortPreview();
                return false;
            }

            addTerm.AddChild(context.CurrentNode);

            if (!IsToken(context, TokenType.MulOp))
            {
                context.AcceptPreview();
                context.CurrentNode = addTerm;
                return true;
            }

            addTerm.AddChild(context.CreateTerminal());

            if (!TryParseMulTerm(context))
            {
                context.AbortPreview();
                return false;
            }

            context.AcceptPreview();
            addTerm.AddChild(context.CurrentNode);
            context.CurrentNode = addTerm;

            return true;
        }

        private bool TryParseMulTerm(ParseContext context)
        {
            var mulTerm = context.CreateNonTerminal(NT.MulTerm);

            if (IsToken(context, TokenType.LParen))
            {
                mulTerm.AddChild(context.CreateTerminal());

                if (TryParseConstExpr(context))
                {
                    mulTerm.AddChild(context.CurrentNode);
                    if (!IsToken(context, TokenType.RParen))
                    {
                        CreateErrorAndRecover("')'", context);
                        return false;
                    }
                    else
                    {
                        mulTerm.AddChild(context.CreateTerminal());
                        context.CurrentNode = mulTerm;
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
                mulTerm.AddChild(context.CurrentNode);
                context.CurrentNode = mulTerm;
                return true;
            }

            return false;
        }

        private bool TryParseOperation(ParseContext context)
        {
            var operation = context.CreateNonTerminal(NT.Operation);

            if (TryParseOp(context))
            {
                operation.AddChild(context.CurrentNode);

                if (!TryParseOperand(context))
                    return true;

                operation.AddChild(context.CurrentNode);

                while (IsToken(context, TokenType.Separator))
                {
                    operation.AddChild(context.CreateTerminal());

                    if (!TryParseOperand(context))
                    {
                        CreateErrorAndRecover("operand", context);
                        return false;
                    }

                    operation.AddChild(context.CurrentNode);
                }

                context.CurrentNode = operation;
                return true;
            }

            return false;
        }

        private bool TryParseOp(ParseContext context)
        {
            var op = context.CreateNonTerminal(NT.Op);

            switch (context.CurrentToken.TokenType)
            {
                case TokenType.Mnemonic:
                case TokenType.Identifier:
                    op.AddChild(context.CreateTerminal());
                    context.CurrentNode = op;
                    return true;
                default: return false;
            }
        }

        private bool TryParseOperand(ParseContext context)
        {
            var operand = context.CreateNonTerminal(NT.Operand);

            context.EnterPreview();
            if (!TryParseConstant(context)
                && !TryParseRegisterAlias(context)
                && !TryParseAbsolute(context)
                && !TryParseIndirect(context)
                && !TryParseDisplacement(context)
                && !TryParseOffset(context))
            {
                context.AbortPreview();
                return false;
            }
            context.AcceptPreview();
            operand.AddChild(context.CurrentNode);
            context.CurrentNode = operand;
            return true;
        }

        private bool TryParseConstant(ParseContext context)
        {
            var constant = context.CreateNonTerminal(NT.Constant);

            context.EnterPreview();
            if (TryParseConstExpr(context)
                || TryParseLiteral(context))
            {
                context.AcceptPreview();
                constant.AddChild(context.CurrentNode);
                context.CurrentNode = constant;
                return true;
            }

            context.AbortPreview();
            return false;
        }

        private bool TryParseLiteral(ParseContext context)
        {
            var literal = context.CreateNonTerminal(NT.Literal);

            switch (context.CurrentToken.TokenType)
            {
                case TokenType.BinNumber:
                case TokenType.OctNumber:
                case TokenType.DecNumber:
                case TokenType.HexNumber:
                case TokenType.Identifier:
                case TokenType.Char:
                    {
                        literal.AddChild(context.CreateTerminal());
                        context.CurrentNode = literal;
                        return true;
                    }
                default: return false;
            }
        }

        private bool TryParseAbsolute(ParseContext context)
        {
            var absolute = context.CreateNonTerminal(NT.AbsoluteMemAccess);

            if (IsToken(context, TokenType.LBracket))
            {
                context.EnterPreview();
                absolute.AddChild(context.CreateTerminal());

                if (TryParseConstant(context))
                {
                    absolute.AddChild(context.CurrentNode);
                    if (IsToken(context, TokenType.RBracket))
                    {
                        context.AcceptPreview();
                        absolute.AddChild(context.CreateTerminal());
                        context.CurrentNode = absolute;
                        return true;
                    }
                }

                context.AbortPreview();
            }
            return false;
        }

        private bool TryParseDisplacement(ParseContext context)
        {
            var parent = context.CreateNonTerminal(NT.DisplacementMemAccess);

            if (IsToken(context, TokenType.LBracket))
            {
                context.EnterPreview();
                parent.AddChild(context.CreateTerminal());

                if (TryParseRegisterAlias(context))
                {
                    if (IsToken(context, TokenType.AddOp))
                    {
                        var reg = context.CurrentNode;
                        var displacement = context.CreateTerminal();
                        displacement.AddChild(reg);
                        parent.AddChild(displacement);

                        if (TryParseConstant(context))
                        {
                            displacement.AddChild(context.CurrentNode);

                            if (IsToken(context, TokenType.RBracket))
                            {
                                context.AcceptPreview();
                                parent.AddChild(context.CreateTerminal());
                                context.CurrentNode = parent;
                                return true;
                            }
                        }
                    }
                }
                else if (TryParseConstant(context))
                {
                    if (IsToken(context, TokenType.AddOp))
                    {
                        var constant = context.CurrentNode;
                        var displacement = context.CreateTerminal();
                        displacement.AddChild(constant);
                        parent.AddChild(displacement);

                        if (TryParseRegisterAlias(context))
                        {
                            displacement.AddChild(context.CurrentNode);

                            if (IsToken(context, TokenType.RBracket))
                            {
                                context.AcceptPreview();
                                parent.AddChild(context.CreateTerminal());
                                context.CurrentNode = parent;
                                return true;
                            }
                        }
                    }
                }
                context.AbortPreview();
            }
            return false;
        }

        private bool TryParseOffset(ParseContext context)
        {
            var parent = context.CreateNonTerminal(NT.OffsetMemAccess);

            if (IsToken(context, TokenType.LBracket))
            {
                context.EnterPreview();
                parent.AddChild(context.CreateTerminal());

                if (TryParseRegisterAlias(context))
                {
                    if (IsToken(context, TokenType.AddOp))
                    {
                        var reg = context.CurrentNode;
                        var offset = context.CreateTerminal();
                        offset.AddChild(reg);
                        parent.AddChild(offset);

                        if (TryParseRegisterAlias(context))
                        {
                            offset.AddChild(context.CurrentNode);

                            if (IsToken(context, TokenType.RBracket))
                            {
                                context.AcceptPreview();
                                parent.AddChild(context.CreateTerminal());
                                context.CurrentNode = parent;
                                return true;
                            }
                        }
                    }
                }
                context.AbortPreview();
            }
            return false;
        }

        private bool TryParseIndirect(ParseContext context)
        {
            var parent = context.CreateNonTerminal(NT.IndirectMemAccess);
            if (IsToken(context, TokenType.LBracket))
            {
                context.EnterPreview();
                parent.AddChild(context.CreateTerminal());

                if (TryParseRegisterAlias(context))
                {
                    parent.AddChild(context.CurrentNode);

                    if (IsToken(context, TokenType.RBracket))
                    {
                        context.AcceptPreview();
                        parent.AddChild(context.CreateTerminal());
                        context.CurrentNode = parent;
                        return true;
                    }
                }
                context.AbortPreview();
            }
            return false;
        }

        private bool TryParseRegisterAlias(ParseContext context)
        {
            var registerAlias = context.CreateNonTerminal(NT.RegisterAlias);

            if (IsToken(context, TokenType.Register)
                || IsToken(context, TokenType.Identifier))
            {
                registerAlias.AddChild(context.CreateTerminal());
                context.CurrentNode = registerAlias;
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