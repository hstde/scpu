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

        public ParseTreeNode CurrentNode { get; set; }
        public ParserState State { get; set; }

        public IReadOnlyList<Token> Tokens { get; }
        public Token CurrentToken => HasCurrentToken ? Tokens[tokenPosition] : eofToken;
        public bool HasCurrentToken => tokenPosition < Tokens.Count;
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
    }
}