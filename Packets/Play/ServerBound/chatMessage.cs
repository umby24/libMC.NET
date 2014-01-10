using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    public class ChatMessage { // -- Max Length: 100.
        public static void SendChat(Minecraft mc, string message) {
            mc.nh.wSock.writeVarInt(1);
            mc.nh.wSock.writeString(message);
            mc.nh.wSock.Purge();
        }

    }
}
