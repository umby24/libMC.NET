using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class Explosion : Packet {
        public float X, Y, Z, Radius, Motion_X, Motion_Y, Motion_Z;
        public int recordCount;
        public byte[] records;

        public Explosion(ref Minecraft mc) {
            X = mc.nh.wSock.readFloat();
            Y = mc.nh.wSock.readFloat();
            Z = mc.nh.wSock.readFloat();

            Radius = mc.nh.wSock.readFloat();
            recordCount = mc.nh.wSock.readInt();
            records = mc.nh.wSock.readByteArray(recordCount * 3);
            Motion_X = mc.nh.wSock.readFloat();
            Motion_Y = mc.nh.wSock.readFloat();
            Motion_Z = mc.nh.wSock.readFloat();

            mc.RaiseExplosion(X, Y, Z);
        }
    }
}
