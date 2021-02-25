namespace Sasm.Test.Tokenizing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using NUnit.Framework;
    using Sasm.Tokenizing;
    using static Sasm.Tokenizing.TokenType;

    public class TokenizerTests
    {
        public static IEnumerable<TestCaseData> SimpleCases
        {
            get
            {
                TestCaseData CreateCase(string text, TokenType type, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(type, 0, text, 0, text.Length),
                            CreateEolToken(text),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                string emptyLine = string.Empty;
                yield return new TestCaseData(
                    emptyLine,
                    new Token[] {
                        CreateEolToken(emptyLine),
                        CreateEofToken()
                    })
                    .SetName("{m}_Generates a valid End of line token on empty input");

                yield return CreateCase("(", LParen, "left parenthesis");
                yield return CreateCase(")", RParen, "right parenthesis");
                yield return CreateCase("+", AddOp, "operator +");
                yield return CreateCase("-", AddOp, "operator -");
                yield return CreateCase("*", MulOp, "operator *");
                yield return CreateCase("/", MulOp, "operator /");
                yield return CreateCase(",", Separator, "separator");
            }
        }

        public static IEnumerable<TestCaseData> LabelCases
        {
            get
            {
                TestCaseData CreateCase(string label, string testName)
                {
                    return new TestCaseData(
                        label,
                        new Token[]
                        {
                            new Token(LabelDefinition, 0, label, 0, label.Length - 1),
                            CreateEolToken(label),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                TestCaseData CreateErrorCase(string errorLabel, TokenType firstTokenType, string testName)
                {
                    return new TestCaseData(
                        errorLabel,
                        new Token[]
                        {
                            new Token(firstTokenType, 0, errorLabel, 0, errorLabel.Length - 1),
                            new Token(Unknown, 0, errorLabel, errorLabel.Length - 1, 1),
                            CreateEolToken(errorLabel),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("label:", "normal label");
                yield return CreateCase(".label:", "local label");
                yield return CreateCase("label1:", "alphanumeric label");
                yield return CreateCase(".1:", "numeric local label");
                yield return CreateCase(".label1:", "alphanumeric local label");
                yield return CreateCase("_1:", "underscore numeric label");
                yield return CreateCase("_label:", "underscore normal label");
                yield return CreateCase("_label1:", "underscore alphanumeric label");
                yield return CreateCase("_mov:", "underscore mnemonic label");
                yield return CreateCase("_dw:", "underscore command label");
                yield return CreateCase("_.include:", "underscore dot command label");
                yield return CreateCase("_label_:", "underscore label undrescore");
                yield return CreateCase("this.has.mulitple.dots:", "multiple dots label");
                yield return CreateCase("this_has_multiple_underscores:", "multiple underscores label");
                yield return CreateCase(".include_multiple_punctionations.:", "multiple punctionations label");

                yield return CreateErrorCase("1:", DecNumber, "label should not be number");
                yield return CreateErrorCase("mov:", Mnemonic, "label should not be mnemonic");
                yield return CreateErrorCase("dw:", DataDefinition, "label should not be command");
                yield return CreateErrorCase(".include:", Include, "label should not be dot command");
            }
        }

        public static IEnumerable<TestCaseData> NumberCases
        {
            get
            {
                TestCaseData CreateCase(string number, TokenType numberType, string testName, int start = 0)
                {
                    return new TestCaseData(
                        number,
                        new Token[]
                        {
                            new Token(numberType, 0, number, start, number.Length - start),
                            CreateEolToken(number),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("1234", DecNumber, "decimal number");
                yield return CreateCase("01234", OctNumber, "octal number");
                yield return CreateCase("0b1001", BinNumber, "binary number");
                yield return CreateCase("0x100f", HexNumber, "hexadecimal number");
                yield return CreateCase("-1234", DecNumber, "negative decimal number");
                yield return CreateCase("-01234", DecNumber, "negative decimal number with leading zero");
                yield return CreateCase(" -1234", DecNumber, "negative decimal number with leading whitespace", 1);

                yield return CreateCase("12_34", DecNumber, "decimal number with grouping helper");
                yield return CreateCase("1234_", DecNumber, "decimal number with grouping helper after last digit");
                yield return CreateCase("012_34", OctNumber, "octal number with grouping helper");
                yield return CreateCase("0_1234", OctNumber, "octal number with grouping helper after prefix");
                yield return CreateCase("01234_", OctNumber, "octal number with grouping helper after last digit");
                yield return CreateCase("0b10_01", BinNumber, "binary number with grouping helper");
                yield return CreateCase("0b_1001", BinNumber, "binary number with grouping helper after prefix");
                yield return CreateCase("0b1001_", BinNumber, "binary number with grouping helper after last digit");
                yield return CreateCase("0x10_0f", HexNumber, "hexadecimal number with grouping helper");
                yield return CreateCase("0x_100f", HexNumber, "hexadecimal number with grouping helper after prefix");
                yield return CreateCase("0x100f_", HexNumber, "hexadecimal number with grouping helper after last digit");
            }
        }

        public static IEnumerable<TestCaseData> IdentifierCases
        {
            get
            {
                TestCaseData CreateCase(string text, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(Identifier, 0, text, 0, text.Length),
                            CreateEolToken(text),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("ident", "normal identifier");
                yield return CreateCase("ident1", "alphanumeric identifier");
                yield return CreateCase("_1", "underscore numeric identifier");
                yield return CreateCase(".1", "local label numeric identifier");
                yield return CreateCase(".ident", "local label identifier");
                yield return CreateCase(".ident1", "local label alphanumeric identifier");
                yield return CreateCase("$", "current start of line address identifier");
            }
        }

        public static IEnumerable<TestCaseData> MnemonicCases
        {
            get
            {
                TestCaseData CreateCase(string text, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(Mnemonic, 0, text, 0, text.Length),
                            CreateEolToken(text),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("mov", "lower case mnemonic");
                yield return CreateCase("MOV", "upper case mnemonic");
                yield return CreateCase("mOv", "mixed case mnemonic");
            }
        }

        public static IEnumerable<TestCaseData> RegisterCases
        {
            get
            {
                TestCaseData CreateCase(string text, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(Register, 0, text, 0, text.Length),
                            CreateEolToken(text),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("bc", "lower case register");
                yield return CreateCase("BC", "upper case register");
                yield return CreateCase("bC", "mixed case register");
            }
        }

        public static IEnumerable<TestCaseData> CommandCases
        {
            get
            {
                TestCaseData CreateCase(string text, TokenType type, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(type, 0, text, 0, text.Length),
                            CreateEolToken(text),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase(".include", Include, "dotted lower case command");
                yield return CreateCase("db", DataDefinition, "not dotted lower case command");

                yield return CreateCase(".INCLUDE", Include, "dotted upper case command");
                yield return CreateCase("DB", DataDefinition, "not dotted upper case command");


                yield return CreateCase(".iNcLuDe", Include, "dotted mixed case command");
                yield return CreateCase("dB", DataDefinition, "not dotted mixed case command");
            }
        }

        public static IEnumerable<TestCaseData> CommentCases
        {
            get
            {
                TestCaseData CreateCase(string text, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(Comment, 0, text, 0, text.Length),
                            CreateEolToken(text),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("; text", "comment");
                yield return CreateCase(";", "empty comment");
                yield return CreateCase("; mov hl, 3", "comment overrides other tokens");
            }
        }

        public static IEnumerable<TestCaseData> CharCases
        {
            get
            {
                TestCaseData CreateCase(string text, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(TokenType.Char, 0, text, 1, text.Length - 2),
                            CreateEolToken(text),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                TestCaseData CreateErrorCase(string errorLabel, TokenType secondTokenType, string testName)
                {
                    return new TestCaseData(
                        errorLabel,
                        new Token[]
                        {
                            new Token(Unknown, 0, errorLabel, 0, 1),
                            new Token(secondTokenType, 0, errorLabel, 1, errorLabel.Length - 1),
                            CreateEolToken(errorLabel),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                TestCaseData CreateErrorCase2(
                    string errorLabel,
                    string testName,
                    params (TokenType type, int start, int length)[] expectedTokens)
                {
                    var tokBuilder = expectedTokens
                        .Select(t => new Token(t.type, 0, errorLabel, t.start, t.length))
                        .Append(CreateEolToken(errorLabel))
                        .Append(CreateEofToken())
                        .ToArray();

                    return new TestCaseData(
                        errorLabel,
                        tokBuilder)
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("'c'", "normal character");
                yield return CreateCase("'\\'", "char that looks like an escaped char");

                yield return CreateErrorCase("'c", Register, "missing closing char delimiter");
                yield return CreateErrorCase2("'\\n'", "multiple characters", (Unknown, 0, 1), (Unknown, 1, 1), (Identifier, 2, 1), (Unknown, 3, 1));
                yield return CreateErrorCase2("''\\'", "multiple delimitors", (Unknown, 0, 1), (TokenType.Char, 2, 1));
            }
        }

        public static IEnumerable<TestCaseData> StringCases
        {
            get
            {
                TestCaseData CreateCase(string text, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(TokenType.String, 0, text, 1, text.Length - 2),
                            CreateEolToken(text),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }
                TestCaseData CreateCaseEscaped(string text, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(TokenType.EscapedString, 0, text, 1, text.Length - 2),
                            CreateEolToken(text),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                TestCaseData CreateErrorCase(string errorLabel, TokenType secondTokenType, string testName)
                {
                    return new TestCaseData(
                        errorLabel,
                        new Token[]
                        {
                            new Token(Unknown, 0, errorLabel, 0, 1),
                            new Token(secondTokenType, 0, errorLabel, 1, errorLabel.Length - 1),
                            CreateEolToken(errorLabel),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                TestCaseData CreateErrorCase2(string errorLabel, TokenType secondTokenType, int secondLength, TokenType thirdTokenType, int thirdLength, string testName)
                {
                    return new TestCaseData(
                        errorLabel,
                        new Token[]
                        {
                            new Token(Unknown, 0, errorLabel, 0, 1),
                            new Token(secondTokenType, 0, errorLabel, 1, secondLength),
                            new Token(secondTokenType, 0, errorLabel, 1 + secondLength, thirdLength),
                            CreateEolToken(errorLabel),
                            CreateEofToken()
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("\"\"", "empty string");
                yield return CreateCase("\".\\path\\file.ext\"", "path");
                yield return CreateCase("\".\\path\\\"", "path with backslash at end");
                yield return CreateCaseEscaped("`escaped string`", "special escaped string");
                yield return CreateCaseEscaped("`escaped string\\n`", "escaped string with \\n");

                yield return CreateErrorCase("\"c", Register, "missing closing string delimiter");
                yield return CreateErrorCase2("`\\`", Unknown, 1, Unknown, 1, "missing closing string delimiter while having escaped delimiter");
            }
        }

        public static IEnumerable<TestCaseData> MinusOperatorPrefixCases
        {
            get
            {
                TestCaseData CreateCase(
                    string line,
                    string name,
                    params (TokenType type, int start, int length)[] tokens)
                {
                    var tokenBuilder = new List<Token>();

                    foreach (var t in tokens)
                        tokenBuilder.Add(new Token(t.type, 0, line, t.start, t.length));

                    tokenBuilder.Add(CreateEolToken(line));
                    tokenBuilder.Add(CreateEofToken());

                    return new TestCaseData(
                        line,
                        tokenBuilder.ToArray())
                        .SetName("{m}_" + name);
                }

                yield return CreateCase(
                    "1-1", "simple substraction without whitespace",
                    (TokenType.DecNumber, 0, 1), (TokenType.AddOp, 1, 1), (TokenType.DecNumber, 2, 1));
                yield return CreateCase(
                    "1 - 1", "simple substraction with whitespace",
                    (TokenType.DecNumber, 0, 1), (TokenType.AddOp, 2, 1), (TokenType.DecNumber, 4, 1));
                yield return CreateCase(
                    "1 -1", "simple substraction with no whitespace between minus and number",
                    (TokenType.DecNumber, 0, 1), (TokenType.AddOp, 2, 1), (TokenType.DecNumber, 3, 1));
                yield return CreateCase(
                    "i-1", "simple substraction from ident",
                    (TokenType.Identifier, 0, 1), (TokenType.AddOp, 1, 1), (TokenType.DecNumber, 2, 1));
                yield return CreateCase(
                    "$-1", "simple substraction from special ident",
                    (TokenType.Identifier, 0, 1), (TokenType.AddOp, 1, 1), (TokenType.DecNumber, 2, 1));
                yield return CreateCase(
                    "bc-1", "simple substraction from register",
                    (TokenType.Register, 0, 2), (TokenType.AddOp, 2, 1), (TokenType.DecNumber, 3, 1));
                yield return CreateCase(
                    "(1-2)-3", "substraction with partheses",
                    (TokenType.LParen, 0, 1),
                        (TokenType.DecNumber, 1, 1), (TokenType.AddOp, 2, 1), (TokenType.DecNumber, 3, 1),
                    (TokenType.RParen, 4, 1),
                    (TokenType.AddOp, 5, 1), (TokenType.DecNumber, 6, 1));
                yield return CreateCase(
                    "(-1", "negative after opening parenthesis",
                    (TokenType.LParen, 0, 1), (TokenType.DecNumber, 1, 2));
                yield return CreateCase(
                    " -1", "negative after whitespace",
                    (TokenType.DecNumber, 1, 2));
            }
        }

        [TestCaseSource(nameof(SimpleCases))]
        [TestCaseSource(nameof(LabelCases))]
        [TestCaseSource(nameof(NumberCases))]
        [TestCaseSource(nameof(IdentifierCases))]
        [TestCaseSource(nameof(MnemonicCases))]
        [TestCaseSource(nameof(RegisterCases))]
        [TestCaseSource(nameof(CommandCases))]
        [TestCaseSource(nameof(CommentCases))]
        [TestCaseSource(nameof(CharCases))]
        [TestCaseSource(nameof(StringCases))]
        [TestCaseSource(nameof(MinusOperatorPrefixCases))]
        public void TokenizesCorrectly(string content, Token[] expectedTokens)
        {
            var tokenizer = new Tokenizer();
            var actualTokens = tokenizer.Tokenize(content).ToArray();

            Assert.That(actualTokens, Is.EqualTo(expectedTokens));
        }

        private static Token CreateEolToken(string line, int lineNumber = 0)
        {
            return new Token(EndOfLine, lineNumber, line, line.Length, 0);
        }

        private static Token CreateEofToken(int lineNumber = 1)
        {
            return new Token(EndOfFile, lineNumber, "", 0, 0);
        }
    }
}