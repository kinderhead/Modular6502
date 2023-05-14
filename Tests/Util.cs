using Modular6502.Devices;
using Modular6502;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class Util
    {
        public static readonly byte[] ROM = new byte[] { 4, 66, 123, 15, 55, 255, 54 };

        public static CPU CreateBasic()
        {
            return CreateWithROM(ROM);
        }

        public static CPU CreateWithROM(byte[] data)
        {
            byte[] dataWithVector = new byte[0x7FFF];
            for (int i = 0; i < data.Length; i++)
            {
                dataWithVector[i] = data[i];
            }
            dataWithVector[0xFFFC - 0x7FFF] = 0xFF;
            dataWithVector[0xFFFD - 0x7FFF] = 0x7F;
            var cpu = new CPU().Map(new RAM(), 0x7FFF).Map(new ROM(dataWithVector), 0x7FFF);
            cpu.Reset();
            cpu.SP = 0xFF;
            return cpu;
        }

        public static CPU CreateWithFile(string path)
        {
            return CreateWithROM(File.ReadAllBytes(path));
        }
    }
}
