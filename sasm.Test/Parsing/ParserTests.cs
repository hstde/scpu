namespace Sasm.Test.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using Sasm.Parsing;

    public static class ParserTests
    {
        public static IEnumerable<TestCaseData> NoErrorCases
        {
            get
            {
                TestCaseData CreateCase(string input, string name)
                    => new TestCaseData(input, Array.Empty<SourceReference>()).SetName("{m}_" + name);

                yield return CreateCase("ab 1", "identifier number");
                yield return CreateCase("abc", "one identifier");
                yield return CreateCase("ab 1+1", "identifier two number addition");
                yield return CreateCase("ab abc+abc", "identifier two ident addition");
                yield return CreateCase("ab 1+-1", "identifier negative number addition");
                yield return CreateCase("ab 1+'a'", "identifier addition with a character");
                yield return CreateCase("ab 1+2*3", "identifier addition and multiplication");
                yield return CreateCase("ab 2*(1+3)", "identifier subexpressions");
                yield return CreateCase("ab 1+(2+abc)", "identifier subexpression with ident");
                yield return CreateCase("ld a, 1", "mnemonic with two arguments");
                yield return CreateCase("", "empty expression");
                yield return CreateCase("ld a, [de]", "load a indirect de");
                yield return CreateCase("ld [ix+1], b", "load displacement ix b");
                yield return CreateCase("lea ix, [iy+hl]", "load effective address to ix of iy offset hl");
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
                yield return CreateCase("ab 1+bc", "register not expected with addition", (3, 1));
                yield return CreateCase("ab bc+1", "addition not expected", (5, 1));
                yield return CreateCase("12", "number not expected", (0, 2));
                yield return CreateCase("ab 1+(1+bc)", "register not expected in subexpression", (3, 1));
                yield return CreateCase("ab ()", "missing expression in parentheses", (3, 1));
                yield return CreateCase("ab (1+1", "missing closing parenthesis", (3, 1));
                yield return CreateCase("ab 1+2+3)", "missing opening parenthesis", (8, 1));
            }
        }

        [TestCaseSource(nameof(NoErrorCases))]
        [TestCaseSource(nameof(ErrorCases))]
        public static void ParsesCorrectly(string input, SourceReference[] expectedLocations)
        {
            var tree = new Parser().Parse(input);

            var actualErrors = tree.Messages;

            var actualLocations = actualErrors.Select(e => e.Source).ToArray();

            Assert.That(actualLocations, Is.EqualTo(expectedLocations));
        }
    }
}