using System.Collections.Generic;

namespace Sasm.Parsing
{
    using Irony.Parsing;

    public class Parser
    {
        public char[] NewLine { get; }
        private readonly Irony.Parsing.Parser internalParser;

        public Parser(NonTerminal startTerm)
        {
            internalParser = new Irony.Parsing.Parser(new LanguageData(Grammar.Instance), startTerm);
            NewLine = internalParser.Language.Grammar.NewLine.LineTerminators.ToCharArray();
        }

        public Parser() : this(Grammar.Instance.Root)
        {
        }

        public ParseTree Parse(string source)
        {
            return Parse(source, "<source>");
        }

        public ParseTree Parse(string source, string fileName)
        {
            return internalParser.Parse(source, fileName);
        }


    }
}