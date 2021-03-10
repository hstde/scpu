namespace Sasm.Ast
{
    using Irony.Parsing;

    public class AstContext : Irony.Ast.AstContext
    {
        public string FileName { get; }

        public AstContext(string filename, LanguageData language) : base(language)
        {
            FileName = filename;
            DefaultNodeType = typeof(AstNode);
            DefaultIdentifierNodeType = typeof(IdentNode);
            DefaultLiteralNodeType = typeof(LiteralNode);
        }
    }
}