using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    public class Player {
        public Player(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(3);
            mc.nh.wSock.writeBool(mc.thisPlayer.onGround);
            mc.nh.wSock.Purge();
        }
    }
}
