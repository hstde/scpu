namespace Sasm.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CodeGenerator
    {
        private List<int> labelLocations;
        private List<byte> codeStream;

        // methods for generating the code
        public CodeGenerator()
        {
            codeStream = new List<byte>();
        }

        public byte[] GetBytes()
        {
            return codeStream.ToArray();
        }

        public Label DefineLabel()
        {
            if (labelLocations is null)
                labelLocations = new List<int>();

            labelLocations.Add(-1);

            return new Label(labelLocations.Count - 1);
        }

        public void MarkLabel(Label l)
        {
            if(l.labelIndex <= 0 || l.labelIndex >= labelLocations.Count)
                throw new ArgumentException("Invalid label!");
            if(labelLocations[l.labelIndex] != -1)
                throw new ArgumentException("Redefined label!");
            
            labelLocations[l.labelIndex] = codeStream.Count;
        }

        public void Emit(OpCode op)
        {
            if(!OpCodes.TakesNoArgument(op))
                throw new ArgumentException("No argument provided for opcode " + op);
            
            if(op.size != 1)
                codeStream.Add(op.prefix);

            codeStream.Add(op.opcode);
        }

        public void Emit(OpCode op, Label l)
        {

        }

        public void Emit(OpCode op, byte arg)
        {

        }

        public void Emit(OpCode op, short arg)
        {
            
        }
    }
}