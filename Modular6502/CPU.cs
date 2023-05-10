using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modular6502
{
    public class CPU
    {
        public readonly List<Device> Devices = new();
        public readonly Dictionary<ushort, ExternalRegister> ExternalRegisters = new();

        public byte A;
        public byte X;
        public byte Y;
        public ushort PC;
        public byte SP;

        public bool N = true;
        public bool V = true;
        public bool B = true;
        public bool D = true;
        public bool I = true;
        public bool Z = true;
        public bool C = true;
        
        //public long Cycle { get; protected set; }

        public ushort StackLocation = 0x0100;

        public CPU()
        {

        }

        public void Reset()
        {
            B = true;
            D = false;
            I = true;
            SP = 0;
            PC = ReadWord(0xFFFC);
            //Cycle = 0;
        }

        public CPU Map(Device device)
        {
            if (device.FixedSize == 0) throw new ArgumentOutOfRangeException(nameof(device), "Device does not provide a fixed size");
            return Map(device, device.FixedSize);
        }

        private int totalAllocatedSizeCount = 0;
        public CPU Map(Device device, ushort allocatedSize)
        {
            totalAllocatedSizeCount += allocatedSize;
            if (totalAllocatedSizeCount > 0xFFFF)
            {
                throw new ArgumentOutOfRangeException(nameof(device), "Unable to allocate memory for device");
            }

            device.Init(allocatedSize);
            Devices.Add(device);
            return this;
        }

        public CPU Map(ExternalRegister register, ushort addr)
        {
            register.Init(addr);
            ExternalRegisters[addr] = register;
            return this;
        }

        public byte Read(ushort addr)
        {
            //Cycle++;

            foreach (ushort i in ExternalRegisters.Keys)
            {
                if (addr - i <= 4 && addr - i >= 0)
                {
                    return ExternalRegisters[i].Read((ushort)(addr - i));
                }
            }

            ushort original = addr;
            foreach (var i in Devices)
            {
                if (i.AllocatedSize > addr)
                {
                    return i.Read(addr);
                }
                addr -= i.AllocatedSize;
            }
            Console.WriteLine($"CPU tried to read from unmapped address ${original:X}");
            return 0;
        }

        public ushort ReadWord(ushort addr)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (ushort)(Read(addr) | (Read((ushort)(addr + 1)) << 8));
            }
            else
            {
                return (ushort)((Read(addr) << 8) | Read((ushort)(addr + 1)));
            }
        }

        public void Write(ushort addr, byte value)
        {
            //Cycle++;

            foreach (ushort i in ExternalRegisters.Keys)
            {
                if (addr - i <= 4 && addr - i >= 0)
                {
                    ExternalRegisters[i].Write((ushort)(addr - i), value);
                    return;
                }
            }

            ushort original = addr;
            foreach (var i in Devices)
            {
                if (i.AllocatedSize > addr)
                {
                    i.Write(addr, value);
                    return;
                }
                addr -= i.AllocatedSize;
            }
            Console.WriteLine($"CPU tried to write to unmapped address ${original:X}");
        }

        public void WriteWord(ushort addr, ushort value)
        {
            if (BitConverter.IsLittleEndian)
            {
                Write(addr, (byte)(value & 0xFF));
                Write((ushort)(addr + 1), (byte)((value >> 8) & 0xFF));
            }
            else
            {
                Write(addr, (byte)((value >> 8) & 0xFF));
                Write((ushort)(addr + 1), (byte)(value & 0xFF));
            }
        }

        public byte ReadNext()
        {
            byte insn = Read(PC);
            PC++;
            return insn;
        }

        public ushort ReadNextWord()
        {
            ushort insn = ReadWord(PC);
            PC += 2;
            return insn;
        }

        public byte GetStatusRegisters()
        {
            return (byte)((byte)(Convert.ToByte(N) << 7) |
                   (byte)(Convert.ToByte(V) << 6) |
                   0b00110000 |
                   (byte)(Convert.ToByte(D) << 3) |
                   (byte)(Convert.ToByte(I) << 2) |
                   (byte)(Convert.ToByte(Z) << 1) |
                   Convert.ToByte(C));
        }

        public void LoadStatusRegisters(byte data)
        {
            N = (data & 0b10000000) > 0;
            V = (data & 0b01000000) > 0;
            D = (data & 0b00001000) > 0;
            I = (data & 0b00000100) > 0;
            Z = (data & 0b00000010) > 0;
            C = (data & 0b00000001) > 0;
        }

        public void Step()
        {
            byte insn = ReadNext();

            #region Addressing modes
            byte imm() => ReadNext();
            byte abs() => Read(ReadNextWord());
            byte x_abs() => Read((ushort)(ReadNextWord() + X));
            byte y_abs() => Read((ushort)(ReadNextWord() + Y));
            byte zero() => Read(ReadNext());
            byte x_zero() => Read((ushort)(ReadNext() + X));
            byte y_zero() => Read((ushort)(ReadNext() + Y));
            byte x_zero_ind() => Read(ReadWord((ushort)(ReadNext() + X)));
            byte zero_y_ind() => Read((ushort)(ReadWord(ReadNext()) + Y));

            ushort write_abs() => ReadNextWord();
            ushort write_x_abs() => (ushort)(ReadNextWord() + X);
            ushort write_y_abs() => (ushort)(ReadNextWord() + Y);
            ushort write_zero() => ReadNext();
            ushort write_x_zero() => (ushort)(ReadNext() + X);
            ushort write_x_zero_ind() => ReadWord((ushort)(ReadNext() + X));
            ushort write_y_zero_ind() => ReadWord((ushort)(ReadNext() + Y));
            ushort write_zero_y_ind() => (ushort)(ReadWord(ReadNext()) + Y);
            #endregion

            switch (insn)
            {
                case 0xEA: // NOP
                    break;
                #region Arithmetic
                case 0x69:
                    ADC(imm());
                    break;
                case 0x6D:
                    ADC(abs());
                    break;
                case 0x7D:
                    ADC(x_abs());
                    break;
                case 0x79:
                    ADC(y_abs());
                    break;
                case 0x65:
                    ADC(zero());
                    break;
                case 0x75:
                    ADC(x_zero());
                    break;
                case 0x61:
                    ADC(x_zero_ind());
                    break;
                case 0x71:
                    ADC(zero_y_ind());
                    break;
                #endregion
                #region Loading
                case 0xA9:
                    LDA(imm());
                    break;
                case 0xAD:
                    LDA(abs());
                    break;
                case 0xBD:
                    LDA(x_abs());
                    break;
                case 0xB9:
                    LDA(y_abs());
                    break;
                case 0xA5:
                    LDA(zero());
                    break;
                case 0xB5:
                    LDA(x_zero());
                    break;
                case 0xA1:
                    LDA(x_zero_ind());
                    break;
                case 0xB1:
                    LDA(zero_y_ind());
                    break;

                case 0xA2:
                    LDX(imm());
                    break;
                case 0xAE:
                    LDX(abs());
                    break;
                case 0xBE:
                    LDX(y_abs());
                    break;
                case 0xA6:
                    LDX(zero());
                    break;
                case 0xB6:
                    LDX(y_zero());
                    break;

                case 0xA0:
                    LDY(imm());
                    break;
                case 0xAC:
                    LDY(abs());
                    break;
                case 0xBC:
                    LDY(x_abs());
                    break;
                case 0xA4:
                    LDY(zero());
                    break;
                case 0xB4:
                    LDY(x_zero());
                    break;
                #endregion
                #region Storing
                case 0x8D:
                    STA(write_abs());
                    break;
                case 0x9D:
                    STA(write_x_abs());
                    break;
                case 0x99:
                    STA(write_y_abs());
                    break;
                case 0x85:
                    STA(write_zero());
                    break;
                case 0x95:
                    STA(write_x_zero());
                    break;
                case 0x81:
                    STA(write_x_zero_ind());
                    break;
                case 0x91:
                    STA(write_zero_y_ind());
                    break;

                case 0x8E:
                    STX(write_abs());
                    break;
                case 0x86:
                    STX(write_zero());
                    break;
                case 0x96:
                    STX(write_y_zero_ind());
                    break;

                case 0x8C:
                    STY(write_abs());
                    break;
                case 0x84:
                    STY(write_zero());
                    break;
                case 0x94:
                    STY(write_x_zero_ind());
                    break;
                #endregion
                #region Transfers
                case 0xAA:
                    TAX();
                    break;
                case 0xA8:
                    TAY();
                    break;
                case 0xBA:
                    TSX();
                    break;
                case 0x8A:
                    TXA();
                    break;
                case 0x9A:
                    TXS();
                    break;
                case 0x98:
                    TYA();
                    break;
                #endregion
                #region Stack
                case 0x48:
                    PHA();
                    break;
                case 0x08:
                    PHP();
                    break;
                case 0x68:
                    PLA();
                    break;
                case 0x28:
                    PLP();
                    break;
                #endregion
                #region Shifts
                case 0x0A:
                    ASL();
                    break;
                case 0x0E:
                    ASL(write_abs());
                    break;
                case 0x1E:
                    ASL(write_x_abs());
                    break;
                case 0x06:
                    ASL(write_zero());
                    break;
                case 0x16:
                    ASL(write_x_zero());
                    break;

                case 0x4A:
                    LSR();
                    break;
                case 0x4E:
                    LSR(write_abs());
                    break;
                case 0x5E:
                    LSR(write_x_abs());
                    break;
                case 0x46:
                    LSR(write_zero());
                    break;
                case 0x56:
                    LSR(write_x_zero());
                    break;

                case 0x2A:
                    ROL();
                    break;
                case 0x2E:
                    ROL(write_abs());
                    break;
                case 0x3E:
                    ROL(write_x_abs());
                    break;
                case 0x26:
                    ROL(write_zero());
                    break;
                case 0x36:
                    ROL(write_x_zero());
                    break;

                case 0x6A:
                    ROR();
                    break;
                case 0x6E:
                    ROR(write_abs());
                    break;
                case 0x7E:
                    ROR(write_x_abs());
                    break;
                case 0x66:
                    ROR(write_zero());
                    break;
                case 0x76:
                    ROR(write_x_zero());
                    break;
                #endregion
                #region Logicals
                case 0x29:
                    AND(imm());
                    break;
                case 0x2D:
                    AND(abs());
                    break;
                case 0x3D:
                    AND(x_abs());
                    break;
                case 0x39:
                    AND(y_abs());
                    break;
                case 0x25:
                    AND(zero());
                    break;
                case 0x35:
                    AND(x_zero());
                    break;
                case 0x21:
                    AND(x_zero_ind());
                    break;
                case 0x31:
                    AND(zero_y_ind());
                    break;

                case 0x2C:
                    BIT(abs());
                    break;
                case 0x24:
                    BIT(zero());
                    break;

                case 0x49:
                    EOR(imm());
                    break;
                case 0x4D:
                    EOR(abs());
                    break;
                case 0x5D:
                    EOR(x_abs());
                    break;
                case 0x59:
                    EOR(y_abs());
                    break;
                case 0x45:
                    EOR(zero());
                    break;
                case 0x55:
                    EOR(x_zero());
                    break;
                case 0x41:
                    EOR(x_zero_ind());
                    break;
                case 0x51:
                    EOR(zero_y_ind());
                    break;

                case 0x09:
                    ORA(imm());
                    break;
                case 0x0D:
                    ORA(abs());
                    break;
                case 0x1D:
                    ORA(x_abs());
                    break;
                case 0x19:
                    ORA(y_abs());
                    break;
                case 0x05:
                    ORA(zero());
                    break;
                case 0x15:
                    ORA(x_zero());
                    break;
                case 0x01:
                    ORA(x_zero_ind());
                    break;
                case 0x11:
                    ORA(zero_y_ind());
                    break;

                #endregion
                default:
                    break;
            }

            //Cycle++;
        }

        public void ADC(byte val)
        {
            if (D)
            {
                // TODO: decimal mode
            }
            else
            {
                var res = A + val + (C ? 1 : 0);
                var sres = (sbyte)A + (sbyte)val + (C ? 1 : 0);
                C = res > 255;
                Z = res == 0;
                V = sres > 127 || sres < -128;
                N = sres < 0;
                A = (byte)res;
            }
        }

        public void LDA(byte val)
        {
            A = val;
            Z = A == 0;
            N = (A & 0b10000000) != 0;
        }

        public void LDX(byte val)
        {
            X = val;
            Z = X == 0;
            N = (X & 0b10000000) != 0;
        }

        public void LDY(byte val)
        {
            Y = val;
            Z = Y == 0;
            N = (Y & 0b10000000) != 0;
        }

        public void STA(ushort addr)
        {
            Write(addr, A);
        }

        public void STX(ushort addr)
        {
            Write(addr, X);
        }

        public void STY(ushort addr)
        {
            Write(addr, Y);
        }

        public void TAX()
        {
            X = A;
            Z = X == 0;
            N = (X & 0b10000000) != 0;
        }

        public void TAY()
        {
            Y = A;
            Z = Y == 0;
            N = (Y & 0b10000000) != 0;
        }

        public void TSX()
        {
            X = SP;
            Z = X == 0;
            N = (X & 0b10000000) != 0;
        }

        public void TXA()
        {
            A = X;
            Z = A == 0;
            N = (A & 0b10000000) != 0;
        }

        public void TXS()
        {
            SP = X;
        }

        public void TYA()
        {
            A = Y;
            Z = A == 0;
            N = (A & 0b10000000) != 0;
        }

        public void PHA()
        {
            Write((ushort)(0x0100 + SP), A);
            SP--;
        }

        public void PHP()
        {
            Write((ushort)(0x0100 + SP), GetStatusRegisters());
            SP--;
        }

        public void PLA()
        {
            SP++;
            A = Read((ushort)(0x0100 + SP));
            Z = A == 0;
            N = (A & 0b10000000) != 0;
        }

        public void PLP()
        {
            SP++;
            LoadStatusRegisters(Read((ushort)(0x0100 + SP)));
        }

        public void ASL()
        {
            C = (A & 0b10000000) != 0;
            A <<= 1;
            N = (A & 0b10000000) != 0;
            Z = A == 0;
        }

        public void ASL(ushort addr)
        {
            byte x = Read(addr);
            C = (x & 0b10000000) != 0;
            x <<= 1;
            N = (x & 0b10000000) != 0;
            Z = x == 0;
            Write(addr, x);
        }

        public void LSR()
        {
            C = (A & 0b00000001) != 0;
            A >>= 1;
            N = false;
            Z = A == 0;
        }

        public void LSR(ushort addr)
        {
            byte x = Read(addr);
            C = (x & 0b00000001) != 0;
            x >>= 1;
            N = false;
            Z = x == 0;
            Write(addr, x);
        }

        public void ROL()
        {
            bool wasCarry = C;
            C = (A & 0b10000000) != 0;
            A <<= 1;
            if (wasCarry) A++;
            N = (A & 0b10000000) != 0;
            Z = A == 0;
        }

        public void ROL(ushort addr)
        {
            bool wasCarry = C;
            byte x = Read(addr);
            C = (x & 0b10000000) != 0;
            x <<= 1;
            if (wasCarry) x++;
            N = (x & 0b10000000) != 0;
            Z = x == 0;
            Write(addr, x);
        }

        public void ROR()
        {
            bool wasCarry = C;
            C = (A & 0b00000001) != 0;
            A >>= 1;
            if (wasCarry) A |= 0b10000000;
            N = (A & 0b10000000) != 0;
            Z = A == 0;
        }

        public void ROR(ushort addr)
        {
            bool wasCarry = C;
            byte x = Read(addr);
            C = (x & 0b00000001) != 0;
            x >>= 1;
            if (wasCarry) x |= 0b10000000;
            N = (x & 0b10000000) != 0;
            Z = x == 0;
            Write(addr, x);
        }

        public void AND(byte val)
        {
            A = (byte)(A & val);
            N = (A & 0b10000000) != 0;
            Z = A == 0;
        }

        public void BIT(byte val)
        {
            var and = (byte)(A & val);
            N = (val & 0b10000000) != 0;
            V = (val & 0b01000000) != 0;
            Z = and == 0;
        }

        public void EOR(byte val)
        {
            A = (byte)(A ^ val);
            N = (A & 0b10000000) != 0;
            Z = A == 0;
        }

        public void ORA(byte val)
        {
            A = (byte)(A | val);
            N = (A & 0b10000000) != 0;
            Z = A == 0;
        }
    }
}
