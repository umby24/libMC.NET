using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Status {
    public class ClientPing : Packet {
        public ClientPing(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(1);
            mc.nh.wSock.writeLong(DateTime.Now.Ticks);
            mc.nh.wSock.Purge();
        }
    }
}
