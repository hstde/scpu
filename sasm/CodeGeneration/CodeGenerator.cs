namespace Sasm.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CodeGenerator
    {
        private List<int> labelLocations;
        private List<FixupData> fixupDatas;
        private List<byte> codeStream;

        private int CurrentCodeAddress => codeStream.Count;

        // methods for generating the code
        // this code is not final, but just a proof of concep
        // we probably want to have the section information here
        // and linking has to be done externally
        // but for now it will suffice
        public CodeGenerator()
        {
            codeStream = new List<byte>();
        }

        public byte[] GetBytes()
        {
            if (codeStream.Count == 0)
                return null;
            
            var bytes = new byte[codeStream.Count];

            codeStream.CopyTo(0, bytes, 0, bytes.Length);

            FixupByteStream(ref bytes);

            return bytes;
        }

        private void FixupByteStream(ref byte[] bytes)
        {
            // this is the linking process, do we want to do that here? no
            foreach(var fixup in fixupDatas)
            {
                int absoluteAddress = GetLabelPosition(fixup.label);
                int relativeAddress = absoluteAddress - (fixup.position + fixup.numberOfBytesToFix);

                if(fixup.numberOfBytesToFix == 1)
                {
                    // put in the relative address
                    if(relativeAddress < SByte.MinValue || relativeAddress > SByte.MaxValue)
                        throw new NotSupportedException("relative address does not fit into sbyte");
                    
                    bytes[fixup.position] = (byte)relativeAddress;
                }
                else if(fixup.numberOfBytesToFix == 2)
                {
                    // treat this as the absolute address
                    bytes[fixup.position] = (byte)(absoluteAddress >> 8);
                    bytes[fixup.position + 1] = (byte)absoluteAddress;
                }
                else
                    throw new NotSupportedException("Unexpected number of bytes to fix");
            }
        }

        private int GetLabelPosition(Label lbl)
        {
            if (lbl.labelIndex < 0 || lbl.labelIndex >= labelLocations.Count)
                throw new ArgumentException("Invalid label!");
            if (labelLocations[lbl.labelIndex] < 0)
                throw new ArgumentException("Undefined label!");

            return labelLocations[lbl.labelIndex];
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
            if (l.labelIndex < 0 || l.labelIndex >= labelLocations.Count)
                throw new ArgumentException($"Invalid label!");
            if (labelLocations[l.labelIndex] != -1)
                throw new ArgumentException("Redefined label!");

            labelLocations[l.labelIndex] = CurrentCodeAddress;
        }

        public void Emit(OpCode op)
        {
            InternalEmit(op);
        }

        public void Emit(OpCode op, Label label)
        {
            InternalEmit(op);
            if (OpCodes.TakesSingleByteArgument(op))
            {
                AddFixup(label, CurrentCodeAddress, 1);
                // dummy byte will get replaced
                InternalEmit((byte)0);
            }
            else if (OpCodes.TakesTwoByteArgument(op))
            {
                AddFixup(label, CurrentCodeAddress, 2);
                // dummy for 16 bit constant
                InternalEmit((ushort)0);
            }
            else
                throw new ArgumentException("Unexpected length of opcode " + op);
        }

        public void Emit(OpCode op, byte arg)
        {
            InternalEmit(op);
            InternalEmit(arg);
        }

        public void Emit(OpCode op, ushort arg)
        {
            InternalEmit(op);
            InternalEmit(arg);
        }

        public void Emit(byte data)
        {
            InternalEmit(data);
        }

        public void Emit(ushort data)
        {
            InternalEmit(data);
        }

        private void InternalEmit(OpCode op)
        {
            if (op.size != 1)
                InternalEmit(op.prefix);

            InternalEmit(op.opcode);
        }

        private void InternalEmit(byte b)
        {
            codeStream.Add(b);
        }

        private void InternalEmit(ushort s)
        {
            InternalEmit((byte)(s >> 8));
            InternalEmit((byte)s);
        }

        private void AddFixup(Label lbl, int pos, int size)
        {
            if (fixupDatas is null)
                fixupDatas = new List<FixupData>();

            fixupDatas.Add(new FixupData
            {
                label = lbl,
                position = pos,
                numberOfBytesToFix = size
            });
        }
    }
}