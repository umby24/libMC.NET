using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;

namespace libMC.NET.Packets.Play {
    class spawnExpOrb : Packet {
        Entities.Entity expOrb;

        public spawnExpOrb(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readVarInt();
            int X = mc.nh.wSock.readInt();
            int Y = mc.nh.wSock.readInt();
            int Z = mc.nh.wSock.readInt();
            short count = mc.nh.wSock.readShort();

            expOrb = new Entities.Entity(ref mc, "ExpOrb");
            expOrb.Location = new Vector(X, Y, Z);
            expOrb.count = count;

            //TOOD: Add to tracked entities
        }
    }
}
