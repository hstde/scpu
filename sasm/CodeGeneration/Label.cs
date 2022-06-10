using System;

namespace Sasm.CodeGeneration
{
    public struct Label
    {
        public readonly int labelIndex;

        public Label(int labelIndex)
        {
            this.labelIndex = labelIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is Label label &&
                   labelIndex == label.labelIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(labelIndex);
        }
    }
}