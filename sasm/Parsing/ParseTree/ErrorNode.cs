namespace Sasm.Parsing.ParseTree
{
    using System;

    public class ErrorNode : ParseTreeNode
    {
        public override bool HasErrors => true;
        public string ErrorMessage { get; }

        public ErrorNode(SourceReference sourceReference, string message) 
            : base(sourceReference, null)
        {
            ErrorMessage = message;
        }

        public override string ToString()
        {
            return $"Error @ {sourceReference.lineNumber}:{sourceReference.start} " + ErrorMessage;
        }
    }
}