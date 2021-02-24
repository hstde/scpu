namespace Sasm.Tokenizing
{
    public enum TokenType
    {
            Unknown,
            Mnemonic,
            LabelDefinition,
            String,
            Char,
            DecNumber,
            HexNumber,
            BinNumber,
            OctNumber,
            Register,
            Comment,
            Command,
            LParen,
            RParen,
            Operator,
            Identifier,
            Separator,
            EndOfLine,
    }
}