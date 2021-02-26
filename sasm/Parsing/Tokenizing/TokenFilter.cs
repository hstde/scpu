using System.Collections.Generic;
using System.Linq;

namespace Sasm.Parsing.Tokenizing
{
    public class TokenFilter
    {
        public IEnumerable<Token> Filter(IEnumerable<Token> tokens)
        {
            return tokens.Where(t => t.TokenType != TokenType.Comment);
        }
    }
}