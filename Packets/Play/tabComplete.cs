using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class TabComplete : Packet {
        public int count;
        public string match;

        public TabComplete(ref Minecraft mc) {
            count = mc.nh.wSock.readVarInt();
            match = mc.nh.wSock.readString();

        }
    }
}
