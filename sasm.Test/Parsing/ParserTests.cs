namespace Sasm.Test.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Irony.Parsing;
    using NUnit.Framework;
    using Sasm.Parsing;
    using Grammar = Sasm.Parsing.Grammar;
    using Parser = Sasm.Parsing.Parser;

    public static class ParserTests
    {
        private static Grammar G = new Grammar();
        public static IEnumerable<TestCaseData> NoErrorCases
        {
            get
            {
                TestCaseData CreateCaseSingleLine(string input, string name, params TestNode[] nodes)
                {
                    var startNode = new TestNode(G.Start);
                    if ((nodes.Any()))
                    {
                        var line = new TestNode(G.LabledInstruction);
                        line.children.AddRange(nodes);
                        startNode.children.Add(line);
                    }
                    return new TestCaseData(input, startNode).SetName("{m}_" + name);
                }
                TestNode Node(BnfTerm type, params TestNode[] children)
                {
                    var node = new TestNode(type);
                    node.children.AddRange(children);
                    return node;
                }
                TestNode NodeS(string type, params TestNode[] children)
                {
                    var node = new TestNode(type);
                    node.children.AddRange(children);
                    return node;
                }


                yield return CreateCaseSingleLine(
                    "ab 1",
                    "identifier number",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Ident)),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Constant,
                                        Node(G.Literal,
                                            Node(G.Number))))))));
                yield return CreateCaseSingleLine(
                    "abc",
                    "one identifier",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Ident)),
                            Node(G.OperandList))));
                yield return CreateCaseSingleLine(
                    "ab 1+1",
                    "identifier two number addition",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Ident)),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Constant,
                                        Node(G.BinExpr,
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Number))),
                                            Node(G.BinOp,
                                                new TestNode("+")),
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Number))))))))));
                yield return CreateCaseSingleLine(
                    "ab abc+abc",
                    "identifier two ident addition",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Ident)),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Constant,
                                        Node(G.BinExpr,
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Ident))),
                                            Node(G.BinOp,
                                                new TestNode("+")),
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Ident))))))))));
                yield return CreateCaseSingleLine(
                    "ab 1+-1",
                    "identifier negative number addition",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Ident)),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Constant,
                                        Node(G.BinExpr,
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Number))),
                                            Node(G.BinOp,
                                                new TestNode("+")),
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Number))))))))));
                yield return CreateCaseSingleLine(
                    "ab 1+'a'",
                    "identifier addition with a character",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Ident)),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Constant,
                                        Node(G.BinExpr,
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Number))),
                                            Node(G.BinOp,
                                                new TestNode("+")),
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.CharLiteral))))))))));
                yield return CreateCaseSingleLine(
                    "ab 1+2*3",
                    "identifier addition and multiplication",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Ident)),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Constant,
                                        Node(G.BinExpr,
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Number))),
                                            Node(G.BinOp,
                                                new TestNode("+")),
                                            Node(G.Constant,
                                                Node(G.BinExpr,
                                                    Node(G.Constant,
                                                        Node(G.Literal,
                                                            Node(G.Number))),
                                                    Node(G.BinOp,
                                                        new TestNode("*")),
                                                    Node(G.Constant,
                                                        Node(G.Literal,
                                                            Node(G.Number))))))))))));
                yield return CreateCaseSingleLine(
                    "ab 2*(1+3)",
                    "identifier subexpressions",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Ident)),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Constant,
                                        Node(G.BinExpr,
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Number))),
                                            Node(G.BinOp,
                                                new TestNode("*")),
                                            Node(G.Constant,
                                                new TestNode("("),
                                                Node(G.Constant,
                                                    Node(G.BinExpr,
                                                        Node(G.Constant,
                                                            Node(G.Literal,
                                                                Node(G.Number))),
                                                        Node(G.BinOp,
                                                            new TestNode("+")),
                                                        Node(G.Constant,
                                                            Node(G.Literal,
                                                                Node(G.Number))))),
                                                new TestNode(")")))))))));
                yield return CreateCaseSingleLine(
                    "ab 1+(2+abc)",
                    "identifier subexpression with ident",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Ident)),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Constant,
                                        Node(G.BinExpr,
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Number))),
                                            Node(G.BinOp,
                                                new TestNode("+")),
                                            Node(G.Constant,
                                                new TestNode("("),
                                                Node(G.Constant,
                                                    Node(G.BinExpr,
                                                        Node(G.Constant,
                                                            Node(G.Literal,
                                                                Node(G.Number))),
                                                        Node(G.BinOp,
                                                            new TestNode("+")),
                                                        Node(G.Constant,
                                                            Node(G.Literal,
                                                                Node(G.Ident))))),
                                                new TestNode(")")))))))));
                yield return CreateCaseSingleLine(
                    "ld a, 1",
                    "mnemonic with two arguments",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Mnemonic,
                                    new TestNode("ld"))),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Register,
                                        new TestNode("a"))),
                                Node(G.Operand,
                                    Node(G.Constant,
                                        Node(G.Literal,
                                            Node(G.Number))))))));
                yield return CreateCaseSingleLine(
                    "",
                    "empty expression");
                yield return CreateCaseSingleLine(
                    "ld a, [de]",
                    "load a indirect de",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Mnemonic,
                                    new TestNode("ld"))),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Register,
                                        new TestNode("a"))),
                                Node(G.Operand,
                                    Node(G.IndirectMemAccess,
                                        new TestNode("["),
                                        Node(G.Register,
                                            new TestNode("de")),
                                        new TestNode("]")))))));
                yield return CreateCaseSingleLine(
                    "ld [ix+1], b",
                    "load displacement ix b",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Mnemonic,
                                    new TestNode("ld"))),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.DisplacementMemAccess,
                                        new TestNode("["),
                                        NodeS("Unnamed2",
                                            Node(G.Register,
                                                new TestNode("ix")),
                                            new TestNode("+"),
                                            Node(G.Constant,
                                                Node(G.Literal,
                                                    Node(G.Number)))),
                                        new TestNode("]"))),
                                Node(G.Operand,
                                    Node(G.Register,
                                        new TestNode("b")))))));
                yield return CreateCaseSingleLine(
                    "lea ix, [iy+hl]",
                    "load effective address to ix of iy offset hl",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Mnemonic,
                                    new TestNode("lea"))),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Register,
                                        new TestNode("ix"))),
                                Node(G.Operand,
                                    Node(G.OffsetMemAccess,
                                        new TestNode("["),
                                        Node(G.Register,
                                            new TestNode("iy")),
                                        new TestNode("+"),
                                        Node(G.Register,
                                            new TestNode("hl")),
                                        new TestNode("]")))))));
                yield return CreateCaseSingleLine(
                    "label:",
                    "label definition",
                    Node(G.LabelDefinition,
                        Node(G.Ident),
                        new TestNode(":")));
                yield return CreateCaseSingleLine(
                    ";comment",
                    "comment");
                yield return CreateCaseSingleLine(
                    "label:;comment",
                    "label with comment",
                    Node(G.LabelDefinition,
                        Node(G.Ident),
                        new TestNode(":")));
                yield return CreateCaseSingleLine(
                    "label: inc a",
                    "label with mnemonic and argument",
                    Node(G.LabelDefinition,
                        Node(G.Ident),
                        new TestNode(":")),
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Mnemonic,
                                    new TestNode("inc"))),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Register,
                                        new TestNode("a")))))));
                yield return CreateCaseSingleLine(
                    "label: inc a ; increment a",
                    "label with mnemonic and argument and comment",
                    Node(G.LabelDefinition,
                        Node(G.Ident),
                        new TestNode(":")),
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Mnemonic,
                                    new TestNode("inc"))),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Register,
                                        new TestNode("a")))))));
                yield return CreateCaseSingleLine(
                    "mov a, b ; copy b into a",
                    "mnemonic with two arguments and comment",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Mnemonic,
                                    new TestNode("mov"))),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.Register,
                                        new TestNode("a"))),
                                Node(G.Operand,
                                    Node(G.Register,
                                        new TestNode("b")))))));
                yield return CreateCaseSingleLine(
                    "ld [1], a",
                    "load absolute with register",
                    Node(G.Instruction,
                        Node(G.Operation,
                            Node(G.Op,
                                Node(G.Mnemonic,
                                    new TestNode("ld"))),
                            Node(G.OperandList,
                                Node(G.Operand,
                                    Node(G.AbsoluteMemAccess,
                                        new TestNode("["),
                                        Node(G.Constant,
                                            Node(G.Literal,
                                                Node(G.Number))),
                                        new TestNode("]"))),
                                Node(G.Operand,
                                    Node(G.Register,
                                        new TestNode("a")))))));
                yield return CreateCaseSingleLine(
                    ".include \"test.inc\"",
                    "include",
                    Node(G.Instruction,
                        Node(G.Directive,
                            Node(G.IncludeDirective,
                                new TestNode(".include"),
                                Node(G.NormalString)))));
                yield return CreateCaseSingleLine(
                    ".org 100+2",
                    "origin",
                    Node(G.Instruction,
                        Node(G.Directive,
                            Node(G.OriginDirective,
                                new TestNode(".org"),
                                Node(G.Constant,
                                    Node(G.BinExpr,
                                        Node(G.Constant,
                                            Node(G.Literal,
                                                Node(G.Number))),
                                        Node(G.BinOp,
                                            new TestNode("+")),
                                        Node(G.Constant,
                                            Node(G.Literal,
                                                Node(G.Number)))))))));
                yield return CreateCaseSingleLine(
                    ".times 50 db 0",
                    "times with data command",
                    Node(G.Instruction,
                        Node(G.Directive,
                            Node(G.TimesDirective,
                                new TestNode(".times"),
                                Node(G.Constant,
                                    Node(G.Literal,
                                        Node(G.Number))),
                                NodeS("Unnamed1",
                                    Node(G.DataDirective,
                                        Node(G.DataDefinition,
                                            NodeS("db")),
                                        Node(G.DataConstantList,
                                            Node(G.DataConstant,
                                                Node(G.Constant,
                                                    Node(G.Literal,
                                                        Node(G.Number)))))))))));
                yield return CreateCaseSingleLine(
                    ".times 50 not",
                    "times with operation",
                    Node(G.Instruction,
                        Node(G.Directive,
                            Node(G.TimesDirective,
                                new TestNode(".times"),
                                Node(G.Constant,
                                    Node(G.Literal,
                                        Node(G.Number))),
                                NodeS("Unnamed1",
                                    Node(G.Operation,
                                        Node(G.Op,
                                            Node(G.Mnemonic,
                                                NodeS("not"))),
                                        Node(G.OperandList)))))));
            }
        }

        public static IEnumerable<TestCaseData> ErrorCases
        {
            get
            {
                TestCaseData CreateCase(
                    string input,
                    string name,
                    params int[] errorLocations)
                {
                    var locations = errorLocations
                        .Select(l => new SourceLocation(l, 0, l))
                        .ToArray();
                    return new TestCaseData(input, locations).SetName("{m}_" + name);
                }


                yield return CreateCase("bc", "register not expected", 0);
                yield return CreateCase("ab 1+bc", "register not expected with addition", 5);
                yield return CreateCase("ab bc+1", "addition not expected", 5);
                yield return CreateCase("12", "number not expected", 0);
                yield return CreateCase("ab 1+(1+bc)", "register not expected in subexpression", 8);
                yield return CreateCase("ab ()", "missing expression in parentheses", 4);
                yield return CreateCase("ab (1+1", "missing closing parenthesis", 7);
                yield return CreateCase("ab 1+2+3)", "missing opening parenthesis", 8);
            }
        }

        [TestCaseSource(nameof(ErrorCases))]
        public static void ReportsErrorsCorrectly(string input, SourceLocation[] expectedLocations)
        {
            var tree = new Parser().Parse(input);

            var actualErrors = tree.ParserMessages;

            var actualLocations = actualErrors.Select(e => e.Location).ToArray();

            Assert.That(actualLocations, Is.EqualTo(expectedLocations));
        }

        [TestCaseSource(nameof(NoErrorCases))]
        public static void ParsesCorrectly(string input, TestNode expectedTree)
        {
            var tree = new Parser().Parse(input);

            AssertTree(expectedTree, tree);
        }

        private static void AssertTree(TestNode expected, ParseTree actual)
        {
            // we want to fail on the first problem, because the structure is probably garbage
            AssertNode(expected, actual.Root, expected.type.ToString());
        }

        private static void AssertNode(TestNode expected, ParseTreeNode actual, string nodePath)
        {
            var children = actual.ChildNodes.Where(e => e.Term is BnfExpression);
            Assert.Multiple(() =>
            {
                Assert.That(
                    actual.Term.Name,
                    Is.EqualTo(expected.type),
                    $"Tokentype differs on node ({nodePath})!");
                Assert.That(
                    actual.ChildNodes.Count,
                    Is.EqualTo(expected.children.Count),
                    $"Childcount differs on node ({nodePath})!");
            });

            for (int i = 0; i < expected.children.Count; i++)
            {
                AssertNode(
                    expected.children[i],
                    actual.ChildNodes[i],
                    nodePath + "." + expected.children[i].type);
            }
        }

        public class TestNode
        {
            public List<TestNode> children;
            public string type;

            public TestNode(Irony.Parsing.BnfTerm type)
            {
                this.type = type.Name;
                this.children = new List<TestNode>();
            }

            public TestNode(string type)
            {
                this.type = type;
                this.children = new List<TestNode>();
            }
        }
    }
}