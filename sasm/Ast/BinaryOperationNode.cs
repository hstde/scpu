namespace Sasm.Ast
{
    using Irony.Ast;
    using Irony.Parsing;

    public class BinaryOperationNode : AstNode
    {
        public AstNode Lhs { get; private set; }
        public AstNode Rhs { get; private set; }
        public string Operation { get; private set; }

        public override void Assemble(AssemblyContext context)
        {
            throw new System.NotImplementedException();
        }

        public override object Evaluate(EvaluationContext context)
        {
            return null;
        }

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            var children = parseNode.GetMappedChildNodes();
            Lhs = AddChild(children[0]);
            Rhs = AddChild(children[2]);
            Operation = children[1].FindTokenAndGetText();
        }
    }
}