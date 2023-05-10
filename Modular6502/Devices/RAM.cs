using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modular6502.Devices
{
    public class RAM : Device
    {
        protected byte[] data = new byte[1];

        public override void Init(ushort allocatedSize)
        {
            base.Init(allocatedSize);
            data = new byte[allocatedSize];
        }

        public override byte Read(ushort addr)
        {
            return data[addr];
        }

        public override void Write(ushort addr, byte value)
        {
            data[addr] = value;
        }
    }
}
