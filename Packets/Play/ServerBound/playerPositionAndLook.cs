using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play.ServerBound {
    public class playerPositionAndLook {
        public playerPositionAndLook(ref Minecraft mc) {
            mc.nh.wSock.writeVarInt(6);
            mc.nh.wSock.writeDouble(mc.ThisPlayer.location.x);
            mc.nh.wSock.writeDouble(mc.ThisPlayer.location.y - 1.620);
            mc.nh.wSock.writeDouble(mc.ThisPlayer.location.y);
            mc.nh.wSock.writeDouble(mc.ThisPlayer.location.z);
            mc.nh.wSock.writeFloat(mc.ThisPlayer.look[0]);
            mc.nh.wSock.writeFloat(mc.ThisPlayer.look[1]);
            mc.nh.wSock.writeBool(mc.ThisPlayer.onGround);
            mc.nh.wSock.Purge();
        }
    }
}
