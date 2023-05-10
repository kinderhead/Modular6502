using Modular6502;
using Modular6502.Devices;

namespace Tests
{
    [TestClass]
    public class MemoryTest
    {
        [TestMethod]
        public void TestWriteRead()
        {
            CPU cpu = Util.CreateBasic();
            cpu.Write(0x400, 4);
            Assert.AreEqual(4, cpu.Read(0x400));
        }

        [TestMethod]
        public void TestWriteReadWord()
        {
            CPU cpu = Util.CreateBasic();
            cpu.WriteWord(0x400, 0x3F4D);
            Assert.AreEqual(0x3F4D, cpu.ReadWord(0x400));
        }
    }
}