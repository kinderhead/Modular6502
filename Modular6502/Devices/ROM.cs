using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modular6502.Devices
{
    public class ROM : Device
    {
        protected byte[] data;

        public ROM(byte[] data)
        {
            this.data = data;
        }

        public override byte Read(ushort addr)
        {
            if (addr >= data.Length) return 0;
            return data[addr];
        }
    }
}
