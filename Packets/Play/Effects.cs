using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class Effects : Packet {
        public int Effect_ID, X, Z, Data;
        public byte Y;
        public bool DisableRelVolume;

        public Effects(ref Minecraft mc) {
            Effect_ID = mc.nh.wSock.readInt();
            X = mc.nh.wSock.readInt();
            Y = mc.nh.wSock.readByte();
            Z = mc.nh.wSock.readInt();
            Data = mc.nh.wSock.readInt();
            DisableRelVolume = mc.nh.wSock.readBool();

            //TODO: Implement this, Pull requests welcome and are encouraged for parsing the IDs and raising an event for this.
        }
    }
}
