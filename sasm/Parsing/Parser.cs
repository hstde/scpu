using System.Collections.Generic;

namespace Sasm.Parsing
{
    using Irony.Parsing;

    public class Parser
    {
        private readonly Grammar grammar;
        private readonly LanguageData languageData;
        private readonly Irony.Parsing.Parser internalParser;

        public Parser(NonTerminal startTerm)
        {
            grammar = new Grammar();
            languageData = new LanguageData(grammar);
            internalParser = new Irony.Parsing.Parser(languageData, startTerm);
        }

        public Parser()
        {
            grammar = new Grammar();
            languageData = new LanguageData(grammar);
            internalParser = new Irony.Parsing.Parser(languageData);
        }

        public ParseTree Parse(string source)
        {
            return Parse(source, "<source>");
        }

        public ParseTree Parse(string source, string fileName)
        {
            return internalParser.Parse(source, fileName);
        }

        public void BuildAst(ParseTree tree)
        {
            grammar.BuildAst(languageData, tree);
        }

    }
}