using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sasm.Parsing.Tokenizing;

namespace Sasm.Parsing
{
    public class ParseContext
    {
        private int tokenPosition;
        private List<LogMessage> messages;
        private bool hasErrors;
        private Token eofToken;
        private Stack<int> tokenPositionStack;

        public ParseTreeNode CurrentNode { get; set; }
        public ParserState State { get; set; }

        public IReadOnlyList<Token> Tokens { get; }
        public Token CurrentToken => tokenPosition < Tokens.Count ? Tokens[tokenPosition] : eofToken;
        public Token? NextToken => tokenPosition + 1 < Tokens.Count ? Tokens[tokenPosition + 1] : null;
        public string FileName { get; }
        public IReadOnlyList<string> SourceLines { get; }
        public Stopwatch ParseWatch { get; }
        public IReadOnlyList<LogMessage> Messages => messages
            ?? (IReadOnlyList<LogMessage>)Array.Empty<LogMessage>();
        public bool HasErrors
        {
            get => hasErrors;
            set => hasErrors |= value;
        }

        public ParseContext(IReadOnlyList<string> source, string filename, IReadOnlyList<Token> tokens)
        {
            SourceLines = source;
            FileName = filename;
            Tokens = tokens;

            State = ParserState.Init;
            ParseWatch = new Stopwatch();
            tokenPosition = 0;
            eofToken = new Token(TokenType.EndOfFile, SourceLines.Count, "", 0);
        }

        public void AddError(SourceReference source, string message)
        {
            AddMessage(ErrorLevel.Error, source, message);
        }

        public void AddMessage(ErrorLevel level, SourceReference source, string message)
        {
            if (messages is null)
                messages = new List<LogMessage>();

            messages.Add(new LogMessage(level, message, source));
        }

        public bool MoveNextToken()
        {
            return ++tokenPosition < Tokens.Count;
        }

        public ParseTreeNode ConsumeToken()
        {
            CurrentNode = new ParseTreeNode(CurrentToken);
            MoveNextToken();
            return CurrentNode;
        }

        public void StoreTokenPosition()
        {
            State = ParserState.Previewing;
            if (tokenPositionStack is null)
                tokenPositionStack = new Stack<int>();
            tokenPositionStack.Push(tokenPosition);
        }

        public bool RestoreTokenPosition()
        {
            State = ParserState.Parsing;
            if (tokenPositionStack is null || !tokenPositionStack.Any())
                return false;
            tokenPosition = tokenPositionStack.Pop();
            return true;
        }

        public bool DropStoredTokenPosition()
        {
            State = ParserState.Parsing;
            if (tokenPositionStack is null || !tokenPositionStack.Any())
                return false;
            tokenPositionStack.Pop();
            return true;
        }
    }
}