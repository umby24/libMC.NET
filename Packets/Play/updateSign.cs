using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class updateSign : Packet {
        public int X, Z;
        public short Y;
        public string Line_1, Line_2, Line_3, Line_4;

        public updateSign(ref Minecraft mc) {
            X = mc.nh.wSock.readInt();
            Y = mc.nh.wSock.readShort();
            Z = mc.nh.wSock.readInt();

            Line_1 = mc.nh.wSock.readString();
            Line_2 = mc.nh.wSock.readString();
            Line_3 = mc.nh.wSock.readString();
            Line_4 = mc.nh.wSock.readString();

            mc.raiseDebug(this, "Sign updated.");
        }
    }
}
