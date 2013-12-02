using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    public class PlayerPosition {
        public PlayerPosition(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(4);
            mc.nh.wSock.writeDouble(mc.thisPlayer.vector[0]);
            mc.nh.wSock.writeDouble(mc.thisPlayer.vector[1]);
            mc.nh.wSock.writeDouble(mc.thisPlayer.vector[1] + 1.2);
            mc.nh.wSock.writeDouble(mc.thisPlayer.vector[2]);
            mc.nh.wSock.writeBool(mc.thisPlayer.onGround);
            mc.nh.wSock.Purge();
        }
    }
}
