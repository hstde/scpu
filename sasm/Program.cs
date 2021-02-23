using System;

namespace Sasm
{
    class Program
    {
        struct Argument
        {
            public AddressMode mode;
            public ArgumentType type;
            public int? immediateData;
            public string? label;
        }

        struct ProgramLine
        {
            public string? label;
            public Mnemonic? mnemonic;
            public Argument? arg1;
            public Argument? arg2;
            
        }

        static void Main(string[] args)
        {
            TokenizeLine("label: ld h, (ix+127)");
        }

        static ProgramLine TokenizeLine(string line)
        {
            return new ProgramLine
            {
                label = "label",
                mnemonic = Mnemonic.LD,
                arg1 = new Argument
                {
                    mode = AddressMode.Implied,
                    type = ArgumentType.RegH
                },
                arg2 = new Argument
                {
                    mode = AddressMode.Displacement,
                    type = ArgumentType.RegIX,
                    immediateData = 127
                }
            };
        }
    }
}
