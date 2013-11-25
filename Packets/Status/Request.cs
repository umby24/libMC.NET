using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Status {
    public class Request : Packet {
        public Request(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(0);
            mc.nh.wSock.Purge();
        }
    }
}
