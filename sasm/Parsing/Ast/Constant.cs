namespace Sasm.Parsing.Ast
{
    public class Constant : AstNode
    {
        public readonly int value;

        public Constant(SourceReference reference, int value) : base(reference)
        {
            this.value = value;
        }
    }
}