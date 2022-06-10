namespace Sasm.Ast
{
    using Irony.Ast;
    using Irony.Parsing;

    public class LabledInstructionNode : AstNode
    {
        public override void Assemble(AssemblyContext context)
        {
            throw new System.NotImplementedException();
        }

        public override object Evaluate(EvaluationContext context)
        {
            throw new System.NotImplementedException();
        }

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            foreach(var child in parseNode.GetMappedChildNodes())
                AddChild(child);
        }
    }
}