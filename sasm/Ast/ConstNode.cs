namespace Sasm.Ast
{
    using Irony.Ast;
    using Irony.Parsing;

    public class ConstNode : AstNode
    {
        public IdentNode Target { get; private set; }
        public AstNode Value { get; private set; }

        public override void Assemble(AssemblyContext context)
        {
            throw new System.NotImplementedException();
        }

        public override object Evaluate(EvaluationContext context)
        {
            var value = Value.Evaluate(context);
            Target.SetValue(context, value);
            return value;
        }

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            var children = parseNode.GetMappedChildNodes();
            Target = AddChild(children[1]) as IdentNode;
            Value = AddChild(children[2]);
        }
    }
}