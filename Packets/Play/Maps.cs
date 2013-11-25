using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class Maps : Packet {
        int itemDamage;
        short length;
        byte[] data;

        public Maps(ref Minecraft mc) { // -- No clue what this is honestly.
            itemDamage = mc.nh.wSock.readVarInt();
            length = mc.nh.wSock.readShort();
            data = mc.nh.wSock.readByteArray(length);
        }
    }
}
