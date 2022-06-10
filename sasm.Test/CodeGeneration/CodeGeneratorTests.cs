namespace Sasm.Test.CodeGeneration
{
    using System;
    using NUnit.Framework;
    using Sasm.CodeGeneration;

    public class CodeGeneratorTests
    {
        [Test]
        public void CalculatesAddressesCorrectly_TwoByteArgument_BackwardJump()
        {
            var op = OpCodes.J_IMM16;
            ushort expectedAddress = 10;
            byte expectedAddressHigh = (byte)(expectedAddress >> 8);
            byte expectedAddressLow = (byte)expectedAddress;

            var expectedBytes = new byte[] {
                // 10 bytes of padding
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                op.opcode, expectedAddressHigh, expectedAddressLow
            };

            var codeGen = new CodeGenerator();
            
            for(int i = 0; i < 10; i++)
                codeGen.Emit((byte)0);
            // put a label at address 10
            var startLabel = codeGen.DefineLabel();
            codeGen.MarkLabel(startLabel);
            codeGen.Emit(op, startLabel);

            var actualBytes = codeGen.GetBytes();

            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }

        [Test]
        public void CalculatesAddressesCorrectly_TwoByteArgument_ForwardJump()
        {
            var op = OpCodes.J_IMM16;
            ushort expectedAddress = 13;
            byte expectedAddressHigh = (byte)(expectedAddress >> 8);
            byte expectedAddressLow = (byte)expectedAddress;

            var expectedBytes = new byte[] {
                // 10 bytes of padding
                op.opcode, expectedAddressHigh, expectedAddressLow,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };

            var codeGen = new CodeGenerator();
            var startLabel = codeGen.DefineLabel();
            codeGen.Emit(op, startLabel);
            for (int i = 0; i < 10; i++)
                codeGen.Emit((byte)0);
            // put a label at address 10
            codeGen.MarkLabel(startLabel);

            var actualBytes = codeGen.GetBytes();

            Assert.That(actualBytes, Is.EqualTo(expectedBytes));
        }
    }
}