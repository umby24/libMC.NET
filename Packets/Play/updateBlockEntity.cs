using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class UpdateBlockEntity : Packet {
        public int X, Z;
        public short Y, DataLength;
        public byte action;
        public byte[] NBT;

        public UpdateBlockEntity(ref Minecraft mc) {
            X = mc.nh.wSock.readInt();
            Y = mc.nh.wSock.readShort();
            Z = mc.nh.wSock.readInt();
            action = mc.nh.wSock.readByte();
            DataLength = mc.nh.wSock.readShort();
            NBT = mc.nh.wSock.readByteArray(DataLength);

        }
    }
}
