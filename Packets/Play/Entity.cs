using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class Entity : Packet {
        public Entity(ref Minecraft mc) {
            mc.nh.wSock.readInt(); // -- Just bypass this, our other packets are already safe enough to not need initilization.
        }
    }
}
