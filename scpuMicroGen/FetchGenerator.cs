namespace ScpuMicroGen
{
    using Sasm.CodeGeneration;
    using static ScpuMicroGen.ControlLines;
    using static ScpuMicroGen.ControlLines2;

    public static class FetchGenerator
    {
        public static (ControlLines[], ControlLines2[]) Generate(Flags flags, byte op)
        {
            // flags will always be 0, so can be ignored
            // the opcode is a bit different here:
            // 0 normal fetch; 1 interrupt; 3 reset
            if (op == 0)
            {
                // normal fetch
                return (new ControlLines[]
                    {
                        // PC -> ABuf, PC -> ACBA
                        (Reg2LBusSelPCl | Reg2HBusSelPCh | ABufLoad | ACBALoad),
                        // ACBA + 1 -> HLBus, HLBus -> PC
                        // D -> IR
                        // we will also load the ACBA + 1 into Abuf & ACAB
                        // so instructions don't need to do this
                        // and we'll save some cycles
                        (ACOpInc | ACRes2HLBus | HBus2RegSelPCh | LBus2RegSelPCl
                            | ACRes2ACB | ACBALoad | ABufLoad
                            | BusEnablePin | Write_ReadPinRead | DBuf2DBus
                            | TogglePhase),

                    },
                    new ControlLines2[]
                    {
                        SendSync,
                        ControlLines2.None,
                    });
            }
            else if (op == 1)
            {
                // interrupt
                return (new ControlLines[]
                    {
                        ControlLines.None
                    },
                    new ControlLines2[]
                    {
                        ControlLines2.None,
                    });
            }
            else if (op == 2)
            {
                // reset
                return (new ControlLines[]
                    {
                        // 0 -> PC
                        (HBus2RegSelPCh | Imm2DBus | DBus2HBus).SetImmediate(0),
                        (LBus2RegSelPCl | Imm2DBus | DBus2LBus).SetImmediate(0),
                        // Execute a NOP to get into the normal fetch phase
                        (Imm2DBus | TogglePhase).SetImmediate((byte)OpCodeValues.NOP),
                    },
                    new ControlLines2[]
                    {
                        ControlLines2.None,
                        ControlLines2.None,
                        ControlLines2.None,
                    });
            }
            else
            {
                return (new ControlLines[0], new ControlLines2[0]);
            }
        }
    }
}