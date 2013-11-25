using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class tabComplete : Packet {
        public int count;
        public string match;

        public tabComplete(ref Minecraft mc) {
            count = mc.nh.wSock.readVarInt();
            match = mc.nh.wSock.readString();

        }
    }
}
