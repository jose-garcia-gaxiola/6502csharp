﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Components
{
    public partial class Olc6502
    {

        #region Memory ops

        internal byte Imp()
        {
            fetched = a;
            return 0;
        }

        internal byte Imm()
        {
            addrAbs = (ushort)pc++;

            return 0;
        }

        internal byte Zp0()
        {
            addrAbs = Read(pc);
            pc++;
            addrAbs &= 0x00FF;
            return 0;
        }

        internal byte Zpx()
        {
            addrAbs = (ushort)(Read(pc) + x);
            pc++;
            addrAbs &= 0x00FF;
            return 0;
        }

        internal byte Zpy()
        {
            addrAbs = (ushort)(Read(pc) + y);
            pc++;
            addrAbs &= 0x00FF;
            return 0;
        }

        byte Rel()
        {
            addrRel = Read(pc);
            pc++;
            if ((addrRel & 0x80) != 0)
                addrRel |= 0xFF00;
            return 0;
        }

        internal byte Abs()
        {
            byte lo = Read(pc);
            pc++;
            byte hi = Read(pc);
            pc++;

            addrAbs = (ushort)((hi << 8) | lo);

            return 0;
        }

        internal byte Abx()
        {
            byte lo = Read(pc);
            pc++;
            byte hi = Read(pc);
            pc++;

            addrAbs = (ushort)((hi << 8) | lo);
            addrAbs = (ushort)((addrAbs + x) & 0xFFFF);

            if ((addrAbs & 0xFF00) != (hi << 8))
                return 1;

            return 0;
        }

        internal byte Aby()
        {
            byte lo = Read(pc);
            pc++;
            byte hi = Read(pc);
            pc++;

            addrAbs = (ushort)((hi << 8) | lo);
            addrAbs = (ushort)((addrAbs + y) & 0xFFFF);

            if ((addrAbs & 0xFF00) != (hi << 8))
                return 1;

            return 0;
        }

        internal byte Ind()
        {
            byte ptrLo = Read(pc);
            pc++;
            byte ptrHi = Read(pc);
            pc++;

            ushort ptr = (ushort)((ptrHi << 8) | ptrLo);

            if (ptrLo == 0x00FF)
                addrAbs = (ushort)((Read((ptr & 0xFF00)) << 8) | Read(ptr));
            else
                addrAbs = (ushort)((Read((ptr + 0x01)) << 8) | Read(ptr));

            return 0;
        }

        internal byte Izx()
        {
            ushort t = Read(pc);
            pc++;

            ushort lo = Read(((t + x) & 0x00FF));
            ushort hi = Read(((t + x + 1) & 0x00FF));

            addrAbs = (ushort)((hi << 8) | lo);

            return 0;
        }

        internal byte Izy()
        {
            ushort t = Read(pc);
            pc++;

            ushort lo = Read(t & 0x00FF);
            ushort hi = Read((t + 1) & 0x00FF);

            addrAbs = (ushort)((hi << 8) | lo);
            addrAbs = (ushort)((addrAbs + y) & 0xFFFF);

            if ((addrAbs & 0xff00) != (hi << 8))
                return 1;

            return 0;
        }

        #endregion

        #region Instructions

        internal byte fetch()
        {
            if (!(lookup[opcode].addrMode == Imp))
                fetched = Read(addrAbs);
            return fetched;
        }

        internal byte Adc()
        {
            fetch();
            uint temp = (uint)(a + fetched + GetFlag(Flags6502.C));
            SetFlag(Flags6502.C, temp > 255);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0);
            SetFlag(Flags6502.V, (((~(uint)a ^ (uint)fetched) & ((uint)a ^ (uint)temp)) & 0x0080) != 0);
            SetFlag(Flags6502.N, (temp & 0x80) != 0);            

            a = (byte)(temp & 0xFF);
            return 1;
        }

        internal byte Sbc()
        {
            fetch();
            ushort value = (ushort)(fetched ^ 0x00FFu);

            ushort temp = Convert.ToUInt16(a + value + GetFlag(Flags6502.C));
            SetFlag(Flags6502.C, (temp & 0xFF00) != 0);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0);
            SetFlag(Flags6502.V, ((temp ^ a) & (temp ^ value) & 0x0080) != 0);
            SetFlag(Flags6502.N, (temp & 0x0080) != 0);

            a = (byte)(temp & 0x00ff);
            return 1;
        }

        internal byte And()
        {
            fetch();
            a = (byte)(a & fetched);
            SetFlag(Flags6502.Z, a == 0x00);
            SetFlag(Flags6502.N, (a & 0x80) != 0);

            return 1;
        }

        internal byte Asl()
        {
            fetch();
            uint temp = (uint)fetched << 1;
            SetFlag(Flags6502.C, (temp & 0xFF00) > 0);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetFlag(Flags6502.N, (temp & 0x80) != 0);
            if (lookup[opcode].addrMode == Imp)
                a = (byte)(temp & 0x00FF);
            else
                Write(addrAbs, (byte)(temp & 0x00FF));
            return 0;
        }

        internal byte Bcc()
        {
            if (GetFlag(Flags6502.C) == 0x00)
            {
                cycles++;
                addrAbs = (ushort)(pc + addrRel);

                if ((addrAbs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addrAbs;
            }

            return 0;
        }

        internal byte Bcs()
        {
            if (GetFlag(Flags6502.C) == 0x01)
            {
                cycles++;
                addrAbs = (ushort)((pc + addrRel) & 0xFFFF);

                if ((addrAbs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addrAbs;
            }

            return 0;
        }

        internal byte Beq()
        {
            if (GetFlag(Flags6502.Z) == (byte)Flags6502.Z)
            {
                cycles++;
                addrAbs = (ushort)((pc + addrRel) & 0xFFFF);

                if ((addrAbs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addrAbs;
            }
            return 0;
        }

        internal byte Bit()
        {
            fetch();
            temp = Convert.ToUInt16(a & fetched);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetFlag(Flags6502.N, (fetched & 0x80) != 0);
            SetFlag(Flags6502.V, (fetched & (1 << 6)) != 0);
            return 0;
        }

        internal byte Bmi()
        {
            if (GetFlag(Flags6502.N) == (byte)Flags6502.N)
            {
                cycles++;
                addrAbs = (ushort)((pc + addrRel) & 0xFFFF);

                if ((addrAbs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addrAbs;
            }

            return 0;
        }

        internal byte Bne()
        {
            if (GetFlag(Flags6502.Z) == 0x00)
            {
                cycles++;
                addrAbs = (ushort)((pc + addrRel) & 0xFFFF);

                if ((addrAbs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addrAbs;
            }

            return 0;
        }

        internal byte Bpl()
        {
            if (GetFlag(Flags6502.N) == 0x00)
            {
                cycles++;
                addrAbs = (ushort)((pc + addrRel) & 0xFFFF);

                if ((addrAbs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addrAbs;
            }

            return 0;
        }

        internal byte Brk()
        {
            pc++;

            SetFlag(Flags6502.I, true);
            Write(0x0100 + stkp, (byte)((pc >> 8) & 0x00FF));
            stkp--;
            Write(0x0100 + stkp, (byte)(pc & 0x00FF));
            stkp--;

            SetFlag(Flags6502.B, true);
            Write(0x0100 + stkp, status);
            stkp--;
            SetFlag(Flags6502.B, false);

            pc = (ushort)(Read(0xFFFE) | (Read(0xFFFF) << 8));
            return 0;
        }

        internal byte Bvc()
        {
            if (GetFlag(Flags6502.V) == 0x00)
            {
                cycles++;
                addrAbs = (ushort)((pc + addrRel) & 0xFFFF);

                if ((addrAbs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addrAbs;
            }

            return 0;
        }

        internal byte Bvs()
        {
            if (GetFlag(Flags6502.V) == (byte)Flags6502.V)
            {
                cycles++;
                addrAbs = (ushort)((pc + addrRel) & 0xFFFF);

                if ((addrAbs & 0xFF00) != (pc & 0xFF00))
                    cycles++;

                pc = addrAbs;
            }

            return 0;
        }

        internal byte Clc()
        {
            SetFlag(Flags6502.C, false);
            return 0;
        }

        internal byte Cld()
        {
            SetFlag(Flags6502.D, false);
            return 0;
        }

        internal byte Cli()
        {
            SetFlag(Flags6502.I, false);
            return 0;
        }

        internal byte Clv()
        {
            SetFlag(Flags6502.V, false);
            return 0;
        }

        internal byte Cmp()
        {
            fetch();
            ushort temp =Convert.ToUInt16((a - fetched) & 0xFFFF);
            SetFlag(Flags6502.C, a >= fetched);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flags6502.N, (temp & 0x0080) != 0);
            return 1;
        }

        internal byte Cpx()
        {
            fetch();
            ushort temp = Convert.ToUInt16((x - fetched) & 0xFFFF);
            SetFlag(Flags6502.C, x >= fetched);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flags6502.N, (temp & 0x0080) != 0);
            return 0;
        }

        internal byte Cpy()
        {
            fetch();
            ushort temp = Convert.ToUInt16((y - fetched) & 0xFFFF);
            SetFlag(Flags6502.C, y >= fetched);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flags6502.N, (temp & 0x0080) != 0);
            return 0;
        }

        internal byte Dec()
        {
            fetch();
            temp = (ushort)(fetched - 1);
            Write(addrAbs, (byte)(temp & 0x00FF));
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flags6502.N, (temp & 0x0080) != 0);
            return 0;
        }

        internal byte Dex()
        {
            x--;
            SetFlag(Flags6502.Z, x == 0x00);
            SetFlag(Flags6502.N, (x & 0x80) != 0);
            return 0;
        }

        internal byte Dey()
        {
            y--;
            SetFlag(Flags6502.Z, y == 0x00);
            SetFlag(Flags6502.N, (y & 0x80) != 0);
            return 0;
        }

        internal byte Eor()
        {
            fetch();
            a = Convert.ToByte(a ^ fetched);
            SetFlag(Flags6502.Z, a == 0x00);
            SetFlag(Flags6502.N, (a & 0x80) != 0);
            return 1;
        }

        internal byte Inc()
        {
            fetch();
            temp = Convert.ToUInt16((ushort)fetched + 1);
            Write(addrAbs, Convert.ToByte(temp & 0x00FF));
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flags6502.N, (temp & 0x0080) != 0);
            return 0;
        }

        internal byte Pha()
        {
            Write(0x0100 + stkp, a);
            stkp--;
            return 0;
        }

        internal byte Pla()
        {
            stkp++;
            a = Read(0x0100 + stkp);
            SetFlag(Flags6502.Z, a == 0x00);
            SetFlag(Flags6502.N, (a & 0x80) != 0);

            return 0;
        }

        internal byte Rti()
        {
            stkp++;
            status = Read(0x0100 + stkp);
            status = Convert.ToByte(status & ~Convert.ToByte(Flags6502.B));
            status = Convert.ToByte(status & ~Convert.ToByte(Flags6502.U));

            stkp++;
            pc = Read(0x0100 + stkp);
            stkp++;
            pc |= (ushort)(Read(0x0100 + stkp) << 8);

            return 0;
        }

        internal byte Iny()
        {
            y++;
            SetFlag(Flags6502.Z, y == 0x00);
            SetFlag(Flags6502.N, (y & 0x80) != 0);
            return 0;
        }

        internal byte Inx()
        {
            x++;
            SetFlag(Flags6502.Z, x == 0x00);
            SetFlag(Flags6502.N, (x & 0x80) != 0);
            return 0;
        }

        internal byte Lda()
        {
            fetch();
            a = fetched;
            SetFlag(Flags6502.Z, a == 0x00);
            SetFlag(Flags6502.N, (a & 0x80) != 0);
            return 1;
        }

        internal byte Ldy()
        {
            fetch();
            y = fetched;
            SetFlag(Flags6502.Z, y == 0x00);
            SetFlag(Flags6502.N, (y & 0x80) != 0);
            return 1;
        }

        internal byte Ldx()
        {
            fetch();
            x = fetched;
            SetFlag(Flags6502.Z, x == 0x00);
            SetFlag(Flags6502.N, (x & 0x80) != 0);
            return 1;
        }

        internal byte Nop()
        {
            // Sadly not all NOPs are equal, Ive added a few here
            // based on https://wiki.nesdev.com/w/index.php/CPU_unofficial_opcodes
            // and will add more based on game compatibility, and ultimately
            // I'd like to cover all illegal opcodes too
            switch (opcode)
            {
                case 0x1C:
                case 0x3C:
                case 0x5C:
                case 0x7C:
                case 0xDC:
                case 0xFC:
                    return 1;
            }
            return 0;
        }       

        internal byte Php()
        {
            Write(0x0100 + stkp, Convert.ToByte(status | (byte)Flags6502.B | (byte)Flags6502.U));
            SetFlag(Flags6502.B, false);
            SetFlag(Flags6502.U, false);
            stkp--;
            return 0;
        }

        internal byte Jsr()
        {
            pc--;

            Write(0x0100 + stkp, Convert.ToByte((pc >> 8) & 0x00FF));
            stkp--;
            Write(0x0100 + stkp, Convert.ToByte(pc & 0x00FF));
            stkp--;

            pc = addrAbs;
            return 0;
        }

        internal byte Rol()
        {
            fetch();
            temp = Convert.ToUInt16(((ushort)fetched << 1) | GetFlag(Flags6502.C));
            SetFlag(Flags6502.C, (temp & 0xFF00) != 0);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flags6502.N, (temp & 0x0080) != 0);
            if (lookup[opcode].addrMode == Imp)
                a = Convert.ToByte(temp & 0x00FF);
            else
                Write(addrAbs, Convert.ToByte(temp & 0x00FF));
            return 0;
        }

        internal byte Plp()
        {
            stkp++;
            status = Read(0x0100 + stkp);
            SetFlag(Flags6502.U, true);
            return 0;
        }

        internal byte Ora()
        {
            fetch();
            a = Convert.ToByte(a | fetched);
            SetFlag(Flags6502.Z, a == 0x00);
            SetFlag(Flags6502.N, (a & 0x80) != 0);
            return 1;
        }

        internal byte Sec()
        {
            SetFlag(Flags6502.C, true);
            return 0;
        }

        internal byte Lsr()
        {
            fetch();
            SetFlag(Flags6502.C, (fetched & 0x0001) != 0);
            temp = Convert.ToUInt16((ushort)fetched >> 1);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x0000);
            SetFlag(Flags6502.N, (temp & 0x0080) != 0);
            if (lookup[opcode].addrMode == Imp)
                a = Convert.ToByte(temp & 0x00FF);
            else
                Write(addrAbs, Convert.ToByte(temp & 0x00FF));
            return 0;
        }

        internal byte Jmp()
        {
            pc = addrAbs;
            return 0;
        }

        internal byte Ror()
        {
            fetch();
            temp = Convert.ToUInt16(((ushort)(GetFlag(Flags6502.C) << 7) | (ushort)(fetched >> 1)));
            SetFlag(Flags6502.C, (fetched & 0x01) != 0);
            SetFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            SetFlag(Flags6502.N, (temp & 0x0080) != 0);
            if (lookup[opcode].addrMode == Imp)
                a = Convert.ToByte(temp & 0x00FF);
            else
                Write(addrAbs, Convert.ToByte(temp & 0x00FF));
            return 0;
        }

        internal byte Rts()
        {
            stkp++;
            pc = Read(0x0100 + stkp);
            stkp++;
            pc |= (ushort)(Read(0x0100 + stkp) << 8);

            pc++;
            return 0;
        }

        internal byte Sei()
        {
            SetFlag(Flags6502.I, true);
            return 0;
        }

        internal byte Sed()
        {
            SetFlag(Flags6502.D, true);
            return 0;
        }

        internal byte Sta()
        {
            Write(addrAbs, a);
            return 0;
        }

        internal byte Stx()
        {
            Write(addrAbs, x);
            return 0;
        }

        internal byte Sty()
        {
            Write(addrAbs, y);
            return 0;
        }

        internal byte Tax()
        {
            x = a;
            SetFlag(Flags6502.Z, x == 0x00);
            SetFlag(Flags6502.N, (x & 0x80) != 0);
            return 0;
        }

        internal byte Tay()
        {
            y = a;
            SetFlag(Flags6502.Z, y == 0x00);
            SetFlag(Flags6502.N, (y & 0x80) != 0);
            return 0;
        }

        internal byte Txa()
        {
            a = x;
            SetFlag(Flags6502.Z, a == 0x00);
            SetFlag(Flags6502.N, (a & 0x80) != 0);
            return 0;
        }

        internal byte Txs()
        {
            stkp = x;
            return 0;
        }

        internal byte Tsx()
        {
            x = stkp;
            SetFlag(Flags6502.Z, x == 0x00);
            SetFlag(Flags6502.N, (x & 0x80) != 0);
            return 0;
        }

        internal byte Tya()
        {
            a = y;
            SetFlag(Flags6502.Z, a == 0x00);
            SetFlag(Flags6502.N, (a & 0x80) != 0);
            return 0;
        }

        internal byte Xxx()
        {
            return 0;
        }

        #endregion
    }
}
