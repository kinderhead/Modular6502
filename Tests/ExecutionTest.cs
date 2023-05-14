using Modular6502;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class ExecutionTest
    {
        [TestMethod]
        public void TestReadNext()
        {
            CPU cpu = Util.CreateBasic();
            foreach (var i in Util.ROM)
            {
                Assert.AreEqual(i, cpu.ReadNext());
            }
            Assert.AreEqual(0x7FFF + Util.ROM.Length, cpu.PC);
        }

        [TestMethod]
        public void TestADCAndCommonAddressingModes()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x69, 1, 0x6D, 0x00, 0x20, 0x7D, 0xFF, 0x1F, 0x79, 0xFF, 0x1F, 0x65, 0x01, 0x75, 0x00, 0x61, 0x02, 0x71, 0x05 });
            cpu.X = 1;
            cpu.Y = 1;
            cpu.C = false;
            cpu.Write(0x2000, 1);
            cpu.Write(0x01, 1);
            cpu.WriteWord(0x05, 0x1FFF);
            cpu.WriteWord(0x03, 0x2000);

            for (int i = 0; i < 8; i++)
            {
                cpu.Step();
            }

            Assert.AreEqual(8, cpu.A);
        }

        [TestMethod]
        public void TestADCCarry()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x69, 200, 0x69, 56 });
            cpu.C = false;
            cpu.Step();
            cpu.Step();
            Assert.AreEqual(0, cpu.A);
            Assert.IsTrue(cpu.C);
        }

        [TestMethod]
        public void TestADCOverflow1()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x69, 1, 0x69, 1 });
            cpu.C = false;
            cpu.Step();
            cpu.Step();
            Assert.AreEqual(2, cpu.A);
            Assert.IsFalse(cpu.V);
        }

        [TestMethod]
        public void TestADCOverflow2()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x69, 127, 0x69, 1 });
            cpu.C = false;
            cpu.Step();
            cpu.Step();
            Assert.AreEqual(128, cpu.A);
            Assert.IsTrue(cpu.V);
        }

        [TestMethod]
        public void TestLDA()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA9, 52 });
            cpu.Step();
            Assert.AreEqual(52, cpu.A);
        }

        [TestMethod]
        public void TestLDAZero()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA9, 0 });
            cpu.Step();
            Assert.AreEqual(0, cpu.A);
            Assert.IsTrue(cpu.Z);
        }

        [TestMethod]
        public void TestLDANegative()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA9, 234 });
            cpu.Step();
            Assert.AreEqual(234, cpu.A);
            Assert.IsTrue(cpu.N);
        }

        [TestMethod]
        public void TestLDX()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA2, 52 });
            cpu.Step();
            Assert.AreEqual(52, cpu.X);
        }

        [TestMethod]
        public void TestLDXZero()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA2, 0 });
            cpu.Step();
            Assert.AreEqual(0, cpu.X);
            Assert.IsTrue(cpu.Z);
        }

        [TestMethod]
        public void TestLDXNegative()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA2, 234 });
            cpu.Step();
            Assert.AreEqual(234, cpu.X);
            Assert.IsTrue(cpu.N);
        }

        [TestMethod]
        public void TestLDXYIndexZeroPageAddressing()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xB6, 4 });
            cpu.Write(6, 52);
            cpu.Y = 2;
            cpu.Step();
            Assert.AreEqual(52, cpu.X);
        }

        [TestMethod]
        public void TestLDY()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA0, 52 });
            cpu.Step();
            Assert.AreEqual(52, cpu.Y);
        }

        [TestMethod]
        public void TestLDYZero()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA0, 0 });
            cpu.Step();
            Assert.AreEqual(0, cpu.Y);
            Assert.IsTrue(cpu.Z);
        }

        [TestMethod]
        public void TestLDYNegative()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA0, 234 });
            cpu.Step();
            Assert.AreEqual(234, cpu.Y);
            Assert.IsTrue(cpu.N);
        }

        [TestMethod]
        public void TestSTAAndCommonAddressingModes()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x8D, 0x00, 0x20, 0x9D, 0xFF, 0x1F, 0x99, 0xFF, 0x1F, 0x85, 0x01, 0x95, 0x00, 0x81, 0x01, 0x91, 0x05 });
            cpu.X = 1;
            cpu.Y = 1;
            cpu.WriteWord(0x02, 0x2000);
            cpu.WriteWord(0x05, 0x1FFF);
            for (int i = 0; i < 7; i++)
            {
                cpu.Step();
                if (cpu.Read(0x01) != i && cpu.Read(0x2000) != i) Assert.Fail($"Expected {i}");
                cpu.A++;
            }
        }

        [TestMethod]
        public void TestSTX()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x8E, 0x00, 0x20 });
            cpu.X = 64;
            cpu.Step();
            Assert.AreEqual(64, cpu.Read(0x2000));
        }

        [TestMethod]
        public void TestSTY()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x8C, 0x00, 0x20 });
            cpu.Y = 64;
            cpu.Step();
            Assert.AreEqual(64, cpu.Read(0x2000));
        }

        [TestMethod]
        public void TestTAX()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xAA });
            cpu.A = 6;
            cpu.Step();
            Assert.AreEqual(6, cpu.X);
        }

        [TestMethod]
        public void TestTAY()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xA8 });
            cpu.A = 6;
            cpu.Step();
            Assert.AreEqual(6, cpu.Y);
        }

        [TestMethod]
        public void TestTSX()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xBA });
            cpu.Step();
            Assert.AreEqual(0xFF, cpu.X);
        }

        [TestMethod]
        public void TestTXA()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x8A });
            cpu.X = 6;
            cpu.Step();
            Assert.AreEqual(6, cpu.A);
        }

        [TestMethod]
        public void TestTXS()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x9A });
            cpu.X = 6;
            cpu.Step();
            Assert.AreEqual(6, cpu.SP);
        }

        [TestMethod]
        public void TestTYA()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x98 });
            cpu.Y = 6;
            cpu.Step();
            Assert.AreEqual(6, cpu.A);
        }

        [TestMethod]
        public void TestPHAPLA()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x48, 0x48, 0x48, 0x68, 0x68, 0x68 });
            cpu.A = 15;
            cpu.Step();
            Assert.AreEqual(0xFE, cpu.SP);
            Assert.AreEqual(15, cpu.Read((ushort)(cpu.SP + 1 + 0x0100)));
            cpu.Step();
            Assert.AreEqual(0xFD, cpu.SP);
            Assert.AreEqual(15, cpu.Read((ushort)(cpu.SP + 1 + 0x0100)));
            cpu.Step();
            Assert.AreEqual(0xFC, cpu.SP);
            Assert.AreEqual(15, cpu.Read((ushort)(cpu.SP + 1 + 0x0100)));

            cpu.A = 0;
            cpu.Step();
            Assert.AreEqual(0xFD, cpu.SP);
            Assert.AreEqual(15, cpu.A);
            cpu.A = 0;
            cpu.Step();
            Assert.AreEqual(0xFE, cpu.SP);
            Assert.AreEqual(15, cpu.A);
            cpu.A = 0;
            cpu.Step();
            Assert.AreEqual(0xFF, cpu.SP);
            Assert.AreEqual(15, cpu.A);
        }

        [TestMethod]
        public void TestPHPPLP()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x08, 0x08, 0x08, 0x28 });
            cpu.Step();
            Assert.AreEqual(0xFE, cpu.SP);
            Assert.AreEqual(0b11110111, cpu.Read((ushort)(cpu.SP + 1 + 0x0100)));
            cpu.Step();
            Assert.AreEqual(0xFD, cpu.SP);
            Assert.AreEqual(0b11110111, cpu.Read((ushort)(cpu.SP + 1 + 0x0100)));
            cpu.Step();
            Assert.AreEqual(0xFC, cpu.SP);
            Assert.AreEqual(0b11110111, cpu.Read((ushort)(cpu.SP + 1 + 0x0100)));

            cpu.N = false;
            cpu.V = false;
            cpu.D = false;
            cpu.I = false;
            cpu.Z = false;
            cpu.C = false;
            cpu.Step();
            Assert.AreEqual(0xFD, cpu.SP);
            Assert.IsTrue(cpu.N);
            Assert.IsTrue(cpu.V);
            Assert.IsFalse(cpu.D);
            Assert.IsTrue(cpu.I);
            Assert.IsTrue(cpu.Z);
            Assert.IsTrue(cpu.C);
        }

        [TestMethod]
        public void TestASLAccumulator()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x0A, 0x0A });
            cpu.A = 0b01010101;
            cpu.Step();
            Assert.AreEqual(0b10101010, cpu.A);
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsFalse(cpu.C);
            cpu.Step();
            Assert.AreEqual(0b01010100, cpu.A);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
        }

        [TestMethod]
        public void TestASL()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x06, 0x00, 0x06, 0x00 });
            cpu.Write(0, 0b01010101);
            cpu.Step();
            Assert.AreEqual(0b10101010, cpu.Read(0));
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsFalse(cpu.C);
            cpu.Step();
            Assert.AreEqual(0b01010100, cpu.Read(0));
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
        }

        [TestMethod]
        public void TestLSRAccumulator()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x4A, 0x4A });
            cpu.A = 0b01010101;
            cpu.Step();
            Assert.AreEqual(0b00101010, cpu.A);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
            cpu.Step();
            Assert.AreEqual(0b00010101, cpu.A);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsFalse(cpu.C);
        }

        [TestMethod]
        public void TestLSR()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x46, 0x00, 0x46, 0x00 });
            cpu.Write(0, 0b01010101);
            cpu.Step();
            Assert.AreEqual(0b00101010, cpu.Read(0));
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
            cpu.Step();
            Assert.AreEqual(0b00010101, cpu.Read(0));
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsFalse(cpu.C);
        }

        [TestMethod]
        public void TestROLAccumulator()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x2A, 0x2A });
            cpu.A = 0b01010101;
            cpu.C = true;
            cpu.Step();
            Assert.AreEqual(0b10101011, cpu.A);
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsFalse(cpu.C);
            cpu.C = true;
            cpu.Step();
            Assert.AreEqual(0b01010111, cpu.A);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
        }

        [TestMethod]
        public void TestROL()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x26, 0x00, 0x26, 0x00 });
            cpu.C = true;
            cpu.Write(0, 0b01010101);
            cpu.Step();
            Assert.AreEqual(0b10101011, cpu.Read(0));
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsFalse(cpu.C);
            cpu.C = true;
            cpu.Step();
            Assert.AreEqual(0b01010111, cpu.Read(0));
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
        }

        [TestMethod]
        public void TestRORAccumulator()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x6A, 0x6A });
            cpu.A = 0b01010101;
            cpu.C = true;
            cpu.Step();
            Assert.AreEqual(0b10101010, cpu.A);
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
            cpu.Step();
            Assert.AreEqual(0b11010101, cpu.A);
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsFalse(cpu.C);
        }

        [TestMethod]
        public void TestROR()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x66, 0x00, 0x66, 0x00 });
            cpu.Write(0, 0b01010101);
            cpu.C = true;
            cpu.Step();
            Assert.AreEqual(0b10101010, cpu.Read(0));
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
            cpu.Step();
            Assert.AreEqual(0b11010101, cpu.Read(0));
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsFalse(cpu.C);
        }

        [TestMethod]
        public void TestAND()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x2D, 0x00, 0x20 });
            cpu.Write(0x2000, 0b00001111);
            cpu.A = 0xFF;
            cpu.Step();
            Assert.AreEqual(0b00001111, cpu.A);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestBIT()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x2C, 0x00, 0x20 });
            cpu.Write(0x2000, 0b11110000);
            cpu.A = 0b11011001;
            cpu.Step();
            Assert.IsTrue(cpu.N);
            Assert.IsTrue(cpu.V);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestEOR()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x4D, 0x00, 0x20 });
            cpu.Write(0x2000, 0b11110000);
            cpu.A = 0b11011001;
            cpu.Step();
            Assert.AreEqual(0b00101001, cpu.A);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestORA()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x0D, 0x00, 0x20 });
            cpu.Write(0x2000, 0b11110000);
            cpu.A = 0b11011001;
            cpu.Step();
            Assert.AreEqual(0b11111001, cpu.A);
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestCMP()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xC9, 15, 0xC9, 70, 0xC9, 69 });
            cpu.A = 69;
            cpu.Step();
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
            cpu.Step();
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.C);
            Assert.IsFalse(cpu.Z);
            cpu.Step();
            Assert.IsFalse(cpu.N);
            Assert.IsTrue(cpu.Z);
            Assert.IsTrue(cpu.C);
        }

        [TestMethod]
        public void TestCPX()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xE0, 15, 0xE0, 70, 0xE0, 69 });
            cpu.X = 69;
            cpu.Step();
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
            cpu.Step();
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.C);
            Assert.IsFalse(cpu.Z);
            cpu.Step();
            Assert.IsFalse(cpu.N);
            Assert.IsTrue(cpu.Z);
            Assert.IsTrue(cpu.C);
        }

        [TestMethod]
        public void TestCPY()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xC0, 15, 0xC0, 70, 0xC0, 69 });
            cpu.Y = 69;
            cpu.Step();
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
            cpu.Step();
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.C);
            Assert.IsFalse(cpu.Z);
            cpu.Step();
            Assert.IsFalse(cpu.N);
            Assert.IsTrue(cpu.Z);
            Assert.IsTrue(cpu.C);
        }

        [TestMethod]
        public void TestSBC()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xE9, 55, 0xE9, 56 });
            cpu.A = 110;
            cpu.Step();
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsTrue(cpu.C);
            Assert.AreEqual(55, cpu.A);
            cpu.C = false;
            cpu.Step();
            Assert.IsTrue(cpu.N);
            Assert.IsFalse(cpu.Z);
            Assert.IsFalse(cpu.C);
            Assert.AreEqual(-2, (sbyte)cpu.A);
        }

        [TestMethod]
        public void TestDEC()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xCE, 0x00, 0x20 });
            cpu.Write(0x2000, 5);
            cpu.Step();
            Assert.AreEqual(4, cpu.Read(0x2000));
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestDEX()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xCA });
            cpu.X = 5;
            cpu.Step();
            Assert.AreEqual(4, cpu.X);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestDEY()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x88 });
            cpu.Y = 5;
            cpu.Step();
            Assert.AreEqual(4, cpu.Y);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestINC()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xEE, 0x00, 0x20 });
            cpu.Write(0x2000, 5);
            cpu.Step();
            Assert.AreEqual(6, cpu.Read(0x2000));
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestINX()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xE8 });
            cpu.X = 5;
            cpu.Step();
            Assert.AreEqual(6, cpu.X);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestINY()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0xC8 });
            cpu.Y = 5;
            cpu.Step();
            Assert.AreEqual(6, cpu.Y);
            Assert.IsFalse(cpu.N);
            Assert.IsFalse(cpu.Z);
        }

        [TestMethod]
        public void TestJMP()
        {
            CPU cpu = Util.CreateWithROM(new byte[] { 0x6C, 0x00, 0x20 });
            cpu.WriteWord(0x2000, 0x6969);
            cpu.Step();
            Assert.AreEqual(0x6969, cpu.PC);
        }

        [TestMethod]
        public void TestJSRRTS()
        {
            CPU cpu = Util.CreateWithFile("jsr.rom");
            for (int i = 0; i < 4; i++)
            {
                cpu.Step();
            }
            Assert.AreEqual(0, cpu.A);
            Assert.AreEqual(0x40, cpu.X);
        }
    }
}
