namespace Sasm.Test.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using Sasm.Parsing;
    using Sasm.Parsing.ParseTree;
    using Sasm.Tokenizing;

    public static class ParserTests
    {
        public static IEnumerable<TestCaseData> NoErrorCases
        {
            get
            {
                TestCaseData CreateCase(string input, string name)
                    => new TestCaseData(input, Array.Empty<SourceReference>()).SetName("{m}_" + name);

                yield return CreateCase("1", "one number");
                yield return CreateCase("abc", "one identifier");
                yield return CreateCase("1+1", "two number addition");
                yield return CreateCase("abc+abc", "two ident addition");
                yield return CreateCase("1+-1", "negative number");
                yield return CreateCase("1+'a'", "addition with a character");
                yield return CreateCase("1+2*3", "addition and multiplication");
                yield return CreateCase("2*(1+3)", "subexpressions");
                yield return CreateCase("1+(2+abc)", "subexpression with ident");
            }
        }

        public static IEnumerable<TestCaseData> ErrorCases
        {
            get
            {
                TestCaseData CreateCase(
                    string input,
                    string name,
                    params (int start, int length)[] errorLocations)
                {
                    var locations = errorLocations
                        .Select(l => new SourceReference(0, l.start, l.length))
                        .ToArray();
                    return new TestCaseData(input, locations).SetName("{m}_" + name);
                }


                yield return CreateCase("bc", "register not expected", (0, 2));
                yield return CreateCase("1+bc", "register not expected with addition", (2, 2));
                yield return CreateCase("bc+1", "register not expected first argument", (0, 2));
                yield return CreateCase("ld bc", "mnemonic not expected", (0, 2));
                yield return CreateCase("1+(1+bc)", "register not expected in subexpression", (5, 2));
                yield return CreateCase("()", "missing expression in parentheses", (1, 1));
                yield return CreateCase("", "missing expression", (0, 0));
                yield return CreateCase("(1+1", "missing closing parenthesis", (4, 0));
                yield return CreateCase("1+2+3)", "missing opening parenthesis", (5, 1));
            }
        }

        [TestCaseSource(nameof(NoErrorCases))]
        [TestCaseSource(nameof(ErrorCases))]
        public static void ParsesCorrectly(string input, SourceReference[] expectedLocations)
        {
            var tokens = new Tokenizer().Tokenize(input);
            var tree = new Parser().ParseTokenList(tokens);

            var actualErrors = ErrorHelper.CollectErrors(tree);

            var actualLocations = actualErrors.Select(e => e.sourceReference).ToArray();

            Assert.That(actualLocations, Is.EqualTo(expectedLocations));
        }
    }
}