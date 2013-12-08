using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Login {
    class loginStart : Packet {
        public loginStart(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(0);
            mc.nh.wSock.writeString(mc.ClientName);
            mc.nh.wSock.Purge();
        }
    }
}
