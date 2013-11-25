using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class Statistics : Packet {
        public int Count;
        public string[] name;
        public int[] value;

        public Statistics(ref Minecraft mc) {
            Count = mc.nh.wSock.readVarInt();

            name = new string[Count];
            value = new int[Count];

            for (int i = 0; i < Count; i++) {
                name[i] = mc.nh.wSock.readString();
                value[i] = mc.nh.wSock.readVarInt();
            }
        }
    }
}
