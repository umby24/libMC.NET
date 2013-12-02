using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    public class playerPositionAndLook {
        public playerPositionAndLook(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(6);
            mc.nh.wSock.writeDouble(mc.thisPlayer.vector[0]);
            mc.nh.wSock.writeDouble(mc.thisPlayer.vector[1]);
            mc.nh.wSock.writeDouble(mc.thisPlayer.vector[1] + 1.2);
            mc.nh.wSock.writeDouble(mc.thisPlayer.vector[2]);
            mc.nh.wSock.writeFloat(mc.thisPlayer.look[0]);
            mc.nh.wSock.writeFloat(mc.thisPlayer.look[1]);
            mc.nh.wSock.writeBool(mc.thisPlayer.onGround);
            mc.nh.wSock.Purge();
        }
    }
}
