namespace Sasm.Parsing
{
    public struct SourceReference
    {
        public readonly string filename;
        public readonly string line;
        public readonly int lineNumber;
        public readonly int start;
        public readonly int length;
    }
}