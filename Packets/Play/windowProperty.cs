using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class WindowProperty : Packet {
        public byte windowID;
        public short property, value;

        public WindowProperty(ref Minecraft mc) {
            windowID = mc.nh.wSock.readByte();
            property = mc.nh.wSock.readShort();
            value = mc.nh.wSock.readShort();

        }
    }
}
