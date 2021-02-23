namespace Sasm.Tokenizing
{
    public enum TokenType
    {
            Unknown,
            Mnemonic,
            LabelTerminator,
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
            Name,
            Separator,
    }
}