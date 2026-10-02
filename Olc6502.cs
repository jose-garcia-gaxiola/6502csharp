using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.SymbolStore;
using System.Reflection.Emit;
using System.Text;

namespace Components
{
    public partial class Olc6502
    {
        Bus bus;

        byte a = 0x00;
        byte x = 0x00;
        byte y = 0x00;
        byte stkp = 0x00;
        ushort pc = 0x0000;
        byte status = 0x00;
        private byte fetched = 0x00;
        private ushort addrAbs = 0x0000;
        private ushort addrRel = 0x00;
        private byte opcode = 0x00;
        private byte cycles = 0x00;
        private ushort temp;
        private ulong cycleCount = 0;
        public List<Instruction> lookup;

        //private AddressingMode[] modes;

        //private AddressingMode[] opCodes;

        public Olc6502()
        {
            lookup = new List<Instruction> {
                new Instruction { name = "BRK", operate = Brk, addrMode = Imp , cycles = 7 }, new Instruction { name ="ORA", operate = Ora, addrMode = Izx, cycles = 6 },
                new Instruction { name = "???", operate = Xxx,  addrMode = Imp, cycles = 2 },new Instruction {name =  "???", operate = Xxx, addrMode = Imp, cycles =8 },
                new Instruction { name = "???", operate = Nop, addrMode = Zp0, cycles = 3 }, new Instruction {name = "ORA", operate = Ora, addrMode = Zp0, cycles =3 },
                new Instruction { name = "ASL", operate = Asl, addrMode = Zp0, cycles =5 },new Instruction {name =  "???", operate = Xxx,addrMode = Imp,cycles = 5 },
                new Instruction { name = "PHP", operate = Php, addrMode = Imp, cycles = 3 }, new Instruction{name =  "ORA", operate =Ora, addrMode =  Imm,cycles = 2 },
                new Instruction { name = "ASL", operate =Asl, addrMode = Imp, cycles = 2 },new Instruction {name =  "???", operate = Xxx, addrMode = Imp,cycles = 2 },
                new Instruction { name = "???", operate = Nop, addrMode = Abs, cycles = 4 }, new Instruction {name = "ORA", operate = Ora, addrMode = Abs, cycles = 4 },
                new Instruction { name = "ASL", operate = Asl,  addrMode = Abs, cycles = 6 }, new Instruction {name =  "???", operate =Xxx, addrMode = Imp, cycles =6 },
                new Instruction { name = "BPL", operate = Bpl, addrMode = Rel, cycles = 2 }, new Instruction {name = "ORA", operate = Ora, addrMode = Izy, cycles =5 },
                new Instruction { name = "???", operate = Xxx, addrMode = Imp, cycles = 2 },new Instruction {name = "???", operate =Xxx, addrMode = Imp, cycles =8 },
                new Instruction { name = "???", operate = Nop, addrMode = Zpx, cycles = 4 },new Instruction { name =  "ORA", operate = Ora, addrMode = Zpx, cycles = 4 },
                new Instruction { name = "ASL", operate =Asl, addrMode = Zpx, cycles =6 },new Instruction {name = "???", operate = Xxx, addrMode = Imp, cycles =6 },
                new Instruction { name = "CLC", operate = Clc, addrMode = Imp, cycles =2 }, new Instruction { name = "ORA", operate = Ora, addrMode = Aby,cycles = 4 },
                new Instruction { name = "???", operate = Nop,  addrMode = Imp, cycles = 2 }, new Instruction {name =  "???", operate = Xxx, addrMode = Imp, cycles =7 },
                new Instruction { name = "???", operate = Nop, addrMode = Abx, cycles =4 }, new Instruction {name = "ORA", operate = Ora, addrMode =Abx, cycles =4 },
                new Instruction { name = "ASL", operate = Asl, addrMode = Abx, cycles =7 },new Instruction {name =  "???", operate = Xxx, addrMode = Imp,  cycles = 7 },
                new Instruction { name = "JSR", operate = Jsr, addrMode = Abs, cycles =6 }, new Instruction { name=  "AND", operate = And, addrMode = Izx, cycles = 6 },
                new Instruction { name = "???", operate = Xxx, addrMode = Imp, cycles =2 },new Instruction{name = "???", operate = Xxx, addrMode = Imp, cycles =8 },
                new Instruction { name = "BIT", operate = Bit, addrMode = Zp0, cycles = 3 }, new Instruction {name = "AND",operate = And, addrMode = Zp0, cycles =3 },
                new Instruction { name = "ROL", operate = Rol, addrMode = Zp0, cycles = 5 }, new Instruction { name = "???", operate = Xxx, addrMode =Imp, cycles = 5 },
                new Instruction { name = "PLP", operate  = Plp, addrMode = Imp, cycles = 4 }, new Instruction {name = "AND", operate = And, addrMode = Imm, cycles = 2 },
                new Instruction { name = "ROL", operate =Rol, addrMode = Imp,cycles= 2 }, new Instruction {name = "???", operate = Xxx, addrMode = Imp, cycles =2 },
                new Instruction { name = "BIT", operate = Bit, addrMode = Abs,cycles= 4 },new Instruction { name = "AND", operate =And,  addrMode = Abs, cycles = 4 },
                new Instruction { name = "ROL", operate =Rol, addrMode = Abs, cycles =6 },new Instruction {name =  "???", operate = Xxx, addrMode = Imp, cycles= 6 },
                new Instruction { name = "BMI", operate = Bmi, addrMode = Rel, cycles = 2 },new Instruction {name = "AND", operate =And, addrMode = Izy, cycles = 5 },
                new Instruction { name = "???", operate = Xxx, addrMode = Imp, cycles =2 },new Instruction {name =  "???", operate = Xxx, addrMode = Imp,cycles = 8 },
                new Instruction { name = "???", operate = Nop, addrMode = Zpx, cycles =4 },new Instruction {name = "AND", operate = And, addrMode = Zpx, cycles = 4 },
                new Instruction { name = "ROL", operate = Rol, addrMode = Zpx, cycles= 6 }, new Instruction{ name = "???", operate = Xxx, addrMode = Imp, cycles = 6 },
                new Instruction { name = "SEC", operate =Sec, addrMode = Imp, cycles =2 },new Instruction {name = "AND", operate =And, addrMode = Aby, cycles = 4 },
                new Instruction { name = "???", operate = Nop,  addrMode = Imp, cycles = 2 },new Instruction {name =  "???", operate =Xxx,  addrMode = Imp, cycles = 7 },
                new Instruction { name = "???", operate = Nop, addrMode = Abx,cycles = 4 }, new Instruction {name =  "AND", operate = And,  addrMode = Abx, cycles = 4 },
                new Instruction { name = "ROL", operate = Rol, addrMode = Abx, cycles =7 },new Instruction { name = "???", operate = Xxx, addrMode = Imp, cycles = 7 },
                new Instruction { name = "RTI", operate = Rti, addrMode = Imp, cycles = 6 },new Instruction{name = "EOR", operate = Eor, addrMode = Izx, cycles = 6 },
                new Instruction { name = "???", operate = Xxx, addrMode = Imp, cycles = 2 },new Instruction {name=  "???", operate= Xxx, addrMode = Imp, cycles  =8 },
                new Instruction { name = "???", operate = Nop, addrMode = Zp0, cycles = 3 }, new Instruction{ name = "EOR", operate = Eor, addrMode = Zp0, cycles =3 },
                new Instruction { name = "LSR", operate = Lsr, addrMode = Zp0,cycles = 5 }, new Instruction {name = "???", operate = Xxx, addrMode = Imp, cycles = 5 },
                new Instruction { name = "PHA", operate = Pha, addrMode = Imp, cycles= 3 }, new Instruction {name = "EOR", operate = Eor, addrMode = Imm, cycles = 2 },
                new Instruction { name = "LSR", operate =Lsr, addrMode =Imp,cycles= 2 },new Instruction{name = "???", operate =Xxx, addrMode = Imp, cycles= 2 },
                new Instruction { name = "JMP", operate=Jmp, addrMode =Abs,cycles= 3 },new Instruction {name = "EOR", operate =Eor,  addrMode=Abs, cycles=4 },
                new Instruction { name = "LSR", operate = Lsr, addrMode=Abs,cycles= 6 },new Instruction {name= "???", operate=Xxx,  addrMode=Imp, cycles=6 },
                new Instruction { name = "BVC", operate = Bvc, addrMode=Rel, cycles=2 },new Instruction{name= "EOR", operate=Eor, addrMode=Izy, cycles=5 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=2 },new Instruction {name= "???", operate=Xxx, addrMode=Imp,cycles= 8 },
                new Instruction { name = "???", operate=Nop, addrMode = Zpx, cycles =4 },new Instruction {name = "EOR", operate =Eor, addrMode =Zpx,cycles= 4 },
                new Instruction { name = "LSR", operate=Lsr,  addrMode=Zpx, cycles=6 },new Instruction {name= "???", operate=Xxx, addrMode=Imp, cycles= 6 },
                new Instruction { name = "CLI", operate=Cli, addrMode=Imp, cycles=2 },new Instruction{name= "EOR", operate=Eor, addrMode =Aby, cycles=4 },
                new Instruction { name = "???", operate=Nop, addrMode=Imp, cycles=2 },new Instruction { name ="???", operate =Xxx, addrMode=Imp, cycles=7 },
                new Instruction { name = "???", operate=Nop, addrMode=Abx, cycles=4 },new Instruction {name= "EOR", operate=Eor, addrMode =Abx,cycles= 4 },
                new Instruction { name = "LSR", operate=Lsr, addrMode=Abx, cycles=7 },new Instruction {name= "???", operate=Xxx, addrMode = Imp, cycles=7 },
                new Instruction { name = "RTS", operate= Rts, addrMode = Imp, cycles = 6 },new Instruction {name=  "ADC", operate= Adc, addrMode = Izx, cycles=6 },
                new Instruction { name = "???", operate=Xxx,  addrMode=Imp, cycles=2 },new Instruction {name=  "???", operate=Xxx, addrMode = Imp, cycles=8 },
                new Instruction { name = "???", operate=Nop, addrMode = Zp0, cycles =3 },new Instruction{name= "ADC", operate=Adc, addrMode=Zp0, cycles=3 },
                new Instruction { name = "ROR", operate = Ror, addrMode = Zp0, cycles=5 },new Instruction {name = "???", operate=Xxx, addrMode = Imp, cycles =5 },
                new Instruction { name = "PLA", operate = Pla, addrMode =Imp,cycles= 4 },new Instruction { name="ADC", operate=Adc, addrMode = Imm, cycles=2 },
                new Instruction { name = "ROR", operate =Ror, addrMode =Imp, cycles=2 },new Instruction { name = "???", operate =Xxx, addrMode = Imp,cycles= 2 },
                new Instruction { name = "JMP", operate = Jmp, addrMode = Ind, cycles=5 },new Instruction{name= "ADC", operate = Adc, addrMode = Abs, cycles=4 },
                new Instruction { name = "ROR", operate=Ror, addrMode= Abs, cycles=6 },new Instruction{name= "???", operate = Xxx, addrMode=Imp, cycles=6 },
                new Instruction { name = "BVS", operate=Bvs, addrMode=Rel, cycles=2 },new Instruction {name = "ADC", operate=Adc, addrMode=Izy, cycles=5 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=2 },new Instruction{name= "???", operate=Xxx, addrMode=Imp, cycles=8 },
                new Instruction { name = "???", operate=Nop, addrMode=Zpx, cycles=4 },new Instruction{name= "ADC", operate=Adc, addrMode=Zpx, cycles=4 },
                new Instruction { name = "ROR", operate=Ror, addrMode=Zpx, cycles=6 },new Instruction{name= "???", operate=Xxx, addrMode=Imp, cycles=6 },
                new Instruction { name = "SEI", operate=Sei, addrMode=Imp, cycles=2 },new Instruction{name= "ADC", operate=Adc, addrMode=Aby,cycles= 4 },
                new Instruction { name = "???", operate=Nop, addrMode=Imp, cycles=2 },new Instruction{name= "???", operate=Xxx, addrMode=Imp, cycles =7 },
                new Instruction { name = "???", operate=Nop, addrMode=Abx,cycles= 4 },new Instruction{name= "ADC", operate=Adc, addrMode=Abx, cycles=4 },
                new Instruction { name = "ROR", operate=Ror, addrMode=Abx, cycles=7 },new Instruction{name= "???", operate=Xxx, addrMode=Imp, cycles=7 },
                new Instruction { name = "???", operate=Nop, addrMode=Imm, cycles=2 },new Instruction{name= "STA", operate=Sta, addrMode=Izx, cycles=6 },
                new Instruction { name = "???", operate=Nop, addrMode=Imm, cycles=2 },new Instruction{name= "???", operate=Xxx, addrMode=Imp, cycles=6 },
                new Instruction { name = "STY", operate=Sty, addrMode=Zp0, cycles=3 },new Instruction{name= "STA", operate=Sta, addrMode=Zp0, cycles=3 },
                new Instruction { name = "STX", operate=Stx, addrMode=Zp0, cycles=3 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=3 },
                new Instruction { name = "DEY", operate=Dey, addrMode=Imp, cycles=2 },new Instruction{ name="???", operate=Nop, addrMode=Imm, cycles=2 },
                new Instruction { name = "TXA", operate=Txa, addrMode=Imp, cycles = 2 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=2 },
                new Instruction { name = "STY", operate=Sty, addrMode=Abs, cycles = 4 },new Instruction{ name="STA", operate=Sta, addrMode=Abs, cycles=4 },
                new Instruction { name = "STX", operate=Stx, addrMode=Abs, cycles = 4 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=4 },
                new Instruction { name = "BCC", operate=Bcc, addrMode=Rel, cycles = 2 },new Instruction{ name="STA", operate=Sta, addrMode=Izy, cycles=6 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=2 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=6 },
                new Instruction { name = "STY", operate=Sty, addrMode=Zpx, cycles = 4 },new Instruction{ name="STA", operate=Sta, addrMode=Zpx, cycles=4 },
                new Instruction { name = "STX", operate=Stx, addrMode=Zpy, cycles = 4 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=4 },
                new Instruction { name = "TYA", operate=Tya, addrMode=Imp, cycles = 2 },new Instruction{ name="STA", operate=Sta, addrMode=Aby, cycles=5 },
                new Instruction { name = "TXS", operate=Txs, addrMode=Imp, cycles = 2 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=5 },
                new Instruction { name = "???", operate=Nop, addrMode=Imp, cycles=5 },new Instruction{ name="STA", operate=Sta, addrMode=Abx, cycles=5 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=5 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=5 },
                new Instruction { name = "LDY", operate=Ldy, addrMode=Imm,cycles= 2 },new Instruction{name= "LDA", operate=Lda, addrMode=Izx,cycles= 6 },
                new Instruction { name = "LDX", operate=Ldx, addrMode=Imm, cycles=2 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=6 },
                new Instruction { name = "LDY", operate=Ldy, addrMode=Zp0, cycles=3 },new Instruction{name= "LDA", operate=Lda,  addrMode=Zp0, cycles=3 },
                new Instruction { name = "LDX", operate=Ldx, addrMode=Zp0, cycles=3 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=3 },
                new Instruction { name = "TAY", operate=Tay, addrMode=Imp, cycles=2 },new Instruction{name= "LDA", operate=Lda, addrMode=Imm, cycles=2 },
                new Instruction { name = "TAX", operate=Tax, addrMode=Imp, cycles=2 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=2 },
                new Instruction { name = "LDY", operate=Ldy, addrMode=Abs, cycles=4 },new Instruction{ name="LDA", operate=Lda, addrMode=Abs, cycles= 4 },
                new Instruction { name = "LDX", operate=Ldx, addrMode=Abs, cycles=4 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=4 },
                new Instruction { name = "BCS", operate=Bcs, addrMode=Rel, cycles=2 },new Instruction{name= "LDA", operate=Lda, addrMode=Izy, cycles=5 },new Instruction { name="???", operate=Xxx, addrMode=Imp, cycles=2 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=5 },new Instruction{name= "LDY", operate=Ldy, addrMode=Zpx, cycles=4 },new Instruction{name= "LDA", operate=Lda, addrMode=Zpx, cycles=4 },
                new Instruction { name = "LDX", operate=Ldx, addrMode=Zpy, cycles=4 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=4 },new Instruction{name= "CLV", operate=Clv, addrMode=Imp, cycles=2 },
                new Instruction { name = "LDA", operate=Lda, addrMode=Aby, cycles=4 },new Instruction{name= "TSX", operate=Tsx, addrMode=Imp, cycles=2 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=4 },
                new Instruction { name = "LDY", operate=Ldy, addrMode=Abx, cycles=4 },new Instruction{name= "LDA", operate=Lda, addrMode=Abx, cycles=4 },new Instruction {name= "LDX", operate=Ldx, addrMode=Aby, cycles=4 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=4 },
                new Instruction { name = "CPY", operate=Cpy, addrMode=Imm, cycles=2 },new Instruction{ name="CMP", operate=Cmp, addrMode=Izx, cycles=6 },new Instruction{ name="???", operate=Nop, addrMode=Imm, cycles=2 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=8 },new Instruction {name= "CPY", operate=Cpy, addrMode=Zp0, cycles=3 },new Instruction{name= "CMP", operate=Cmp, addrMode=Zp0, cycles=3 },
                new Instruction { name = "DEC", operate=Dec, addrMode=Zp0, cycles=5 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=5 },new Instruction{name= "INY", operate=Iny, addrMode=Imp, cycles=2 },
                new Instruction { name = "CMP", operate=Cmp, addrMode=Imm, cycles=2 },new Instruction{name= "DEX", operate=Dex, addrMode=Imp, cycles=2 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=2 },
                new Instruction { name = "CPY", operate=Cpy, addrMode=Abs, cycles=4 },new Instruction{name= "CMP", operate=Cmp, addrMode=Abs, cycles=4 },new Instruction{name= "DEC", operate=Dec, addrMode=Abs, cycles=6 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=6 },
                new Instruction { name = "BNE", operate=Bne, addrMode=Rel, cycles=2 },new Instruction{name= "CMP", operate=Cmp, addrMode=Izy, cycles=5 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=2 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=8 },new Instruction{ name="???", operate=Nop, addrMode=Zpx, cycles=4 },new Instruction{name= "CMP", operate=Cmp, addrMode=Zpx, cycles=4 },
                new Instruction { name = "DEC", operate=Dec, addrMode=Zpx, cycles=6 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=6 },new Instruction{name= "CLD", operate=Cld, addrMode=Imp, cycles=2 },
                new Instruction { name = "CMP", operate=Cmp, addrMode=Aby, cycles=4 },new Instruction{name= "NOP", operate=Nop, addrMode=Imp, cycles=2 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=7 },
                new Instruction { name = "???", operate=Nop, addrMode=Abx, cycles=4 },new Instruction{ name="CMP", operate=Cmp, addrMode=Abx, cycles=4 },new Instruction{ name="DEC", operate=Dec, addrMode=Abx, cycles=7 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=7 },
                new Instruction { name = "CPX", operate=Cpx, addrMode=Imm, cycles=2 },new Instruction{name= "SBC", operate=Sbc, addrMode=Izx, cycles=6 },new Instruction{ name="???", operate=Nop, addrMode=Imm, cycles=2 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=8 },new Instruction{ name="CPX", operate=Cpx, addrMode=Zp0, cycles=3 },new Instruction{name= "SBC", operate=Sbc, addrMode=Zp0, cycles=3 },
                new Instruction { name = "INC", operate=Inc, addrMode=Zp0, cycles=5 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=5 },new Instruction{name= "INX", operate=Inx, addrMode=Imp, cycles=2 },
                new Instruction { name = "SBC", operate=Sbc, addrMode=Imm, cycles=2 },new Instruction{name= "NOP", operate=Nop, addrMode=Imp, cycles=2 },new Instruction{ name="???", operate=Sbc, addrMode=Imm, cycles=2 },
                new Instruction { name = "CPX", operate=Cpx, addrMode=Abs, cycles=4 },new Instruction{name= "SBC", operate=Sbc, addrMode=Abs, cycles=4 },new Instruction{name= "INC", operate=Inc, addrMode=Abs, cycles=6 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=6 },
                new Instruction { name = "BEQ", operate=Beq, addrMode=Rel, cycles=2 },new Instruction{name= "SBC", operate=Sbc, addrMode=Izy, cycles=5 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=2 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=8 },new Instruction{ name="???", operate=Nop, addrMode=Zpx, cycles=4 },new Instruction{ name="SBC", operate=Sbc, addrMode=Zpx, cycles= 4 },
                new Instruction { name = "INC", operate=Inc, addrMode=Zpx, cycles=6 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=6 },new Instruction{name= "SED", operate=Sed, addrMode=Imp, cycles=2 },
                new Instruction { name = "SBC", operate=Sbc, addrMode=Aby, cycles=4 },new Instruction{ name="NOP", operate=Nop, addrMode=Imp, cycles=2 },new Instruction{ name="???", operate=Xxx, addrMode=Imp, cycles=7 },
                new Instruction { name = "???", operate=Nop, addrMode=Abx, cycles=4 },new Instruction{ name="SBC", operate=Sbc, addrMode=Abx, cycles=4 },new Instruction{name= "INC", operate=Inc, addrMode=Abx, cycles=7 },
                new Instruction { name = "???", operate=Xxx, addrMode=Imp, cycles=7 }
            };
        }

        internal void ConnectBus(Bus b)
        {
            bus = b;
        }

        private byte Read(int addr)
        {
            return bus.CpuRead(addr, false);
        }

        private void Write(int addr, byte data)
        {
            bus.CpuWrite(addr, ref data);
        }

        public void Clock()
        {
            if (cycles == 0)
            {
                opcode = Read(pc);

                SetFlag(Flags6502.U, false);

                pc++;
                var op = lookup[opcode];

                cycles = op.cycles;

                byte addCycle1 = op.addrMode();

                byte addCycle2 = op.operate();

                SetFlag(Flags6502.U, true);

                cycles += Convert.ToByte((addCycle1 & addCycle2));
                cycleCount++;
            }

            cycles--;
        }

        public int Cycles { get { return cycles; } }

        internal int Reset()
        {
            a = 0;
            x = 0;
            y = 0;
            stkp = 0xFD;
            status = 0x00 | (byte)Flags6502.U;

            addrAbs = 0xFFFC;
            ushort lo = Read(addrAbs);
            ushort hi = Read(addrAbs + 1);

            pc = (ushort)((hi << 8) | lo);
            addrRel = 0;
            addrAbs = 0;
            fetched = 0;

            cycles = 8;
            cycleCount = 0;
            return cycles;
        }

        private int Irq()
        {
            if (GetFlag(Flags6502.I) == 0)
            {
                Write(0x0100 + stkp, (byte)((pc >> 8) & 0x00ff));
                stkp--;
                Write(0x0100 + stkp, (byte)(pc & 0x00ff));
                stkp--;

                SetFlag(Flags6502.B, false);
                SetFlag(Flags6502.U, true);
                SetFlag(Flags6502.I, true);

                Write(0x0100 + stkp--, status);

                addrAbs = 0xfffe;
                ushort lo = Read(addrAbs);
                ushort hi = Read(addrAbs + 1);
                pc = (ushort)((hi << 8) | lo);

                cycles = 7;
                return cycles;
            }

            return 0;
        }

        internal int Nmi()
        {
            Write(0x0100 + stkp, (byte)((pc >> 8) & 0x00ff));
            stkp--;
            Write(0x0100 + stkp, (byte)(pc & 0x00ff));
            stkp--;

            SetFlag(Flags6502.B, false);
            SetFlag(Flags6502.U, true);
            SetFlag(Flags6502.I, true);

            Write(0x0100 + stkp, status);
            stkp--;

            addrAbs = 0xFFFA;
            ushort lo = Read(addrAbs);
            ushort hi = Read(addrAbs + 1);
            pc = (ushort)((hi << 8) | lo);

            cycles = 8;
            return cycles;
        }

        public bool Complete()
        {
            return cycles == 0;
        }

        Dictionary<int, string> Disassemble(int nStart, uint nStop)
        {
            int addr = nStart;
            uint value = 0x00, lo = 0x00, hi = 0x00;
            Dictionary<int, string> mapLines = new Dictionary<int, string>(0x00ff);
            int line_addr = 0;

            // Starting at the specified address we read an instruction
            // byte, which in turn yields information from the lookup table
            // as to how many additional bytes we need to read and what the
            // addressing mode is. I need this info to assemble human readable
            // syntax, which is different depending upon the addressing mode

            // As the instruction is decoded, a std::string is assembled
            // with the readable output
            while (addr <= nStop)
            {
                line_addr = addr;

                // Prefix line with instruction address
                string sInst = $"${addr.ToString("X4")}: ";

                // Read instruction, and get its readable name
                byte opcode = bus.CpuRead(addr, true); addr++;
                sInst += lookup[opcode].name + " ";

                // Get oprands from desired locations, and form the
                // instruction based upon its addressing mode. These
                // routines mimmick the actual fetch routine of the
                // 6502 in order to get accurate data as part of the
                // instruction
                if (lookup[opcode].addrMode == Imp)
                {
                    sInst += " {IMP}";
                }
                else if (lookup[opcode].addrMode == Imm)
                {
                    value = bus.CpuRead(addr, true); addr++;
                    sInst += $"#${addr.ToString("X4")} {{IMM}}";
                }
                else if (lookup[opcode].addrMode == Zp0)
                {
                    lo = bus.CpuRead(addr, true); addr++;
                    hi = 0x00;
                    sInst += "$" + lo.ToString("X2") + " {ZP0}";
                }
                else if (lookup[opcode].addrMode == Zpx)
                {
                    lo = bus.CpuRead(addr, true); addr++;
                    hi = 0x00;
                    sInst += "$" + lo.ToString("X2") + ", X {ZPX}";
                }
                else if (lookup[opcode].addrMode == Zpy)
                {
                    lo = bus.CpuRead(addr, true); addr++;
                    hi = 0x00;
                    sInst += "$" + lo.ToString("X2") + ", Y {ZPY}";
                }
                else if (lookup[opcode].addrMode == Izx)
                {
                    lo = bus.CpuRead(addr, true); addr++;
                    hi = 0x00;
                    sInst += "($" + lo.ToString("X4") + ", X) {IZX}";
                }
                else if (lookup[opcode].addrMode == Izy)
                {
                    lo = bus.CpuRead(addr, true); addr++;
                    hi = 0x00;
                    sInst += "($" + lo.ToString("X2") + "), Y {IZY}";
                }
                else if (lookup[opcode].addrMode == Abs)
                {
                    lo = bus.CpuRead(addr, true); addr++;
                    hi = bus.CpuRead(addr, true); addr++;
                    sInst += "$" + ((hi << 8) | lo).ToString("X4") + " {ABS}";
                }
                else if (lookup[opcode].addrMode == Abx)
                {
                    lo = bus.CpuRead(addr, true); addr++;
                    hi = bus.CpuRead(addr, true); addr++;
                    sInst += "$" + ((hi << 8) | lo).ToString("X4") + ", X {ABX}";
                }
                else if (lookup[opcode].addrMode == Aby)
                {
                    lo = bus.CpuRead(addr, true); addr++;
                    hi = bus.CpuRead(addr, true); addr++;
                    sInst += "$" + ((hi << 8) | lo).ToString("X4") + ", Y {ABY}";
                }
                else if (lookup[opcode].addrMode == Ind)
                {
                    lo = bus.CpuRead(addr, true); addr++;
                    hi = bus.CpuRead(addr, true); addr++;
                    sInst += "($" + ((hi << 8) | lo).ToString("X4") + ") {IND}";
                }
                else if (lookup[opcode].addrMode == Rel)
                {
                    value = bus.CpuRead(addr, true); addr++;
                    sInst += "$" + value.ToString("X2") + " [$" + (addr + value).ToString("X4") + "] {REL}";
                }

                // Add the formed string to a std::map, using the instruction's
                // address as the key. This makes it convenient to look for later
                // as the instructions are variable in length, so a straight up
                // incremental index is not sufficient.
                mapLines[line_addr & 0x00ff] = sInst;
            }

            return mapLines;
        }

        internal void SetFlag(Flags6502 f, bool v)
        {
            if (v)
                status |= (byte)f;
            else
                status &= Convert.ToByte((~((byte)f) & 0xff));
        }

        internal byte GetFlag(Flags6502 f)
        {
            return (byte)(status & (byte)f);
        }

        public int A { get => a; }

        public int Y { get => y; }

        public int X { get => x; }

        public int Pc { get => pc; }

        public int IrqIns { get => Irq(); }

        public int NmiIns { get => Nmi(); }

        public int Stkp { get => stkp; }

    }

    public delegate byte AddressingMode();
    internal delegate string HexDelegate(uint n, byte d);


    public struct Instruction
    {
        public string name;
        public AddressingMode operate;
        public AddressingMode addrMode;
        public byte cycles;
    }
}
