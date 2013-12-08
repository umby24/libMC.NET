using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Common {
    public interface Packet {
        int PacketID { get; }
        int State { get; }

        void Read(MinecraftServer mc);
        void Write(MinecraftServer mc);
    }
}
