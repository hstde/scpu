namespace Sasm.Ast
{
    using Irony.Ast;
    using Irony.Parsing;

    public class LabelDefinitionNode : AstNode
    {
        public IdentNode Label { get; private set; }

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
            var children = parseNode.GetMappedChildNodes();
            Label = AddChild(children[0]) as IdentNode;
            if(Label is null)
                throw new System.Exception("Expected child to be an ident!");
        }
    }
}