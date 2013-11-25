using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    class clientSettings {
        public clientSettings(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(0x15);
            mc.nh.wSock.writeString("en_US");
            mc.nh.wSock.writeByte(0);
            mc.nh.wSock.writeByte(3);
            mc.nh.wSock.writeBool(true);
            mc.nh.wSock.writeByte(1);
            mc.nh.wSock.writeBool(false);
            mc.nh.wSock.Purge();
        }
    }
}
