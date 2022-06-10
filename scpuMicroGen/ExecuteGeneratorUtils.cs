namespace ScpuMicroGen
{
    using System;
    using static ScpuMicroGen.ControlLines;
    using static ScpuMicroGen.ControlLines2;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg8;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg16;

    public static partial class ExecuteGenerator
    {
        static ControlLines ReadReg16(HLBusReg16 reg) => ReadHReg16(reg) | ReadLReg16(reg);

        static ControlLines ReadHReg16(HLBusReg16 reg)
        {
            switch (reg)
            {
                case WZ:
                    return ReadReg8(W);
                case BC:
                    return ReadReg8(B);
                case DE:
                    return ReadReg8(D);
                case HL:
                    return ReadReg8(H);
                case SP:
                    return ReadReg8(SPh);
                case PC:
                    return ReadReg8(PCh);
                case LR:
                    return ReadReg8(LRh);
                case IX:
                    return ReadReg8(IXh);
                case IY:
                    return ReadReg8(IYh);
                case IV:
                    return ReadReg8(IVh);
                default:
                    throw new ArgumentException(reg.ToString());
            }
        }

        static ControlLines ReadLReg16(HLBusReg16 reg)
        {
            switch (reg)
            {
                case WZ:
                    return ReadReg8(Z);
                case BC:
                    return ReadReg8(C);
                case DE:
                    return ReadReg8(E);
                case HL:
                    return ReadReg8(L);
                case SP:
                    return ReadReg8(SPl);
                case PC:
                    return ReadReg8(PCl);
                case LR:
                    return ReadReg8(LRl);
                case IX:
                    return ReadReg8(IXl);
                case IY:
                    return ReadReg8(IYl);
                case IV:
                    return ReadReg8(IVl);
                default:
                    throw new ArgumentException(reg.ToString());
            }
        }

        static ControlLines ReadReg8(HLBusReg8 reg)
        {
            switch (reg)
            {
                case W:
                    return Reg2HBusSelW;
                case B:
                    return Reg2HBusSelB;
                case D:
                    return Reg2HBusSelD;
                case H:
                    return Reg2HBusSelH;
                case SPh:
                    return Reg2HBusSelSPh;
                case PCh:
                    return Reg2HBusSelPCh;
                case LRh:
                    return Reg2HBusSelLRh;
                case IXh:
                    return Reg2HBusSelIXh;
                case IYh:
                    return Reg2HBusSelIYh;
                case IVh:
                    return Reg2HBusSelIVh;
                case Z:
                    return Reg2LBusSelZ;
                case C:
                    return Reg2LBusSelC;
                case E:
                    return Reg2LBusSelE;
                case L:
                    return Reg2LBusSelL;
                case SPl:
                    return Reg2LBusSelSPl;
                case PCl:
                    return Reg2LBusSelPCl;
                case LRl:
                    return Reg2LBusSelLRl;
                case IXl:
                    return Reg2LBusSelIXl;
                case IYl:
                    return Reg2LBusSelIYl;
                case IVl:
                    return Reg2LBusSelIVl;
                default:
                    throw new ArgumentException(reg.ToString());
            }
        }

        static ControlLines Reg82DBus(HLBusReg8 reg)
        {
            switch (reg)
            {
                case HLBusReg8.W:
                case HLBusReg8.B:
                case HLBusReg8.D:
                case HLBusReg8.H:
                case HLBusReg8.SPh:
                case HLBusReg8.PCh:
                case HLBusReg8.LRh:
                    return HBus2DBus | ReadReg8(reg);
                case HLBusReg8.Z:
                case HLBusReg8.C:
                case HLBusReg8.E:
                case HLBusReg8.L:
                case HLBusReg8.SPl:
                case HLBusReg8.PCl:
                case HLBusReg8.LRl:
                    return LBus2DBus | ReadReg8(reg);
                default:
                    throw new ArgumentException(reg.ToString());
            }
        }

        static ControlLines WriteReg16(HLBusReg16 reg) => WriteHReg16(reg) | WriteLReg16(reg);

        static ControlLines WriteHReg16(HLBusReg16 reg)
        {
            switch (reg)
            {
                case WZ:
                    return WriteReg8(W);
                case BC:
                    return WriteReg8(B);
                case DE:
                    return WriteReg8(D);
                case HL:
                    return WriteReg8(H);
                case SP:
                    return WriteReg8(SPh);
                case PC:
                    return WriteReg8(PCh);
                case LR:
                    return WriteReg8(LRh);
                case IX:
                    return WriteReg8(IXh);
                case IY:
                    return WriteReg8(IYh);
                case IV:
                    return WriteReg8(IVh);
                default:
                    throw new ArgumentException(reg.ToString());
            }
        }

        static ControlLines WriteLReg16(HLBusReg16 reg)
        {
            switch (reg)
            {
                case WZ:
                    return WriteReg8(Z);
                case BC:
                    return WriteReg8(C);
                case DE:
                    return WriteReg8(E);
                case HL:
                    return WriteReg8(L);
                case SP:
                    return WriteReg8(SPl);
                case PC:
                    return WriteReg8(PCl);
                case LR:
                    return WriteReg8(LRl);
                case IX:
                    return WriteReg8(IXl);
                case IY:
                    return WriteReg8(IYl);
                case IV:
                    return WriteReg8(IVl);
                default:
                    throw new ArgumentException(reg.ToString());
            }
        }

        static ControlLines WriteReg8(HLBusReg8 reg)
        {
            switch (reg)
            {
                case W:
                    return HBus2RegSelW;
                case B:
                    return HBus2RegSelB;
                case D:
                    return HBus2RegSelD;
                case H:
                    return HBus2RegSelH;
                case SPh:
                    return HBus2RegSelSPh;
                case PCh:
                    return HBus2RegSelPCh;
                case LRh:
                    return HBus2RegSelLRh;
                case IXh:
                    return HBus2RegSelIXh;
                case IYh:
                    return HBus2RegSelIYh;
                case IVh:
                    return HBus2RegSelIVh;
                case Z:
                    return LBus2RegSelZ;
                case C:
                    return LBus2RegSelC;
                case E:
                    return LBus2RegSelE;
                case L:
                    return LBus2RegSelL;
                case SPl:
                    return LBus2RegSelSPl;
                case PCl:
                    return LBus2RegSelPCl;
                case LRl:
                    return LBus2RegSelLRl;
                case IXl:
                    return LBus2RegSelIXl;
                case IYl:
                    return LBus2RegSelIYl;
                case IVl:
                    return LBus2RegSelIVl;
                default:
                    throw new ArgumentException(reg.ToString());
            }
        }

        static ControlLines DBus2Reg8(HLBusReg8 reg)
        {
            switch (reg)
            {
                case HLBusReg8.W:
                case HLBusReg8.B:
                case HLBusReg8.D:
                case HLBusReg8.H:
                case HLBusReg8.SPh:
                case HLBusReg8.PCh:
                case HLBusReg8.LRh:
                    return DBus2HBus | WriteReg8(reg);
                case HLBusReg8.Z:
                case HLBusReg8.C:
                case HLBusReg8.E:
                case HLBusReg8.L:
                case HLBusReg8.SPl:
                case HLBusReg8.PCl:
                case HLBusReg8.LRl:
                    return DBus2LBus | WriteReg8(reg);
                default:
                    throw new ArgumentException(reg.ToString());
            }
        }
    }
}