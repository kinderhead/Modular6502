using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modular6502
{
    public abstract class Device
    {
        public virtual ushort MaxAddresses { get => 0xFFFF; }
        public virtual ushort FixedSize { get => 0; }

        public ushort AllocatedSize { get; protected set; }

        public Device()
        {
            
        }

        public virtual void Init(ushort allocatedSize)
        {
            if (allocatedSize > MaxAddresses || (FixedSize != 0 && FixedSize != allocatedSize))
            {
                throw new ArgumentOutOfRangeException(nameof(allocatedSize), $"Device does not support {allocatedSize} address(es)");
            }

            AllocatedSize = allocatedSize;
        }

        public virtual byte Read(ushort addr)
        {
            Console.WriteLine($"Warning: Device {GetType().FullName} does not support read operations");
            return 0;
        }

        public virtual void Write(ushort addr, byte value)
        {
            Console.WriteLine($"Warning: Device {GetType().FullName} does not support write operations");
        }
    }
}
