using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Modular6502
{
    public class ExternalRegister
    {
        public ushort Address { get; protected set; }
        public readonly string Name;

        protected byte[] value;

        public ExternalRegister(string name, ushort size = 1)
        {
            Name = name;
            value = new byte[size];
            if (size > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "External registers can only hold a max of 4 bytes. Use a device if more storage is needed");
            }
        }

        public void Init(ushort address)
        {
            Address = address;
        }

        public byte Read(ushort address)
        {
            return value[address];
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

        public void Write(ushort address, byte value)
        {
            this.value[address] = value;
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
    }
}
