using System;
using System.Collections.Generic;
using System.Linq;
using Sasm.Parsing.Tokenizing;

namespace Sasm.Parsing
{
    public class ParseTree
    {
        public ParseTreeNode Root { get; }
        public IReadOnlyList<Token> Tokens { get; }
        public string FileName { get; }
        public IReadOnlyList<string> SourceLines { get; }
        public TimeSpan ParseTime { get; }
        public IReadOnlyList<LogMessage> Messages { get; }
        public bool HasErrors => Messages.Count > 0 && Messages.Any(e => e.Level >= ErrorLevel.Error);

        public ParseTree(ParseContext context)
        {
            Root = context.CurrentNode;
            Tokens = context.Tokens;
            FileName = context.FileName;
            SourceLines = context.SourceLines;
            ParseTime = context.ParseWatch.Elapsed;
            Messages = context.Messages;
        }
    }
}