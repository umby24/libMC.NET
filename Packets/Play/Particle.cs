using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class Particle : Packet {
        string particleName;
        float x, y, z, off_x, off_y, off_z, data;
        int number;

        public Particle(ref Minecraft mc) {
            particleName = mc.nh.wSock.readString();
            x = mc.nh.wSock.readFloat();
            y = mc.nh.wSock.readFloat();
            z = mc.nh.wSock.readFloat();
            off_x = mc.nh.wSock.readFloat();
            off_y = mc.nh.wSock.readFloat();
            off_z = mc.nh.wSock.readFloat();
            data = mc.nh.wSock.readFloat();
            number = mc.nh.wSock.readInt();
        }
    }
}
