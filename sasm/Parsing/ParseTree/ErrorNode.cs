namespace Sasm.Parsing.ParseTree
{
    using System;
    using System.Collections.Generic;
    using Sasm.Tokenizing;

    public class ErrorNode : ParseTreeNode
    {
        public override bool HasErrors => true;
        public string ErrorMessage { get; }

        public ErrorNode(string message, SourceReference sourceReference) 
            : base(sourceReference, null)
        {
            ErrorMessage = message;
        }

        public override string ToString()
        {
            return $"Error @ {sourceReference.lineNumber}:{sourceReference.start} " + ErrorMessage;
        }

        public override bool Equals(object obj)
        {
            return obj is ErrorNode node &&
                   EqualityComparer<SourceReference>.Default.Equals(sourceReference, node.sourceReference) &&
                   ErrorMessage == node.ErrorMessage;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(sourceReference, ErrorMessage);
        }
    }
}