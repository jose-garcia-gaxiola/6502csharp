using Components;

static void Equal<T>(T expected, T actual, string context)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"{context}: expected {expected}, got {actual}");
}

static void Write(Bus bus, int address, byte value) => bus.CpuWrite(address, ref value);

static Bus Program(params byte[] bytes)
{
    var bus = new Bus();
    for (int i = 0; i < bytes.Length; i++)
        Write(bus, i, bytes[i]);
    return bus;
}

static void Execute(Bus bus, int expectedCycles, int expectedPc)
{
    int elapsed = 0;
    do
    {
        bus.Cpu.Clock();
        if (++elapsed > 16)
            throw new InvalidOperationException("Instruction did not complete within 16 cycles");
    } while (!bus.Cpu.Complete());
    Equal(expectedCycles, elapsed, "Instruction cycles");
    Equal(expectedPc, bus.Cpu.Pc, "Program counter");
}

var cpu = new Olc6502();
Equal(256, cpu.lookup.Count, "Lookup size");
var modes = new HashSet<string>
{
    "Imp", "Imm", "Zp0", "Zpx", "Zpy", "Rel", "Abs", "Abx", "Aby", "Ind", "Izx", "Izy"
};
for (int opcode = 0; opcode < cpu.lookup.Count; opcode++)
{
    var entry = cpu.lookup[opcode];
    if (string.IsNullOrEmpty(entry.name) || entry.operate is null || entry.addrMode is null ||
        entry.cycles == 0 || !modes.Contains(entry.addrMode.Method.Name) ||
        !ReferenceEquals(cpu, entry.operate.Target) || !ReferenceEquals(cpu, entry.addrMode.Target))
        throw new InvalidOperationException($"Invalid lookup entry at ${opcode:X2}");
}

const string officialOpcodes = """
00:BRK:Imp:7 01:ORA:Izx:6 05:ORA:Zp0:3 06:ASL:Zp0:5 08:PHP:Imp:3 09:ORA:Imm:2 0A:ASL:Imp:2 0D:ORA:Abs:4 0E:ASL:Abs:6
10:BPL:Rel:2 11:ORA:Izy:5 15:ORA:Zpx:4 16:ASL:Zpx:6 18:CLC:Imp:2 19:ORA:Aby:4 1D:ORA:Abx:4 1E:ASL:Abx:7
20:JSR:Abs:6 21:AND:Izx:6 24:BIT:Zp0:3 25:AND:Zp0:3 26:ROL:Zp0:5 28:PLP:Imp:4 29:AND:Imm:2 2A:ROL:Imp:2 2C:BIT:Abs:4 2D:AND:Abs:4 2E:ROL:Abs:6
30:BMI:Rel:2 31:AND:Izy:5 35:AND:Zpx:4 36:ROL:Zpx:6 38:SEC:Imp:2 39:AND:Aby:4 3D:AND:Abx:4 3E:ROL:Abx:7
40:RTI:Imp:6 41:EOR:Izx:6 45:EOR:Zp0:3 46:LSR:Zp0:5 48:PHA:Imp:3 49:EOR:Imm:2 4A:LSR:Imp:2 4C:JMP:Abs:3 4D:EOR:Abs:4 4E:LSR:Abs:6
50:BVC:Rel:2 51:EOR:Izy:5 55:EOR:Zpx:4 56:LSR:Zpx:6 58:CLI:Imp:2 59:EOR:Aby:4 5D:EOR:Abx:4 5E:LSR:Abx:7
60:RTS:Imp:6 61:ADC:Izx:6 65:ADC:Zp0:3 66:ROR:Zp0:5 68:PLA:Imp:4 69:ADC:Imm:2 6A:ROR:Imp:2 6C:JMP:Ind:5 6D:ADC:Abs:4 6E:ROR:Abs:6
70:BVS:Rel:2 71:ADC:Izy:5 75:ADC:Zpx:4 76:ROR:Zpx:6 78:SEI:Imp:2 79:ADC:Aby:4 7D:ADC:Abx:4 7E:ROR:Abx:7
81:STA:Izx:6 84:STY:Zp0:3 85:STA:Zp0:3 86:STX:Zp0:3 88:DEY:Imp:2 8A:TXA:Imp:2 8C:STY:Abs:4 8D:STA:Abs:4 8E:STX:Abs:4
90:BCC:Rel:2 91:STA:Izy:6 94:STY:Zpx:4 95:STA:Zpx:4 96:STX:Zpy:4 98:TYA:Imp:2 99:STA:Aby:5 9A:TXS:Imp:2 9D:STA:Abx:5
A0:LDY:Imm:2 A1:LDA:Izx:6 A2:LDX:Imm:2 A4:LDY:Zp0:3 A5:LDA:Zp0:3 A6:LDX:Zp0:3 A8:TAY:Imp:2 A9:LDA:Imm:2 AA:TAX:Imp:2 AC:LDY:Abs:4 AD:LDA:Abs:4 AE:LDX:Abs:4
B0:BCS:Rel:2 B1:LDA:Izy:5 B4:LDY:Zpx:4 B5:LDA:Zpx:4 B6:LDX:Zpy:4 B8:CLV:Imp:2 B9:LDA:Aby:4 BA:TSX:Imp:2 BC:LDY:Abx:4 BD:LDA:Abx:4 BE:LDX:Aby:4
C0:CPY:Imm:2 C1:CMP:Izx:6 C4:CPY:Zp0:3 C5:CMP:Zp0:3 C6:DEC:Zp0:5 C8:INY:Imp:2 C9:CMP:Imm:2 CA:DEX:Imp:2 CC:CPY:Abs:4 CD:CMP:Abs:4 CE:DEC:Abs:6
D0:BNE:Rel:2 D1:CMP:Izy:5 D5:CMP:Zpx:4 D6:DEC:Zpx:6 D8:CLD:Imp:2 D9:CMP:Aby:4 DD:CMP:Abx:4 DE:DEC:Abx:7
E0:CPX:Imm:2 E1:SBC:Izx:6 E4:CPX:Zp0:3 E5:SBC:Zp0:3 E6:INC:Zp0:5 E8:INX:Imp:2 E9:SBC:Imm:2 EA:NOP:Imp:2 EC:CPX:Abs:4 ED:SBC:Abs:4 EE:INC:Abs:6
F0:BEQ:Rel:2 F1:SBC:Izy:5 F5:SBC:Zpx:4 F6:INC:Zpx:6 F8:SED:Imp:2 F9:SBC:Aby:4 FD:SBC:Abx:4 FE:INC:Abx:7
""";
var specifications = officialOpcodes.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
Equal(151, specifications.Length, "Official opcode count");
foreach (string specification in specifications)
{
    string[] parts = specification.Split(':');
    var entry = cpu.lookup[Convert.ToInt32(parts[0], 16)];
    Equal(parts[1], entry.name, specification);
    Equal(parts[1], entry.operate.Method.Name.ToUpperInvariant(), specification);
    Equal(parts[2], entry.addrMode.Method.Name, specification);
    Equal(byte.Parse(parts[3]), entry.cycles, specification);
}

foreach (ushort address in new ushort[] { 0x0200, 0x01FF })
{
    byte lo = (byte)(address & 0xFF);
    byte hi = (byte)(address >> 8);
    int target = address + 1;
    int readCycles = address == 0x01FF ? 5 : 4;
    var bus = Program(0xA2, 0x01, 0xA9, 0x40,
        0x9D, lo, hi, 0x7D, lo, hi, 0x7E, lo, hi,
        0xBC, lo, hi, 0xBD, lo, hi, 0xDD, lo, hi,
        0xF0, 0x00, 0xDE, lo, hi);
    Execute(bus, 2, 2);
    Execute(bus, 2, 4);
    Execute(bus, 5, 7);
    Equal((byte)0x40, bus.CpuRead(target), "STA absolute,X writes memory");
    Equal(0x40, bus.Cpu.A, "STA preserves accumulator");
    Execute(bus, readCycles, 10);
    Equal(0x80, bus.Cpu.A, "ADC absolute,X");
    Execute(bus, 7, 13);
    Equal((byte)0x20, bus.CpuRead(target), "ROR absolute,X");
    Execute(bus, readCycles, 16);
    Equal(0x20, bus.Cpu.Y, "LDY absolute,X");
    Execute(bus, readCycles, 19);
    Equal(0x20, bus.Cpu.A, "LDA absolute,X");
    Execute(bus, readCycles, 22);
    // BEQ observes CMP's zero flag without exposing private CPU state.
    Execute(bus, 3, 24);
    Execute(bus, 7, 27);
    Equal((byte)0x1F, bus.CpuRead(target), "DEC absolute,X");
}

var nopGroups = new (string Mode, int Cycles, int Length, byte[] Opcodes)[]
{
    ("Imp", 2, 1, [0x1A, 0x3A, 0x5A, 0x7A, 0xDA, 0xFA]),
    ("Imm", 2, 2, [0x80, 0x82, 0x89, 0xC2, 0xE2]),
    ("Zp0", 3, 2, [0x04, 0x44, 0x64]),
    ("Zpx", 4, 2, [0x14, 0x34, 0x54, 0x74, 0xD4, 0xF4]),
    ("Abs", 4, 3, [0x0C]),
    ("Abx", 4, 3, [0x1C, 0x3C, 0x5C, 0x7C, 0xDC, 0xFC])
};
foreach (var group in nopGroups)
{
    foreach (byte opcode in group.Opcodes)
    {
        Equal("Nop", cpu.lookup[opcode].operate.Method.Name, $"NOP ${opcode:X2}");
        Equal(group.Mode, cpu.lookup[opcode].addrMode.Method.Name, $"NOP ${opcode:X2}");
        var bus = Program(opcode, 0x00, 0x02);
        Execute(bus, group.Cycles, group.Length);
        Equal(0, bus.Cpu.A, $"NOP ${opcode:X2} preserves accumulator");
        if (group.Mode == "Abx")
        {
            bus = Program(0xA2, 0x01, opcode, 0xFF, 0x01);
            Execute(bus, 2, 2);
            Execute(bus, 5, 5);
            Equal(1, bus.Cpu.X, $"NOP ${opcode:X2} preserves X");
        }
    }
}

Equal("Sbc", cpu.lookup[0xEB].operate.Method.Name, "Unofficial SBC handler");
Equal("Imm", cpu.lookup[0xEB].addrMode.Method.Name, "Unofficial SBC addressing");
var sbcBus = Program(0xA9, 0x05, 0x38, 0xEB, 0x02);
Execute(sbcBus, 2, 2);
Execute(sbcBus, 2, 3);
Execute(sbcBus, 2, 5);
Equal(3, sbcBus.Cpu.A, "Unofficial SBC consumes immediate operand");

var brkBus = Program(0xA2, 0xFD, 0x9A, 0x00, 0xEA);
Execute(brkBus, 2, 2);
Execute(brkBus, 2, 3);
Execute(brkBus, 7, 0);
Equal((byte)0x00, brkBus.CpuRead(0x01FD), "BRK return address high byte");
Equal((byte)0x05, brkBus.CpuRead(0x01FC), "BRK returns after padding byte");
Equal(0xFA, brkBus.Cpu.Stkp, "BRK pushes three bytes");

Console.WriteLine("Passed: 256 lookup entries, 151 official mappings, indexed execution/timing, 27 unofficial NOPs, SBC alias, and BRK return address.");
