using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    public class clientStatus {
        public clientStatus(ref Minecraft mc, byte action) {
            mc.nh.wSock.writeVarInt(22);
            mc.nh.wSock.writeByte(action);
            mc.nh.wSock.Purge();
        }
    }
}
