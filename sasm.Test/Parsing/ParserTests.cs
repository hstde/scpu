namespace Sasm.Test.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using Sasm.Parsing;
    using Sasm.Parsing.Tokenizing;

    public static class ParserTests
    {
        public static IEnumerable<TestCaseData> NoErrorCases
        {
            get
            {
                TestCaseData CreateCaseSingleLine(string input, string name, params TestNode[] nodes)
                {
                    var startNode = new TestNode(TokenType.Start);
                    var line = new TestNode(TokenType.Line);
                    line.children.AddRange(nodes);
                    startNode.children.Add(line);
                    return new TestCaseData(input, startNode).SetName("{m}_" + name);
                }
                TestCaseData CreateCaseMultiLine(string input, string name, params TestNode[] nodes)
                {
                    var startNode = new TestNode(TokenType.Start);
                    startNode.children.AddRange(nodes);
                    return new TestCaseData(input, startNode).SetName("{m}_" + name);
                }
                TestNode Node(TokenType type, params TestNode[] children)
                {
                    var node = new TestNode(type);
                    node.children.AddRange(children);
                    return node;
                }

                yield return CreateCaseSingleLine(
                    "ab 1",
                    "identifier number",
                    Node(TokenType.Identifier, Node(TokenType.DecNumber)));
                yield return CreateCaseSingleLine(
                    "abc",
                    "one identifier",
                    Node(TokenType.Identifier));
                yield return CreateCaseSingleLine(
                    "ab 1+1",
                    "identifier two number addition",
                    Node(TokenType.Identifier, Node(TokenType.AddOp, Node(TokenType.DecNumber), Node(TokenType.DecNumber))));
                yield return CreateCaseSingleLine(
                    "ab abc+abc",
                    "identifier two ident addition",
                    Node(TokenType.Identifier, Node(TokenType.AddOp, Node(TokenType.Identifier), Node(TokenType.Identifier))));
                yield return CreateCaseSingleLine(
                    "ab 1+-1",
                    "identifier negative number addition",
                    Node(TokenType.Identifier, Node(TokenType.AddOp, Node(TokenType.DecNumber), Node(TokenType.DecNumber))));
                yield return CreateCaseSingleLine(
                    "ab 1+'a'",
                    "identifier addition with a character",
                    Node(TokenType.Identifier, Node(TokenType.AddOp, Node(TokenType.DecNumber), Node(TokenType.Char))));
                yield return CreateCaseSingleLine(
                    "ab 1+2*3",
                    "identifier addition and multiplication",
                    Node(TokenType.Identifier,
                        Node(TokenType.AddOp,
                            Node(TokenType.DecNumber),
                            Node(TokenType.MulOp,
                                Node(TokenType.DecNumber),
                                Node(TokenType.DecNumber)))));
                yield return CreateCaseSingleLine(
                    "ab 2*(1+3)",
                    "identifier subexpressions",
                    Node(TokenType.Identifier,
                        Node(TokenType.MulOp,
                            Node(TokenType.DecNumber),
                            Node(TokenType.LParen,
                                Node(TokenType.AddOp,
                                    Node(TokenType.DecNumber),
                                    Node(TokenType.DecNumber)),
                                Node(TokenType.RParen)))));
                yield return CreateCaseSingleLine(
                    "ab 1+(2+abc)",
                    "identifier subexpression with ident",
                    Node(TokenType.Identifier,
                        Node(TokenType.AddOp,
                            Node(TokenType.DecNumber),
                            Node(TokenType.LParen,
                                Node(TokenType.AddOp,
                                    Node(TokenType.DecNumber),
                                    Node(TokenType.Identifier)),
                                Node(TokenType.RParen)))));
                yield return CreateCaseSingleLine(
                    "ld a, 1",
                    "mnemonic with two arguments",
                    Node(TokenType.Mnemonic,
                        Node(TokenType.Register),
                        Node(TokenType.Separator),
                        Node(TokenType.DecNumber)));
                yield return CreateCaseSingleLine(
                    "",
                    "empty expression");
                yield return CreateCaseSingleLine(
                    "ld a, [de]",
                    "load a indirect de",
                    Node(TokenType.Mnemonic,
                        Node(TokenType.Register),
                        Node(TokenType.Separator),
                        Node(TokenType.LBracket,
                            Node(TokenType.Register),
                            Node(TokenType.RBracket))));
                yield return CreateCaseSingleLine(
                    "ld [ix+1], b",
                    "load displacement ix b",
                    Node(TokenType.Mnemonic,
                        Node(TokenType.LBracket,
                            Node(TokenType.AddOp,
                                Node(TokenType.Register),
                                Node(TokenType.DecNumber)),
                            Node(TokenType.RBracket)),
                        Node(TokenType.Separator),
                        Node(TokenType.Register)));
                yield return CreateCaseSingleLine(
                    "lea ix, [iy+hl]",
                    "load effective address to ix of iy offset hl",
                    Node(TokenType.Mnemonic,
                        Node(TokenType.Register),
                        Node(TokenType.Separator),
                        Node(TokenType.LBracket,
                            Node(TokenType.AddOp,
                                Node(TokenType.Register),
                                Node(TokenType.Register)),
                            Node(TokenType.RBracket))));
                yield return CreateCaseSingleLine(
                    "label:",
                    "label definition",
                    Node(TokenType.LabelDefinition));
                yield return CreateCaseSingleLine(
                    ";comment",
                    "comment",
                    Node(TokenType.Comment));
                yield return CreateCaseSingleLine(
                    "label:;comment",
                    "label with comment",
                    Node(TokenType.LabelDefinition),
                    Node(TokenType.Comment));
                yield return CreateCaseSingleLine(
                    "label: inc a",
                    "label with mnemonic and argument",
                    Node(TokenType.LabelDefinition),
                    Node(TokenType.Mnemonic,
                        Node(TokenType.Register)));
                yield return CreateCaseSingleLine(
                    "label: inc a ; increment a",
                    "label with mnemonic and argument and comment",
                    Node(TokenType.LabelDefinition),
                    Node(TokenType.Mnemonic,
                        Node(TokenType.Register)),
                    Node(TokenType.Comment));
                yield return CreateCaseSingleLine(
                    "mov a, b ; copy b into a",
                    "mnemonic with two arguments and comment",
                    Node(TokenType.Mnemonic,
                        Node(TokenType.Register),
                        Node(TokenType.Separator),
                        Node(TokenType.Register)),
                    Node(TokenType.Comment));
                yield return CreateCaseMultiLine(
                    ";comment1\n"
                    + ";comment2",
                    "two lines of comments",
                    Node(TokenType.Line, Node(TokenType.Comment)),
                    Node(TokenType.Line, Node(TokenType.Comment)));
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

        [TestCaseSource(nameof(ErrorCases))]
        public static void ReportsErrorsCorrectly(string input, SourceReference[] expectedLocations)
        {
            var tree = new Parser().Parse(input);

            var actualErrors = tree.Messages;

            var actualLocations = actualErrors.Select(e => e.Source).ToArray();

            Assert.That(actualLocations, Is.EqualTo(expectedLocations));
        }

        [TestCaseSource(nameof(NoErrorCases))]
        public static void ParsesCorrectly(string input, TestNode expectedTree)
        {
            var tree = new Parser().Parse(input.Split('\n'));

            AssertTree(expectedTree, tree);
        }

        private static void AssertTree(TestNode expected, ParseTree actual)
        {
            // we want to fail on the first problem, because the structure is probably garbage
            AssertNode(expected, actual.Root, expected.type.ToString());
        }

        private static void AssertNode(TestNode expected, ParseTreeNode actual, string nodePath)
        {
            Assert.Multiple(() =>
            {
                Assert.That(
                    actual.Token.TokenType,
                    Is.EqualTo(expected.type),
                    $"Tokentype differs on node ({nodePath})!");
                Assert.That(
                    actual.Children.Count,
                    Is.EqualTo(expected.children.Count),
                    $"Childcount differs on node ({nodePath})!");
            });

            for (int i = 0; i < expected.children.Count; i++)
            {
                AssertNode(
                    expected.children[i],
                    actual.Children[i],
                    nodePath + "." + expected.children[i].type);
            }
        }

        public class TestNode
        {
            public List<TestNode> children;
            public TokenType type;

            public TestNode(TokenType type)
            {
                this.type = type;
                this.children = new List<TestNode>();
            }

            public TestNode(ParseTreeNode node)
            {
                this.type = node.Token.TokenType;
                this.children = new List<TestNode>(node.Children.Count);
            }
        }
    }
}