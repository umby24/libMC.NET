using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    public class chatMessage { // -- Max Length: 100.
        public static void sendChat(ref Minecraft mc, string message) {
            mc.nh.wSock.writeVarInt(1);
            mc.nh.wSock.writeString(message);
            mc.nh.wSock.Purge();
        }

    }
}
