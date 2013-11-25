using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class updateScore : Packet {
        public string itemName, scoreName;
        public byte update;
        public int value;

        public updateScore(ref Minecraft mc) {
            itemName = mc.nh.wSock.readString();
            update = mc.nh.wSock.readByte();
            scoreName = mc.nh.wSock.readString();
            value = mc.nh.wSock.readInt();
        }
    }
}
