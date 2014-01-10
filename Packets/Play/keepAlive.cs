using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class KeepAlive : Packet {
        public KeepAlive(ref Minecraft mc) {
            int id = mc.nh.wSock.readInt();

            // -- Respond
            mc.nh.wSock.writeVarInt(0);
            mc.nh.wSock.writeInt(id);
            mc.nh.wSock.Purge();
        }
    }
}
