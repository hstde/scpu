namespace Sasm.CodeGeneration
{
    public struct FixupData
    {
        public Label label;
        public int position;
        public int numberOfBytesToFix;
    }
}