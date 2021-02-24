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
                            CreateEolToken(text)
                        })
                        .SetName("{m}_" + testName);
                }

                string emptyLine = string.Empty;
                yield return new TestCaseData(emptyLine, new Token[] { CreateEolToken(emptyLine) })
                    .SetName("{m}_Generates a valid End of line token on empty input");

                yield return CreateCase("(", LParen, "left parenthesis");
                yield return CreateCase(")", RParen, "right parenthesis");
                yield return CreateCase("+", Operator, "operator +");
                yield return CreateCase("-", Operator, "operator -");
                yield return CreateCase("*", Operator, "operator *");
                yield return CreateCase("/", Operator, "operator /");
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
                            CreateEolToken(label)
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
                            CreateEolToken(errorLabel)
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
                yield return CreateErrorCase("dw:", Command, "label should not be command");
                yield return CreateErrorCase(".include:", Command, "label should not be dot command");
            }
        }

        public static IEnumerable<TestCaseData> NumberCases
        {
            get
            {
                TestCaseData CreateCase(string number, TokenType numberType, string testName)
                {
                    return new TestCaseData(
                        number,
                        new Token[]
                        {
                            new Token(numberType, 0, number, 0, number.Length),
                            CreateEolToken(number)
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("1234", DecNumber, "decimal number");
                yield return CreateCase("01234", OctNumber, "octal number");
                yield return CreateCase("0b1001", BinNumber, "binary number");
                yield return CreateCase("0x100f", HexNumber, "hexadecimal number");

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
                            CreateEolToken(text)
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
                            CreateEolToken(text)
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
                            CreateEolToken(text)
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
                TestCaseData CreateCase(string text, string testName)
                {
                    return new TestCaseData(
                        text,
                        new Token[]
                        {
                            new Token(Command, 0, text, 0, text.Length),
                            CreateEolToken(text)
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase(".include", "dotted lower case command");
                yield return CreateCase("db", "not dotted lower case command");

                yield return CreateCase(".INCLUDE", "dotted upper case command");
                yield return CreateCase("DB", "not dotted upper case command");


                yield return CreateCase(".iNcLuDe", "dotted mixed case command");
                yield return CreateCase("dB", "not dotted mixed case command");
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
                            CreateEolToken(text)
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
                            CreateEolToken(text)
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
                            CreateEolToken(errorLabel)
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
                            CreateEolToken(errorLabel)
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("'c'", "normal character");
                yield return CreateCase("'\\n'", "escaped character");
                yield return CreateCase("'\\''", "escaped char delimitor");

                yield return CreateErrorCase("'c", Register, "missing closing char delimiter");
                yield return CreateErrorCase2("'\\'", Unknown, 1, Unknown, 1, "missing char delimitor while having escaped delimitor");
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
                            CreateEolToken(text)
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
                            CreateEolToken(errorLabel)
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
                            CreateEolToken(errorLabel)
                        })
                        .SetName("{m}_" + testName);
                }

                yield return CreateCase("\"\"", "empty string");
                yield return CreateCase("\"\\n\"", "escaped character string");
                yield return CreateCase("\".\\path\\file.ext\"", "path");

                yield return CreateErrorCase("\"c", Register, "missing closing string delimiter");
                yield return CreateErrorCase2("\"\\\"", Unknown, 1, Unknown, 1, "missing closing string delimiter while having escaped delimiter");
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
    }
}