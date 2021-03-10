namespace Sasm.Ast
{
    using Irony.Parsing;

    public class LiteralNode : AstNode
    {
        public object Value { get; private set; }

        public override object Evaluate(EvaluationContext context)
        {
            return Value;
        }

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            Value = parseNode.Token.Value;
        }
    }
}