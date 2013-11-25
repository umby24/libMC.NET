using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class windowProperty : Packet {
        public byte windowID;
        public short property, value;

        public windowProperty(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();
            property = mc.nh.wSock.readShort();
            value = mc.nh.wSock.readShort();

        }
    }
}
